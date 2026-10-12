using FortRise;
using HarmonyLib;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// Bolt arrows only seek-turn once.
/// </summary>
public class NerfedBoltArrows : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(BoltArrow), "TryTurn", [typeof(float)]),
            prefix: new HarmonyMethod(BoltArrow_TryTurn_Prefix)
        );
    }

    private static bool BoltArrow_TryTurn_Prefix(BoltArrow __instance, ref bool __result)
    {
        if (__instance.Turns >= 1 && Variants.NerfedBoltArrows.IsActive(__instance.PlayerIndex))
        {
            // After 1 successful seek-turn, pretend turning failed forever.
            __result = false;
            return false;
        }

        return true;
    }
}