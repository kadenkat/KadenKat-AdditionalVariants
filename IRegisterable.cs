using FortRise;

namespace KadenKat.AdditionalVariants;

internal interface IRegisterable
{
    abstract static void Register(IModContent content, IModRegistry registry);
}