using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BerryHelper
{

    [CustomEntity("BerryHelper/TreeBerry")]
    public class TreeBerry : CustomBerry
    {
        public TreeBerry(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset, id)
        {
        }
        private BerryPlayerModifier modifier;
        protected override string SpriteRoot => "TreeBerry/";

        protected override string IdleAnim => "idle/normal";
        protected override string CollectAnim => "collect/normal";

        protected override string GhostIdleAnim => "ghost/idle/idle";
        protected override string GhostCollectAnim => "ghost/collect/normal";

        protected override void OnStartFollowing(Player player)
        {
            base.OnStartFollowing(player);

            if (player != null && modifier == null)
            {
                modifier = new BerryPlayerModifier();
                player.Add(modifier);
            }
        }

        protected override void OnStopFollowing()
        {
            base.OnStopFollowing();

            if (modifier != null)
            {
                Player player = SceneAs<Level>()?.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    player.Remove(modifier);
                }
                modifier = null;
            }
        }

        protected override void Collect(Player player)
        {
            base.Collect(player);

            if (modifier != null)
            {
                modifier.ActiveEffect = false;
                modifier = null;
            }
        }

    }
}
