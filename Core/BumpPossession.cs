using System.Runtime.CompilerServices;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// Ghosts possess archers instead of killing them.
/// </summary>
public class BumpPossession : IHookable
{
    private static readonly ConditionalWeakTable<Player, PlayerGhost> possessors = new();

    private static readonly AccessTools.FieldRef<Player, InputState> playerInput =
        AccessTools.FieldRefAccess<Player, InputState>("input");

    private static readonly AccessTools.FieldRef<Player, Vector2> playerMoveAxis =
        AccessTools.FieldRefAccess<Player, Vector2>("moveAxis");

    private static readonly AccessTools.FieldRef<Player, bool> playerAutoBounce =
        AccessTools.FieldRefAccess<Player, bool>("autoBounce");

    public static void Load(IHarmony harmony)
    {
        // Ghost patches
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), "OnPlayerTouch"),
            prefix: new HarmonyMethod(PlayerGhost_OnPlayerTouch_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), "NormalUpdate"),
            prefix: new HarmonyMethod(PlayerGhost_NormalUpdate_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), nameof(PlayerGhost.OnArrowHit)),
            prefix: new HarmonyMethod(PlayerGhost_OnArrowHit_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(PlayerGhost), nameof(PlayerGhost.Die)),
            prefix: new HarmonyMethod(PlayerGhost_Die_Prefix)
        );

        // Player patches
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), "NormalUpdate"),
            prefix: new HarmonyMethod(Player_NormalUpdate_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredPropertyGetter(typeof(Player), "MaxRunSpeed"),
            postfix: new HarmonyMethod(Player_get_MaxRunSpeed_Postfix)
        );
        harmony.Patch(
            AccessTools.DeclaredPropertySetter(typeof(Player), nameof(Player.AimDirection)),
            prefix: new HarmonyMethod(Player_set_AimDirection_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Die), [typeof(DeathCause), typeof(int), typeof(bool), typeof(bool)]),
            prefix: new HarmonyMethod(Player_Die_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Hurt), [typeof(Lava)]),
            prefix: new HarmonyMethod(Player_HurtLava_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Hurt), [typeof(Explosion), typeof(Vector2)]),
            prefix: new HarmonyMethod(Player_HurtExplosion_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Hurt), [typeof(DeathCause), typeof(Vector2), typeof(int), typeof(SFX)]),
            prefix: new HarmonyMethod(Player_Hurt_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.HurtBouncedOn)),
            prefix: new HarmonyMethod(Player_HurtBouncedOn_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.HurtDelayed)),
            prefix: new HarmonyMethod(Player_HurtDelayed_Prefix)
        );
    }

    public static PlayerGhost? GetPossessor(Player player)
    {
        return possessors.TryGetValue(player, out var ghost) ? ghost : null;
    }

    private static void EnterHost(PlayerGhost ghost, Player host)
    {
        var sprite = GhostFields.Sprite(ghost);
        if (sprite.CurrentAnimID != "dodge")
        {
            return;
        }

        sprite.Play("despawn", restart: true);
        ghost.Seek = false;
        ghost.Collidable = false;
        GhostFields.Halo(ghost).Visible = false;

        GhostData.Get(ghost).Host = host;
        possessors.AddOrUpdate(host, ghost);
    }

    private static void LeaveHost(PlayerGhost ghost, Player host)
    {
        possessors.Remove(host);
        GhostData.Get(ghost).Host = null;

        ghost.Speed = host.Speed == Vector2.Zero ? new Vector2(0f, 1f) : host.Speed.SafeNormalize(-2f);

        var sprite = GhostFields.Sprite(ghost);
        sprite.Visible = true;
        GhostFields.Halo(ghost).Visible = true;

        if (sprite.CurrentAnimID == "death")
        {
            ghost.RemoveSelf();
            return;
        }

        sprite.Play("spawn", restart: true);
        GhostFields.KillDelayCounter(ghost) = new Counter(30);
        ghost.Seek = false;
        ghost.Collidable = true;
    }

    private static void ExitShieldedState(Player player)
    {
        if (player.State == Player.PlayerStates.LedgeGrab || player.State == Player.PlayerStates.Dodging)
        {
            player.State = Player.PlayerStates.Normal;
        }
    }

    private static bool PlayerGhost_OnPlayerTouch_Prefix(PlayerGhost __instance, Player player)
    {
        if (__instance.State == 0 || !Variants.BumpPossession.IsActive(__instance.PlayerIndex))
        {
            return true;
        }

        player.Speed = (player.Position - __instance.Position).SafeNormalize(4.5f);
        __instance.Speed = (__instance.Position - player.Position).SafeNormalize(3f);
        Sounds.env_arrowToyChar.Play(__instance.X);

        if (GetPossessor(player) == null)
        {
            EnterHost(__instance, player);
        }
        return false;
    }

    // While possessing, the ghost just chills inside its host.
    private static bool PlayerGhost_NormalUpdate_Prefix(PlayerGhost __instance, ref int __result)
    {
        var host = GhostData.Get(__instance).Host;
        if (host == null)
        {
            return true;
        }

        if (host.Scene == null || host.Dead)
        {
            LeaveHost(__instance, host);
            return true;
        }

        __instance.Facing = host.Facing;
        __instance.Position = host.Position;
        __instance.Speed = Vector2.Zero;
        __instance.Collidable = false;
        GhostFields.Sprite(__instance).Visible = false;

        __result = 1;
        return false;
    }

    private static bool PlayerGhost_OnArrowHit_Prefix(PlayerGhost __instance, ref bool __result)
    {
        if (GhostData.Get(__instance).Host == null)
        {
            return true;
        }

        __result = false;
        return false;
    }

    private static void PlayerGhost_Die_Prefix(PlayerGhost __instance)
    {
        var host = GhostData.Get(__instance).Host;
        if (host != null)
        {
            LeaveHost(__instance, host);
        }
    }

    // Archer's movement falls back to the ghost's when the archer isn't pushing that axis, and either can jump.
    private static void Player_NormalUpdate_Prefix(Player __instance)
    {
        var ghost = GetPossessor(__instance);
        if (ghost == null)
        {
            return;
        }

        ref var input = ref playerInput(__instance);
        ref var moveAxis = ref playerMoveAxis(__instance);
        var ghostInput = GhostFields.Input(ghost);

        if (input.MoveX == 0)
        {
            moveAxis.X = ghostInput.MoveX;
        }
        if (input.MoveY == 0)
        {
            moveAxis.Y = ghostInput.MoveY;
        }
        input.JumpPressed |= ghostInput.JumpPressed;
        input.JumpCheck |= ghostInput.JumpCheck;

        if (__instance.Scene.OnInterval(5))
        {
            __instance.Level.Particles.Emit(Particles.PlayerDust[TFGame.Characters[ghost.PlayerIndex]], 5, __instance.Position + new Vector2(0f, -6f), Vector2.One * 5f);
        }
    }

    // Opposed stick directions slows the archer down.
    private static void Player_get_MaxRunSpeed_Postfix(Player __instance, ref float __result)
    {
        var ghost = GetPossessor(__instance);
        if (ghost == null)
        {
            return;
        }

        int moveX = playerInput(__instance).MoveX;
        int ghostMoveX = GhostFields.Input(ghost).MoveX;
        if (moveX != 0 && ghostMoveX != 0 && Math.Sign(moveX) != Math.Sign(ghostMoveX))
        {
            __result *= 0.6f;
        }
    }

    // Aim is the average of both sticks.
    private static void Player_set_AimDirection_Prefix(Player __instance, ref float value)
    {
        var ghost = GetPossessor(__instance);
        if (ghost == null)
        {
            return;
        }

        var aimAxis = (playerInput(__instance).AimAxis + GhostFields.Input(ghost).AimAxis) / 2f;
        bool snap = !__instance.Level.Session.MatchSettings.Variants!.FreeAiming[__instance.PlayerIndex];
        value = PlayerInput.GetAimDirection(aimAxis, snap) ?? (__instance.Facing == Facing.Right ? 0f : MathF.PI);
    }

    private static void Player_Die_Prefix(Player __instance, DeathCause deathCause, int killerIndex)
    {
        var ghost = GetPossessor(__instance);
        if (ghost == null || __instance.Dead)
        {
            return;
        }

        if (deathCause == DeathCause.Squish || deathCause == DeathCause.Explosion)
        {
            ghost.Die(killerIndex);
        }
        else
        {
            LeaveHost(ghost, __instance);
        }
    }

    private static bool Player_HurtLava_Prefix(Player __instance, Lava lava)
    {
        var ghost = GetPossessor(__instance);
        if (ghost == null || __instance.HasShield || !__instance.CanHurt)
        {
            return true;
        }

        LeaveHost(ghost, __instance);
        __instance.Speed = 3.2f * lava.PushDirection;
        playerAutoBounce(__instance) = true;
        ExitShieldedState(__instance);
        return false;
    }

    // A real shield still takes the explosion, but knocks the ghost out. Without one, both die.
    private static void Player_HurtExplosion_Prefix(Player __instance)
    {
        var ghost = GetPossessor(__instance);
        if (ghost != null && __instance.HasShield && __instance.CanHurtIgnoreDodge)
        {
            LeaveHost(ghost, __instance);
        }
    }

    private static bool Player_Hurt_Prefix(Player __instance, DeathCause cause, Vector2 bounceFrom)
    {
        var ghost = GetPossessor(__instance);
        if (ghost == null || __instance.HasShield || !__instance.CanHurt
            || cause == DeathCause.Curse || cause == DeathCause.Miasma || cause == DeathCause.Squish || cause == DeathCause.Explosion)
        {
            return true;
        }

        LeaveHost(ghost, __instance);
        __instance.Speed = (__instance.Position - bounceFrom).SafeNormalize(3.2f);
        playerAutoBounce(__instance) = true;
        ExitShieldedState(__instance);
        return false;
    }

    private static bool Player_HurtBouncedOn_Prefix(Player __instance)
    {
        var ghost = GetPossessor(__instance);
        if (ghost == null || __instance.HasShield || !__instance.CanHurtIgnoreDodge)
        {
            return true;
        }

        LeaveHost(ghost, __instance);
        ExitShieldedState(__instance);
        return false;
    }

    private static bool Player_HurtDelayed_Prefix(Player __instance, Arrow arrow)
    {
        var ghost = GetPossessor(__instance);
        if (ghost == null || __instance.HasShield || !__instance.CanHurt)
        {
            return true;
        }

        LeaveHost(ghost, __instance);
        __instance.Speed = arrow.Speed;
        arrow.EnterFallMode(true, false, true);
        ExitShieldedState(__instance);
        return false;
    }
}
