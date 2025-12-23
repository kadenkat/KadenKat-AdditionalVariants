using FortRise;
using HarmonyLib;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace ExampleMod;

public class ArcherGhosts : IHookable
{
    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Update)),
            postfix: new HarmonyMethod(typeof(ArcherGhosts), nameof(Ghost_Update_Postfix))
        );
    }

    private static bool TryGetPlayerCorpse(PlayerGhost ghost, out PlayerCorpse corpse)
    {
        corpse = default!;

        int index = ghost.PlayerIndex;
        var player = ghost.Level.GetPlayer(index);

        if (player != null)
        {
            corpse = DynamicData.For(player).Get<PlayerCorpse>("corpse")!;

            return corpse != null;
        }

        return false;
    }

    private static void Ghost_Update_Postfix(PlayerGhost ghost)
    {
        if (!Variants.ArcherGhosts.IsActive(ghost.PlayerIndex))
        {
            return;
        }

        if (ghost.Collidable)
        {
            var collider = DynamicData.For(ghost).Get<Collider>("collider");

            Entity entity = ghost.CollideFirst(Monocle.GameTags.Arrow);
            if (entity is Arrow arrow)
            {
                if (ghost.Arrows.Empty)
                {
                    ghost.CollectArrow(arrow);
                }
            }
        }
    }
}

public class Variants
{
    public static IVariantEntry ArcherGhosts { get; internal set; } = null!;
}

