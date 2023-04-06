using Microsoft.Extensions.DependencyInjection;

namespace DokumentTre.Factory;

public static class FactoryExtension
{
    public static void AddDiFactory(this ServiceCollection services)
    {
        services.AddSingleton<IFactory, IFactory>(x => new DiFactory(x));
    }
}