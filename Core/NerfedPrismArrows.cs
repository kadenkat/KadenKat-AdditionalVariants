using FortRise;
using HarmonyLib;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// Prism arrows don't destroy corpses, so the corpses can still be used for revives, explosions, or spawning ghosts.
/// This is mostly useful when arrow randomizers are enabled, so when prism arrows appear they don't ruin the fun by
/// destroying corpses.
/// </summary>
public class NerfedPrismArrows : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerCorpse), "CanDoPrismHit"),
            prefix: new HarmonyMethod(PlayerCorpse_CanDoPrismHit_Prefix)
        );
    }

    private static bool PlayerCorpse_CanDoPrismHit_Prefix(Arrow arrow, ref bool __result)
    {
        if (Variants.NerfedPrismArrows.IsActive(arrow.PlayerIndex))
        {
            __result = false;
            return false;
        }
        return true;
    }
}
