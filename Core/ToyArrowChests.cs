using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using TowerFall;

namespace KadenKat.AdditionalVariants;

/// <summary>Every chest opens to a single toy arrows pickup.</summary>
public class ToyArrowChests : IHookable
{
    private static bool openingToyChest;

    private static readonly AccessTools.FieldRef<ArrowTypePickup, ArrowTypes> pickupArrowType =
        AccessTools.FieldRefAccess<ArrowTypePickup, ArrowTypes>("arrowType");

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TreasureChest), nameof(TreasureChest.OpenChest)),
            prefix: new HarmonyMethod(TreasureChest_OpenChest_Prefix),
            finalizer: new HarmonyMethod(TreasureChest_OpenChest_Finalizer)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Pickup), nameof(Pickup.CreatePickup)),
            prefix: new HarmonyMethod(Pickup_CreatePickup_Prefix)
        );
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ArrowTypePickup), nameof(ArrowTypePickup.PlaySound)),
            prefix: new HarmonyMethod(ArrowTypePickup_PlaySound_Prefix)
        );
    }

    private static void TreasureChest_OpenChest_Prefix(TreasureChest __instance, List<Pickups> ___pickups)
    {
        if (__instance.State >= TreasureChest.States.Opening || ___pickups.Contains(Pickups.Gem) || !Variants.ToyArrowChests.IsActive())
        {
            return;
        }

        ___pickups.Clear();
        ___pickups.Add(Pickups.Arrows);
        openingToyChest = true;
    }

    private static void TreasureChest_OpenChest_Finalizer()
    {
        openingToyChest = false;
    }

    // The game has no toy arrow pickup, so use a normal arrorw and change its type to toy upon pickup.
    private static bool Pickup_CreatePickup_Prefix(Vector2 position, Vector2 targetPosition, Pickups type, ref Pickup __result)
    {
        if (!openingToyChest || type != Pickups.Arrows)
        {
            return true;
        }

        // TODO: Add Toy arrow to ArrowTypePickup instead of using normal arrow as placeholder.
        var pickup = new ArrowTypePickup(position, targetPosition, ArrowTypes.Normal);
        pickupArrowType(pickup) = ArrowTypes.Toy;
        pickup.Name = "TOY";
        DynamicData.For(pickup).Set("PickupType", Pickups.Arrows);

        __result = pickup;
        return false;
    }

    private static bool ArrowTypePickup_PlaySound_Prefix(ArrowTypePickup __instance)
    {
        if (pickupArrowType(__instance) != ArrowTypes.Toy)
        {
            return true;
        }

        Sounds.pu_plus2Arrows.Play(__instance.X);
        return false;
    }
}
