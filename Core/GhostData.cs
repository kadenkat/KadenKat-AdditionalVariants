using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>
/// Extra state for the ghost variants to use.
/// </summary>
internal sealed class GhostData
{
    private static readonly ConditionalWeakTable<PlayerGhost, GhostData> table = new();

    public static GhostData Get(PlayerGhost ghost) => table.GetValue(ghost, _ => new GhostData());

    // GhostsRespawn
    public PlayerCorpse? OwnerCorpse;

    // BumpPossession
    public Player? Host;

    // ArcherGhosts
    public ArrowTypes? HeldArrow;
    public WrapHitbox? ArrowPickupHitbox;
    public Vector2 HaloPosition;
    public float HaloZoom;
    public float HaloRotation;

    // TriggerGhosts
    public float ExplodingCounter;
    public bool HasLaughed;
    public Sprite<int>? FlashSprite;
}

/// <summary>
/// Private <see cref="PlayerGhost"/> fields the ghost variants need every frame.
/// </summary>
internal static class GhostFields
{
    public static readonly AccessTools.FieldRef<PlayerGhost, Sprite<string>> Sprite =
        AccessTools.FieldRefAccess<PlayerGhost, Sprite<string>>("sprite");

    public static readonly AccessTools.FieldRef<PlayerGhost, Image> Halo =
        AccessTools.FieldRefAccess<PlayerGhost, Image>("halo");

    public static readonly AccessTools.FieldRef<PlayerGhost, InputState> Input =
        AccessTools.FieldRefAccess<PlayerGhost, InputState>("input");

    public static readonly AccessTools.FieldRef<PlayerGhost, Counter> KillDelayCounter =
        AccessTools.FieldRefAccess<PlayerGhost, Counter>("killDelayCounter");
}
