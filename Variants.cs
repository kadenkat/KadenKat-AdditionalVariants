using FortRise;

namespace KadenKat.AdditionalVariants;

public class Variants : IRegisterable
{
    public static IVariantEntry NerfedPrismArrows { get; internal set; } = null!;
    public static IVariantEntry NerfedBoltArrows { get; internal set; } = null!;
    public static IVariantEntry RedRover { get; internal set; } = null!;
    public static IVariantEntry KillsRevive { get; internal set; } = null!;
    public static IVariantEntry GhostsRespawn { get; internal set; } = null!;
    public static IVariantEntry BumpPossession { get; internal set; } = null!;
    public static IVariantEntry ArcherGhosts { get; internal set; } = null!;
    public static IVariantEntry ThreeSpookyFiveMe { get; internal set; } = null!;
    public static IVariantEntry TriggerGhosts { get; internal set; } = null!;
    public static IVariantEntry SuperToyArrows { get; internal set; } = null!;
    public static IVariantEntry ToyArrowChests { get; internal set; } = null!;

    public static void Register(IModContent content, IModRegistry registry)
    {
        NerfedPrismArrows = registry.Variants.RegisterVariant("NerfedPrismArrows", new()
        {
            Title = "NERFED PRISM ARROWS",
            Icon = TextureRegistry.NerfedPrismArrows,
            Description = "PRISM ARROWS DON'T DESTROY CORPSES",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom,
        });

        NerfedBoltArrows = registry.Variants.RegisterVariant("NerfedBoltArrows", new()
        {
            Title = "NERFED BOLT ARROWS",
            Icon = TextureRegistry.NerfedBoltArrows,
            Description = "BOLT ARROWS TURN ONCE",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom,
        });

        SuperToyArrows = registry.Variants.RegisterVariant("SuperToyArrows", new()
        {
            Title = "SUPER TOY ARROWS",
            Icon = TextureRegistry.SuperToyArrows,
            Description = "TOY ARROWS HAVE STRONGER KNOCKBACK",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom,
        });

        ToyArrowChests = registry.Variants.RegisterVariant("ToyArrowChests", new()
        {
            Title = "TOY ARROW CHESTS",
            Icon = TextureRegistry.ToyArrowChests,
            Description = "CHESTS ONLY CONTAIN TOY ARROWS",
            Flags = CustomVariantFlags.CanRandom,
        });

        RedRover = registry.Variants.RegisterVariant("RedRover", new()
        {
            Title = "RED ROVER",
            Icon = TextureRegistry.RedRover,
            Description = "REVIVE YOUR VICTIM TO YOUR TEAM",
        });

        KillsRevive = registry.Variants.RegisterVariant("KillsRevive", new()
        {
            Title = "KILLS REVIVE",
            Icon = TextureRegistry.KillsRevive,
            Description = "LIFE FOR LIFE",
            Links = [registry.Variants.GetVariant("TeamRevive")!],
        });

        GhostsRespawn = registry.Variants.RegisterVariant("GhostsRespawn", new()
        {
            Title = "GHOSTS RESPAWN",
            Icon = TextureRegistry.GhostsRespawn,
            Description = "GHOSTS RESPAWN FROM THEIR CORPSE",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom,
        });

        BumpPossession = registry.Variants.RegisterVariant("BumpPossession", new()
        {
            Title = "BUMP POSSESSION",
            Icon = TextureRegistry.BumpPossession,
            Description = "GHOSTS DASH INTO ARCHERS TO POSSESS THEM",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom,
        });

        ArcherGhosts = registry.Variants.RegisterVariant("ArcherGhosts", new()
        {
            Title = "ARCHER GHOSTS",
            Icon = TextureRegistry.ArcherGhosts,
            Description = "GHOSTS DASH TO SHOOT ARROWS",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom,
        });

        ThreeSpookyFiveMe = registry.Variants.RegisterVariant("ThreeSpookyFiveMe", new()
        {
            Title = "THREE SPOOKY FIVE ME",
            Icon = TextureRegistry.ThreeSpookyFiveMe,
            Description = "ONLY TOY ARROWS AFFECT GHOSTS",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom,
        });

        TriggerGhosts = registry.Variants.RegisterVariant("TriggerGhosts", new()
        {
            Title = "TRIGGER GHOSTS",
            Icon = TextureRegistry.TriggerGhosts,
            Description = "HOLD JUMP AS A GHOST TO EXPLODE (OR DON'T)",
            Flags = CustomVariantFlags.PerPlayer | CustomVariantFlags.CanRandom,
        });
    }
}
