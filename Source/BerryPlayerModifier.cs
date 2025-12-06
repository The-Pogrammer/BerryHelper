
using Celeste;
using Celeste.Mod;
using Celeste.Mod.BerryHelper;
using Monocle;

public class BerryPlayerModifier : Component
{
    public bool ActiveEffect = true;

    public BerryPlayerModifier() : base(active: true, visible: false) { }

    public override void Update()
    {
        base.Update();

        if (!ActiveEffect)
            return;

        Player player = Entity as Player;
        if (player == null)
            return;

        // Disable dash completely
        player.Dashes = 0;

        // Infinite stamina (110f is the game's hard-coded max)
        player.Stamina = 110f;

        // Optional: ensure climb stamina stays full too
        player.Stamina = Player.ClimbMaxStamina;   // makes climbing consistent
    }
}
