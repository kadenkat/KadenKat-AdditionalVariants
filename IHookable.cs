using FortRise;

namespace ExampleMod;

internal interface IHookable
{
    abstract static void Load(IHarmony harmony);
}
