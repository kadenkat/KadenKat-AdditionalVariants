using FortRise;

namespace KadenKat.AdditionalVariants;

public class TextureRegistry : IRegisterable
{
    public static ISubtextureEntry NerfedPrismArrows { get; private set; } = null!;
    public static ISubtextureEntry NerfedBoltArrows { get; private set; } = null!;
    public static ISubtextureEntry RedRover { get; private set; } = null!;
    public static ISubtextureEntry KillsRevive { get; private set; } = null!;
    public static ISubtextureEntry GhostsRespawn { get; private set; } = null!;
    public static ISubtextureEntry BumpPossession { get; private set; } = null!;
    public static ISubtextureEntry ArcherGhosts { get; private set; } = null!;
    public static ISubtextureEntry ThreeSpookyFiveMe { get; private set; } = null!;
    public static ISubtextureEntry TriggerGhosts { get; private set; } = null!;
    public static ISubtextureEntry SuperToyArrows { get; private set; } = null!;
    public static ISubtextureEntry ToyArrowChests { get; private set; } = null!;

    public static ISubtextureEntry GhostFlash { get; private set; } = null!;

    public static void Register(IModContent content, IModRegistry registry)
    {
        NerfedPrismArrows = RegisterIcon(content, registry, "NerfedPrismArrows", "nerfedPrismArrows");
        NerfedBoltArrows = RegisterIcon(content, registry, "NerfedBoltArrows", "nerfedBoltArrows");
        RedRover = RegisterIcon(content, registry, "RedRover", "redRover");
        KillsRevive = RegisterIcon(content, registry, "KillsRevive", "killsRevive");
        GhostsRespawn = RegisterIcon(content, registry, "GhostsRespawn", "ghostsRespawn");
        BumpPossession = RegisterIcon(content, registry, "BumpPossession", "bumpPossession");
        ArcherGhosts = RegisterIcon(content, registry, "ArcherGhosts", "archerGhosts");
        ThreeSpookyFiveMe = RegisterIcon(content, registry, "ThreeSpookyFiveMe", "threeSpookyFiveMe");
        TriggerGhosts = RegisterIcon(content, registry, "TriggerGhosts", "triggerGhosts");
        SuperToyArrows = RegisterIcon(content, registry, "SuperToyArrows", "superToyArrows");
        ToyArrowChests = RegisterIcon(content, registry, "ToyArrowChests", "toyArrowChests");

        GhostFlash = registry.Subtextures.RegisterTexture("GhostFlash", content.Root.GetRelativePath("Content/images/ghost/flash.png"));
    }

    private static ISubtextureEntry RegisterIcon(IModContent content, IModRegistry registry, string id, string fileName)
    {
        return registry.Subtextures.RegisterTexture(id, content.Root.GetRelativePath($"Content/images/variants/{fileName}.png"));
    }
}
