using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>Toy arrow knockback is twice as strong.</summary>
public class SuperToyArrows : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.OnSqueakyBounce)),
            prefix: new HarmonyMethod(Player_OnSqueakyBounce_Prefix)
        );
    }

    private static bool Player_OnSqueakyBounce_Prefix(Player __instance, Arrow arrow, Vector2 normal)
    {
        if (!Variants.SuperToyArrows.IsActive(arrow.PlayerIndex))
        {
            return true;
        }

        __instance.State = Player.PlayerStates.Normal;
        __instance.Speed = normal * 10f;
        return false;
    }
}
