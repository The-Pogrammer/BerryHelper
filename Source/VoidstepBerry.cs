using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BerryHelper
{
    [CustomEntity("BerryHelper/VoidstepBerry")]
    public class VoidstepBerry : CustomBerry
    {
        public VoidstepBerry(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset, id)
        {
        }

        public override void Update()
        {
            base.Update();

            // Do nothing if already gone or collected
            if (disappeared || collected)
                return;

            // If the berry is following the player, don't disappear (optional — remove if undesired)
            if (Follower?.Leader != null)
                return;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
                return;

            // Detect left or right inputs
            int xInput = Input.MoveX.Value;     // -1, 0, or +1 depending on key input

            if (xInput != 0)
            {
                Disappear();
            }
        }
    }
}
