using FortRise;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace ExampleMod;

public class KadenKatVariants : Mod
{
    public static KadenKatVariants Instance = null!;

    public KadenKatVariants(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;

        RegisterWeakBoltArrowsVariant();
        RegisterRedRoverVariant();
        RegisterKillsReviveVariant();

        PatchTeamReviver();
		AddPressStartTextPatch();

        // This is where you register a lot of features such as custom arrows, variants, pickups, etc..
        // use context.Registry for adding new feature to the game.
        // This is also where you hook methods from the game.
        // use context.Harmony for hooks.
    }

    private static void PatchPlayerCorpse()
    {
		Instance.Context.Harmony.Patch(
            AccessTools.Constructor(typeof(PlayerCorpse), new[] { typeof(Player), typeof(int) }),
            postfix: new HarmonyMethod(typeof(KadenKatVariants), nameof(PlayerCorpse_Ctor_Postfix))
        );
	}

	private static void RegisterWeakBoltArrowsVariant()
    {
        ISubtextureEntry? subtextureEntry = Instance.Context.Registry.Subtextures.GetTexture("variants/startWithBoltArrows", SubtextureAtlasDestination.MenuAtlas);

        if (subtextureEntry != default)
        {
            VariantConfiguration weakBoltArrows = new VariantConfiguration
            {
                Title = "Weak Bolt Arrows",
                Description = "Bolt arrows aren't as thirsty.",
                Icon = subtextureEntry,
            };

            Instance.Context.Registry.Variants.RegisterVariant("Weak Bolt Arrows", weakBoltArrows);

            Instance.Context.Harmony.Patch(
                AccessTools.DeclaredMethod(typeof(BoltArrow), "TryTurn", new[] { typeof(float) }),
                prefix: new HarmonyMethod(typeof(KadenKatVariants), nameof(BoltArrow_TryTurn_Prefix))
            );
        }
    }

    private static void PatchTeamReviver()
    {
		Instance.Context.Harmony.Patch(
			AccessTools.DeclaredMethod(typeof(TeamReviver), "ReviveUpdate"),
			prefix: new HarmonyMethod(typeof(KadenKatVariants), nameof(TeamReviver_ReviveUpdate_Prefix))
		);
	}

	private static void AddPressStartTextPatch()
    {
        var pressStartText = AccessTools.TypeByName("TowerFall.PressStartText")
            ?? throw new System.Exception("Type not found: TowerFall.PressStartText");

        Instance.Context.Harmony.Patch(
            AccessTools.DeclaredMethod(pressStartText, "Render"),
            prefix: new HarmonyMethod(typeof(KadenKatVariants), nameof(PressStartText_Render_Prefix))
        );
    }

    private static void RegisterRedRoverVariant()
    {
        ISubtextureEntry? subtextureEntry = Instance.Context.Registry.Subtextures.GetTexture("variants/startWithBoltArrows", SubtextureAtlasDestination.MenuAtlas);

        if (subtextureEntry != default)
        {
            VariantConfiguration variant = new VariantConfiguration
            {
                Title = "Red Rover",
                Description = "Red rover, red rover, come on over.",
                Icon = subtextureEntry,
            };

            Instance.Context.Registry.Variants.RegisterVariant("Red Rover", variant);
        }
    }

	private static void RegisterKillsReviveVariant()
	{
		ISubtextureEntry? subtextureEntry = Instance.Context.Registry.Subtextures.GetTexture("variants/startWithBoltArrows", SubtextureAtlasDestination.MenuAtlas);

		if (subtextureEntry != default)
		{
			VariantConfiguration variant = new VariantConfiguration
			{
				Title = "Kills Revive",
				Description = "Back for blood, from blood.",
				Icon = subtextureEntry,
			};

			Instance.Context.Registry.Variants.RegisterVariant("Kills Revive", variant);
		}
	}

	// Postfix runs after the constructor body
	private static void PlayerCorpse_Ctor_Postfix(PlayerCorpse __instance, Player player, int killerIndex)
	{
		var teams = player.Level.Session.MatchSettings.Teams;

		if (killerIndex == -1 || teams == null)
			return;

		// Your types/settings; keep as-is from your mod
		int teamSize = teams.GetAmountOfPlayersOfAllegiance(__instance.Allegiance);

		if (Instance.Context.Registry.Variants.GetVariant("Red Rover")!.IsActive()
			&& teamSize > 1
			&& __instance.Allegiance != Allegiance.Neutral)
		{
			var newTeam = teams[killerIndex];

			// set allegiance
			__instance.Allegiance = newTeam;

			// try to set TeamColor backing storage if it exists
			var t = __instance.GetType();

			var teamColorField =
				AccessTools.Field(t, "teamColor") ??
				AccessTools.Field(t, "<TeamColor>k__BackingField");

			if (teamColorField != null)
				teamColorField.SetValue(__instance, newTeam);

			if (!player.Level.Session.CurrentLevel.IsPlayerAlive(killerIndex))
			{
				teams[player.PlayerIndex] = teams[killerIndex];
			}
		}
	}

	private static void TeamReviver_ReviveUpdate_Prefix(TeamReviver __instance)
    {
        var reviver = __instance;
        var teams = __instance.Level.Session.MatchSettings.Teams;

		if (__instance.Finished == true)
        {
            if (Instance.Context.Registry.Variants.GetVariant("Red Rover")!.IsActive())
            {
				if (teams![__instance.Corpse.PlayerIndex] != __instance.Corpse.TeamColor)
				{
                    Instance.Logger.LogInformation("TChange: " + __instance.Corpse.Allegiance + " => " + __instance.Corpse.TeamColor);
					teams[__instance.Corpse.PlayerIndex] = __instance.Corpse.TeamColor;
				}
			}
        }
        else if (__instance.Level.Session.MatchSettings.Mode == TowerFall.Modes.LastManStanding
        && Instance.Context.Registry.Variants.GetVariant("Kill Revive")!.IsActive()
		&& __instance.Level.Session.CurrentLevel.Ending)
        {
			var rl = __instance.Level.Session.RoundLogic as LastManStandingRoundLogic
                        ?? throw new InvalidCastException("RoundLogic is not LastManStandingRoundLogic");

			var field = AccessTools.Field(rl.GetType(), "roundEndCounter")
						?? throw new MissingFieldException(rl.GetType().FullName, "roundEndCounter");

			var counter = field.GetValue(rl)
						 ?? throw new NullReferenceException("roundEndCounter was null");

			var reset = AccessTools.Method(counter.GetType(), "Reset")
						?? throw new MissingMethodException(counter.GetType().FullName, "Reset");

			reset.Invoke(counter, null);
		}
    }

    private static void PressStartText_Render_Prefix(PressStartText __instance)
    {
        Draw.OutlineTextCentered(TFGame.Font, "FILTHY CASUAL EDITION", __instance.Position + new Vector2(30f, -50f), Color.White, Color.Black);

        // Example of a simple prefix hook.
        // This runs before PressStartText.Render().
        // You can access instance fields via __instance.
    }

    private static bool BoltArrow_TryTurn_Prefix(BoltArrow __instance, ref bool __result)
    {
        var variant = Instance.Context.Registry.Variants.GetVariant("Weak Bolt Arrows");
        if (variant != null
            && variant.IsActive(__instance.PlayerIndex)
            && __instance.Turns >= 1)
        {
            // After 1 successful seek-turn, pretend turning failed forever.
            __result = false;
            return false; // skip original TryTurn entirely
        }

        return true; // run original
    }
}