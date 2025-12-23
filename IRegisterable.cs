using FortRise;

namespace ExampleMod;

internal interface IRegisterable
{
    abstract static void Register(IModContent content, IModRegistry registry);
}