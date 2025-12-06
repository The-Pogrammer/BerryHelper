using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Celeste.Mod.BerryHelper
{
    public abstract class CustomBerry : Entity
    {
        protected virtual string SpriteRoot => "TestBerry/";
        protected virtual string IdleAnim => "idle/normal";
        protected virtual string GhostIdleAnim => "ghost/idle/idle";
        protected virtual string CollectAnim => "collect/normal";
        protected virtual string GhostCollectAnim => "ghost/collect/normal";
        protected virtual string DisappearAnim => null;
        protected virtual string RespawnAnim => null;

        protected virtual float WobbleSpeed => 4f;
        protected virtual float WobbleAmplitude => 2f;
        protected virtual float FollowDelay => 0.3f;
        protected virtual float CollectGroundTime => 0.15f;

        private bool wasFollowing = false;

        public EntityID ID;
        public Follower Follower;

        protected Sprite sprite;
        protected BloomPoint bloom;
        protected VertexLight light;

        protected Wiggler wiggler;
        protected Wiggler rotateWiggler;

        protected Tween lightTween;

        protected bool collected = false;
        protected float wobble;
        protected float collectTimer;

        private bool isGhostBerry;

        private static readonly ParticleType P_TreeGlow = new ParticleType(Strawberry.P_Glow);
        private float effectTimer = 0f;
        protected virtual float EffectIntervalMin => 1.5f;
        protected virtual float EffectIntervalMax => 3f;


        protected bool disappeared = false;

        protected virtual bool RespawnOnTransition => true;
        protected virtual bool RespawnOnDeath => true;
        protected virtual float RespawnAfterSeconds => -1f;

        protected virtual Vector2 SpriteOffset => new Vector2(4, 3);
        protected virtual Vector2 SpriteOriginOffset => Vector2.Zero;


        private float respawnTimer = 0f;

        private Collider savedCollider;
        private PlayerCollider playerCollider;


        public CustomBerry(EntityData data, Vector2 offset, EntityID id) : base()
        {
            ID = id;
            Position = data.Position + offset;

            isGhostBerry = SaveData.Instance.CheckStrawberry(ID);

            Depth = -100;

            Collider = new Hitbox(14f, 14f, -7f, -13f);
            savedCollider = Collider;

            playerCollider = new PlayerCollider(OnPlayer);
            Add(playerCollider);

            Add(new MirrorReflection());

            Add(Follower = new Follower(ID, null, OnLoseLeader));
            Follower.FollowDelay = FollowDelay;
        }

        protected virtual Sprite CreateSprite()
        {
            Sprite spr = new Sprite(GFX.Game, SpriteRoot);

            spr.AddLoop("idle", IdleAnim, 0.08f);
            spr.Add("collect", CollectAnim, 0.08f);
            spr.AddLoop("ghostidle", GhostIdleAnim, 0.08f);
            spr.Add("ghostcollect", GhostCollectAnim, 0.08f);

            if (!string.IsNullOrEmpty(DisappearAnim))
                spr.Add("disappear", DisappearAnim, 0.08f);

            if (!string.IsNullOrEmpty(RespawnAnim))
                spr.Add("respawn", RespawnAnim, 0.08f);

            spr.Origin = new Vector2(spr.Width / 2f, spr.Height) + SpriteOriginOffset;

            spr.Position = Vector2.Zero;

            spr.Play(isGhostBerry ? "ghostidle" : "idle");

            return spr;
        }


        public override void Added(Scene scene)
        {
            base.Added(scene);

            effectTimer = Calc.Random.Range(EffectIntervalMin, EffectIntervalMax);

            sprite = CreateSprite();
            sprite.Color = Color.White * 0.8f;
            Add(sprite);

            Add(wiggler = Wiggler.Create(0.4f, 4f, v => {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));

            Add(rotateWiggler = Wiggler.Create(0.5f, 4f, v => {
                sprite.Rotation = v * 30f * Calc.DegToRad;
            }));

            Add(bloom = new BloomPoint(0.75f, 12f));

            Add(light = new VertexLight(Color.White, 1f, 16, 24));

            Add(lightTween = light.CreatePulseTween());

            if (scene is Level level && level.Session.BloomBaseAdd > 0.1f)
                bloom.Alpha *= 0.5f;
        }

        private int FollowIndex => Follower != null ? Follower.FollowIndex : -1;

        public override void Update()
        {
            base.Update();

            if (disappeared)
            {
                if (RespawnAfterSeconds > 0)
                {
                    respawnTimer -= Engine.DeltaTime;
                    if (respawnTimer <= 0f)
                        Respawn();
                }

                return;
            }

            wobble += Engine.DeltaTime * WobbleSpeed;
            float wobbleOffset = (float)Math.Sin(wobble) * WobbleAmplitude;

            Vector2 finalOffset = new Vector2(SpriteOffset.X, wobbleOffset + SpriteOffset.Y);
            sprite.Position = finalOffset;
            Vector2 OtherOffset = Vector2.UnitY * -6;
            bloom.Position = new Vector2(0, wobbleOffset) + OtherOffset;
            light.Position = new Vector2(0, wobbleOffset) + OtherOffset;

            effectTimer -= Engine.DeltaTime;
            if (effectTimer <= 0f)
            {
                if (SceneAs<Level>() is Level level)
                {
                    level.ParticlesFG.Emit(P_TreeGlow, 1, Position + sprite.Position, Vector2.One * 4f);

                    Audio.Play("event:/game/general/strawberry_pulse", Position + OtherOffset);
                    level.Displacement.AddBurst(Position + OtherOffset, 0.6f, 4f, 28f, 0.2f);
                }

                effectTimer = Calc.Random.Range(EffectIntervalMin, EffectIntervalMax);
            }

            bool isFollowing = (Follower != null && Follower.Leader != null);
            Player leaderPlayer = isFollowing ? (Follower.Leader.Entity as Player) : null;

            if (isFollowing && !wasFollowing)
                OnStartFollowing(leaderPlayer);
            else if (!isFollowing && wasFollowing)
                OnStopFollowing();

            wasFollowing = isFollowing;

            int followIndex = FollowIndex;

            if (!collected)
            {
                if (Follower.Leader != null && Follower.DelayTimer <= 0f && followIndex == 0)
                {
                    Player player = leaderPlayer;
                    bool shouldCollect = false;

                    if (player != null && player.Scene != null && !player.StrawberriesBlocked)
                        if (player.OnSafeGround)
                            shouldCollect = true;

                    if (shouldCollect)
                    {
                        collectTimer += Engine.DeltaTime;
                        if (collectTimer > CollectGroundTime)
                            Collect(player);
                    }
                    else
                    {
                        collectTimer = Math.Min(collectTimer, 0f);
                    }
                }
                else
                {
                    if (followIndex > 0)
                        collectTimer = -CollectGroundTime;
                    else
                        collectTimer = Math.Min(collectTimer, 0f);
                }
            }
        }
    


        protected virtual void OnPlayer(Player player)
        {
            if (collected || Follower.Leader != null)
                return;

            Audio.Play(isGhostBerry ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);

            player.Leader.GainFollower(Follower);
            wiggler?.Start();
            Depth = -1000000;
        }

        protected virtual void Collect(Player player)
        {
            if (collected)
                return;

            collected = true;
            int collectIndex = 0;

            if (Follower.Leader != null)
            {
                Player leader = Follower.Leader.Entity as Player;
                if (leader != null)
                {
                    collectIndex = leader.StrawberryCollectIndex++;
                    leader.StrawberryCollectResetTimer = 2.5f;
                    Follower.Leader.LoseFollower(Follower);
                }
            }

            SaveData.Instance.AddStrawberry(ID, golden: false);

            Level level = Scene as Level;
            level.Session.Strawberries.Add(ID);
            level.Session.DoNotLoad.Add(ID);
            level.Session.UpdateLevelStartDashes();

            Add(new Coroutine(CollectRoutine(collectIndex)));
        }

        protected virtual IEnumerator CollectRoutine(int collectIndex)
        {
            Tag = Tags.TransitionUpdate;
            Depth = -2000010;

            Audio.Play("event:/game/general/strawberry_get", Position, "count", collectIndex);

            sprite.Play(isGhostBerry || SaveData.Instance.CheckStrawberry(ID) ? "ghostcollect" : "collect");

            while (sprite.Animating)
                yield return null;

            Scene.Add(new StrawberryPoints(Position, isGhostBerry, collectIndex, false));
            RemoveSelf();
        }

        protected virtual void OnLoseLeader()
        {
            if (collected)
                return;

            Vector2 start = Position;

            Alarm.Set(this, 0.1f, () => {
                Vector2 from = Position;
                Vector2 to = start;

                Tween back = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 0.4f, start: true);
                back.OnUpdate = t => {
                    Position = Vector2.Lerp(from, to, t.Eased);
                };
                Add(back);
            });
        }
        protected virtual void Disappear()
        {
            if (disappeared)
                return;

            disappeared = true;

            playerCollider.Active = false;
            Collider = null;

            Follower.Leader?.LoseFollower(Follower);

            Depth = 10000;

            respawnTimer = RespawnAfterSeconds > 0 ? RespawnAfterSeconds : 0f;

            Add(new Coroutine(DisappearAnimation()));
        }


        protected virtual void Respawn()
        {
            if (!disappeared)
                return;

            disappeared = false;

            sprite.Visible = true;
            light.Visible = true;
            bloom.Visible = true;

            Collider = savedCollider;
            playerCollider.Active = true;

            Depth = -100;

            Add(new Coroutine(RespawnAnimation()));
        }
        protected virtual void OnStartFollowing(Player player) { }

        protected virtual void OnStopFollowing() { }

        protected virtual IEnumerator DisappearAnimation()
        {
            if (!string.IsNullOrEmpty(DisappearAnim))
            {
                sprite.Play("disappear");
                while (sprite.Animating)
                    yield return null;
            }
            sprite.Visible = false;
            bloom.Visible = false;
            light.Visible = false;
        }

        protected virtual IEnumerator RespawnAnimation()
        {
            sprite.Visible = true;
            bloom.Visible = true;
            light.Visible = true;
            if (!string.IsNullOrEmpty(RespawnAnim))
            {
                sprite.Play("respawn");
                while (sprite.Animating)
                    yield return null;
            }
        }
    }
}
