using System.Runtime.CompilerServices;
using FortRise;
using HarmonyLib;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// If an archer who's already dead gets a kill, their corpse auto-revives.
/// The round won't end while a revive is in progress.
/// </summary>
public class KillsRevive : IHookable
{
    private static readonly ConditionalWeakTable<PlayerCorpse, TeamReviver> revivers = new();

    private static readonly AccessTools.FieldRef<LastManStandingRoundLogic, RoundEndCounter> roundEndCounter =
        AccessTools.FieldRefAccess<LastManStandingRoundLogic, RoundEndCounter>("roundEndCounter");

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Die), [typeof(DeathCause), typeof(int), typeof(bool), typeof(bool)]),
            prefix: new HarmonyMethod(Player_Die_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Added)),
            postfix: new HarmonyMethod(Player_Added_Postfix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TeamReviver), nameof(TeamReviver.ReviveUpdate)),
            prefix: new HarmonyMethod(TeamReviver_ReviveUpdate_Prefix)
        );
    }

    private static bool IsActiveIn(Level level)
    {
        return level.Session.MatchSettings.Mode == Modes.LastManStanding && Variants.KillsRevive.IsActive();
    }

    private static void Player_Die_Prefix(Player __instance, int killerIndex)
    {
        var level = __instance.Level;
        if (__instance.Dead || killerIndex == -1 || !IsActiveIn(level) || level.IsPlayerAlive(killerIndex))
        {
            return;
        }

        if (level.GetPlayerOrCorpse(killerIndex) is PlayerCorpse killerCorpse && !revivers.TryGetValue(killerCorpse, out _))
        {
            var reviver = new TeamReviver(killerCorpse, TeamReviver.Modes.Quest) { AutoRevive = true };
            level.Add(reviver);
            revivers.Add(killerCorpse, reviver);
        }
    }

    private static void Player_Added_Postfix(Player __instance)
    {
        var level = __instance.Level;
        if (!level.Ending || !IsActiveIn(level))
        {
            return;
        }

        // If a player revives while the round is ending, cancel the round end and reset the counter.
        if (level.Session.RoundLogic is LastManStandingRoundLogic roundLogic && !roundLogic.FFACheckForAllButOneDead())
        {
            level.Ending = false;
            roundEndCounter(roundLogic).Reset();
        }
    }

    private static void TeamReviver_ReviveUpdate_Prefix(TeamReviver __instance)
    {
        var level = __instance.Level;
        if (__instance.Finished || !level.Ending || !IsActiveIn(level))
        {
            return;
        }

        if (level.Session.RoundLogic is LastManStandingRoundLogic roundLogic)
        {
            roundEndCounter(roundLogic).Reset();
        }
    }
}
