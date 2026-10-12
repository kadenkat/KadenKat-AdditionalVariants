using FortRise;
using HarmonyLib;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// Ghosts can only be affected by toy arrows. Everything else passes through.
/// </summary>
public class ThreeSpookyFiveMe : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), nameof(PlayerGhost.OnArrowHit)),
            prefix: new HarmonyMethod(PlayerGhost_OnArrowHit_Prefix)
        );
    }

    private static bool PlayerGhost_OnArrowHit_Prefix(PlayerGhost __instance, Arrow arrow, ref bool __result)
    {
        if (arrow.ArrowType != ArrowTypes.Toy && Variants.ThreeSpookyFiveMe.IsActive(__instance.PlayerIndex))
        {
            __result = false;
            return false;
        }
        return true;
    }
}
