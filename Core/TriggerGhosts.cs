using FortRise;
using HarmonyLib;
using Monocle;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// Ghosts behave just like trigger corpses.
/// Hold jump to charge until exploding or release to discharge.
/// </summary>
public class TriggerGhosts : IHookable
{
    private const float ChargeFrames = 40f;

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), "NormalUpdate"),
            prefix: new HarmonyMethod(PlayerGhost_NormalUpdate_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), nameof(PlayerGhost.Die)),
            prefix: new HarmonyMethod(PlayerGhost_Die_Prefix)
        );
    }

    private static Sprite<int> GetFlashSprite(PlayerGhost ghost, GhostData data)
    {
        if (data.FlashSprite == null)
        {
            var sprite = GhostFields.Sprite(ghost);
            data.FlashSprite = new Sprite<int>(TextureRegistry.GhostFlash.Subtexture, (int)sprite.Width, (int)sprite.Height);
            data.FlashSprite.Visible = false;
            ghost.Add(data.FlashSprite);
        }
        return data.FlashSprite;
    }

    private static void StopChargeSound(PlayerGhost ghost)
    {
        if (BombPickup.SFXNewest == ghost)
        {
            BombPickup.SFXNewest = null;
            Sounds.sfx_bombChestLoop.Stop();
        }
    }

    private static void PlayerGhost_NormalUpdate_Prefix(PlayerGhost __instance)
    {
        int playerIndex = __instance.PlayerIndex;
        if (playerIndex == -1 || !Variants.TriggerGhosts.IsActive(playerIndex))
        {
            return;
        }

        var data = GhostData.Get(__instance);
        var flash = GetFlashSprite(__instance, data);
        bool triggering = GhostFields.Input(__instance).JumpCheck;

        if (data.ExplodingCounter < 0.2f)
        {
            data.HasLaughed = false;
        }

        if (triggering && data.ExplodingCounter < 0.75f)
        {
            TFGame.PlayerInputs[playerIndex].Rumble(0.2f, 2);
        }
        else if (!triggering && data.ExplodingCounter == 0f)
        {
            TFGame.PlayerInputs[playerIndex].StopRumble();
        }

        if (!triggering)
        {
            StopChargeSound(__instance);
            flash.Visible = false;
            data.ExplodingCounter = Math.Max(0f, data.ExplodingCounter - 1f / ChargeFrames * Engine.TimeMult);
            return;
        }

        if (!data.HasLaughed && data.ExplodingCounter > 0.75f)
        {
            Sounds.env_backfireMushWobbz.Play(__instance.X);
            data.HasLaughed = true;
        }

        var sprite = GhostFields.Sprite(__instance);
        flash.CurrentFrame = sprite.CurrentFrame;
        flash.Origin = sprite.Origin;
        flash.Position = sprite.Position;
        flash.Effects = sprite.Effects;
        flash.Scale = sprite.Scale;
        flash.Color = sprite.Color;
        if (__instance.Scene.OnInterval(5))
        {
            flash.Visible = !flash.Visible;
        }

        data.ExplodingCounter = Math.Min(1f, data.ExplodingCounter + 1f / ChargeFrames * Engine.TimeMult);
        if (data.ExplodingCounter >= 1f)
        {
            Explode(__instance, data);
        }
        else if (BombPickup.SFXNewest != __instance)
        {
            BombPickup.SFXNewest = __instance;
            Sounds.sfx_bombChestLoop.Play(__instance.X);
        }
    }

    private static void Explode(PlayerGhost ghost, GhostData data)
    {
        data.ExplodingCounter = 0f;
        StopChargeSound(ghost);
        Explosion.Spawn(ghost.Level, ghost.Position, ghost.PlayerIndex, plusOneKill: false, triggerBomb: false, bombTrap: false);
        Sounds.pu_bombArrowExplode.Play(ghost.X);
    }

    private static void PlayerGhost_Die_Prefix(PlayerGhost __instance)
    {
        StopChargeSound(__instance);
        if (GhostData.Get(__instance).FlashSprite is { } flash)
        {
            flash.Visible = false;
        }
    }
}
