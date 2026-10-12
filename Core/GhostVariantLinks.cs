using FortRise;
using HarmonyLib;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// Allows Trigger Corpses and Return As Ghosts be on simultaneously.
/// </summary>
public class GhostVariantLinks : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(MatchVariants), [typeof(bool)]),
            postfix: new HarmonyMethod(MatchVariants_ctor_Postfix)
        );
    }

    private static void MatchVariants_ctor_Postfix(MatchVariants __instance)
    {
        __instance.TriggerCorpses.Links?.Remove(__instance.ReturnAsGhosts);
        __instance.ReturnAsGhosts.Links?.Remove(__instance.TriggerCorpses);
    }
}
