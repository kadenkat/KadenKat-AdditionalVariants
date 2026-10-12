using FortRise;
using Microsoft.Extensions.Logging;

namespace KadenKat.AdditionalVariants;

public sealed class AdditionalVariantsModule : Mod
{
    public static AdditionalVariantsModule Instance { get; private set; } = null!;

    private static Type[] Registerables = [
        typeof(TextureRegistry),
        typeof(Variants),
    ];

    // Order matters where two hookables patch the same method.
    // ArcherGhosts must steal an arrow before BumpPossession takes over OnPlayerTouch, and TriggerGhosts
    // must charge before BumpPossession takes over NormalUpdate for a possessing ghost.
    private static Type[] Hookables = [
        typeof(GhostVariantLinks),
        typeof(NerfedPrismArrows),
        typeof(NerfedBoltArrows),
        typeof(RedRover),
        typeof(KillsRevive),
        typeof(GhostsRespawn),
        typeof(ArcherGhosts),
        typeof(TriggerGhosts),
        typeof(BumpPossession),
        typeof(ThreeSpookyFiveMe),
        typeof(SuperToyArrows),
        typeof(ToyArrowChests),
    ];

    public AdditionalVariantsModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
        Instance = this;

        foreach (var hookable in Hookables)
        {
            hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, [context.Harmony]);
        }

        foreach (var registerable in Registerables)
        {
            registerable.GetMethod(nameof(IRegisterable.Register))!.Invoke(null, [content, context.Registry]);
        }
    }
}
