using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using MonoMod.Utils;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// In team matches, a killed archer's corpse joins the killer's team, so a Team Revive brings them back
/// fighting for the other side.
/// </summary>
public class RedRover : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(PlayerCorpse), [typeof(Player), typeof(int)]),
            postfix: new HarmonyMethod(PlayerCorpse_ctor_Postfix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TeamReviver), nameof(TeamReviver.ReviveUpdate)),
            prefix: new HarmonyMethod(TeamReviver_ReviveUpdate_Prefix)
        );
    }

    private static void PlayerCorpse_ctor_Postfix(PlayerCorpse __instance, Player player, int killerIndex)
    {
        var teams = player.Level.Session.MatchSettings.Teams;
        if (killerIndex == -1 || teams == null || !Variants.RedRover.IsActive())
        {
            return;
        }

        // The last archer on a team can't be converted.
        if (__instance.Allegiance == Allegiance.Neutral || teams.GetAmountOfPlayersOfAllegiance(__instance.Allegiance) <= 1)
        {
            return;
        }

        var killerTeam = teams[killerIndex];
        __instance.Allegiance = killerTeam;
        DynamicData.For(__instance).Set("TeamColor", killerTeam);

        // This rewards "from-the-grave" kills, skipping the revival step.
        if (!player.Level.IsPlayerAlive(killerIndex))
        {
            // Make the team change persistent.
            teams[player.PlayerIndex] = killerTeam;
        }
    }

    // Once revived, the archer stays on their new team for the rest of the match.
    private static void TeamReviver_ReviveUpdate_Prefix(TeamReviver __instance)
    {
        if (!__instance.Finished || !Variants.RedRover.IsActive())
        {
            return;
        }

        var teams = __instance.Level.Session.MatchSettings.Teams;
        var corpse = __instance.Corpse;
        if (teams != null && teams[corpse.PlayerIndex] != corpse.TeamColor)
        {
            AdditionalVariantsModule.Instance.Logger.LogInformation("Red Rover: player {Player} {From} => {To}", corpse.PlayerIndex, teams[corpse.PlayerIndex], corpse.TeamColor);
            // Make the team change persistent.
            teams[corpse.PlayerIndex] = corpse.TeamColor;
        }
    }
}
