using System.Collections;
using FortRise;
using HarmonyLib;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// When a ghost dies, it respawns at its corpse.
/// If the corpse is gone, it will no longer respawn.
/// </summary>
public class GhostsRespawn : IHookable
{
    private const int GhostDeadState = 3;

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(PlayerGhost), [typeof(PlayerCorpse)]),
            postfix: new HarmonyMethod(PlayerGhost_ctor_Postfix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), nameof(PlayerGhost.Die)),
            prefix: new HarmonyMethod(PlayerGhost_Die_Prefix)
        );
    }

    private static void PlayerGhost_ctor_Postfix(PlayerGhost __instance, PlayerCorpse corpse)
    {
        GhostData.Get(__instance).OwnerCorpse = corpse;
    }

    private static void PlayerGhost_Die_Prefix(PlayerGhost __instance)
    {
        int playerIndex = __instance.PlayerIndex;
        var corpse = GhostData.Get(__instance).OwnerCorpse;
        if (__instance.State == GhostDeadState || corpse?.Scene == null || playerIndex == -1)
        {
            return;
        }

        if (__instance.Level.Session.MatchSettings.Variants!.ReturnAsGhosts[playerIndex] && Variants.GhostsRespawn.IsActive(playerIndex))
        {
            var corpseData = DynamicData.For(corpse);
            var spawnSequence = corpseData.Invoke<IEnumerator>("GhostSpawnSequence")!;
            corpseData.Set("ghostCoroutine", new Coroutine(spawnSequence));
        }
    }
}
