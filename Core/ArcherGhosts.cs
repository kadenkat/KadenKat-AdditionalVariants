using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// Ghosts can carry one arrow. They pick up loose arrows or steal one from an archer they touch,
/// fire it in their dash direction, and drop it when they die. The halo tilts while holding one.
/// </summary>
public class ArcherGhosts : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), nameof(PlayerGhost.Update)),
            postfix: new HarmonyMethod(PlayerGhost_Update_Postfix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), "OnPlayerTouch"),
            prefix: new HarmonyMethod(PlayerGhost_OnPlayerTouch_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), "DodgeEnter"),
            prefix: new HarmonyMethod(PlayerGhost_DodgeEnter_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), nameof(PlayerGhost.Die)),
            prefix: new HarmonyMethod(PlayerGhost_Die_Prefix)
        );
    }

    private static CharacterSounds GetSounds(PlayerGhost ghost)
    {
        int index = ghost.PlayerIndex;
        return ArcherData.Get(TFGame.Characters[index], TFGame.AltSelect[index]).SFX;
    }

    private static void HoldArrow(PlayerGhost ghost, GhostData data, ArrowTypes arrowType, SFX sound)
    {
        // TODO: Show the arrow with the correct type inside the ghost.
        var halo = GhostFields.Halo(ghost);
        data.HaloPosition = halo.Position;
        data.HaloZoom = halo.Zoom;
        data.HaloRotation = halo.Rotation;
        halo.Position = new Vector2(0f, -12f);
        halo.Zoom = data.HaloZoom * 1.5f;
        halo.Rotation = 1.5f;

        var sprite = GhostFields.Sprite(ghost);
        if (sprite.CurrentAnimID == "normal")
        {
            sprite.Play("happy");
        }

        data.HeldArrow = arrowType;
        sound.Play(ghost.X);
    }

    private static void ReleaseArrow(PlayerGhost ghost, GhostData data)
    {
        var halo = GhostFields.Halo(ghost);
        halo.Position = data.HaloPosition;
        halo.Zoom = data.HaloZoom;
        halo.Rotation = data.HaloRotation;
        data.HeldArrow = null;
    }

    private static void PlayerGhost_Update_Postfix(PlayerGhost __instance)
    {
        if (__instance.Scene == null || !__instance.Collidable || !Variants.ArcherGhosts.IsActive(__instance.PlayerIndex))
        {
            return;
        }

        var data = GhostData.Get(__instance);
        if (data.HeldArrow != null || GhostFields.Sprite(__instance).CurrentAnimID == "dodge")
        {
            return;
        }

        // Ghosts grab arrows with a hitbox larger than their body, like an archer's.
        var collider = __instance.Collider;
        __instance.Collider = data.ArrowPickupHitbox ??= new WrapHitbox(22f, 30f, -11f, -16f);
        var arrow = __instance.CollideFirst(GameTags.Arrow) as Arrow;
        __instance.Collider = collider;

        if (arrow == null || !arrow.IsCollectible)
        {
            return;
        }

        HoldArrow(__instance, data, arrow.ArrowType, GetSounds(__instance).ArrowRecover);
        arrow.BuriedIn?.OnCollect();
        if (arrow.Scene != null)
        {
            arrow.RemoveSelf();
        }
        arrow.Collidable = false;
    }

    private static void PlayerGhost_OnPlayerTouch_Prefix(PlayerGhost __instance, Player player)
    {
        if (__instance.State == 0 || !Variants.ArcherGhosts.IsActive(__instance.PlayerIndex))
        {
            return;
        }

        var data = GhostData.Get(__instance);
        if (data.HeldArrow == null && player.Arrows.HasArrows)
        {
            HoldArrow(__instance, data, player.Arrows.UseArrow(), player.ArcherData.SFX.ArrowSteal);
        }
    }

    // Dashing fires the held arrow in the dash direction
    private static void PlayerGhost_DodgeEnter_Prefix(PlayerGhost __instance)
    {
        var data = GhostData.Get(__instance);
        if (data.HeldArrow is not ArrowTypes arrowType)
        {
            return;
        }

        GetSounds(__instance).FireArrow.Play(__instance.X);
        var arrow = Arrow.Create(arrowType, __instance, __instance.Position + Player.ArrowOffset, __instance.Speed.Angle(), overridePlayerIndex: __instance.PlayerIndex);
        __instance.Level.Add(arrow);

        ReleaseArrow(__instance, data);
        GhostFields.Sprite(__instance).Play("normal");
    }

    private static void PlayerGhost_Die_Prefix(PlayerGhost __instance)
    {
        var data = GhostData.Get(__instance);
        if (data.HeldArrow is not ArrowTypes arrowType)
        {
            return;
        }

        var arrow = Arrow.Create(arrowType, __instance, __instance.Position, Calc.Random.Range(-2.3561945f, 1.57079637f), overridePlayerIndex: __instance.PlayerIndex);
        arrow.Drop(-(int)__instance.Facing);
        __instance.Level.Add(arrow);

        ReleaseArrow(__instance, data);
    }
}
