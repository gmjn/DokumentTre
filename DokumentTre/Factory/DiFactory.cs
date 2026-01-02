using Microsoft.Extensions.DependencyInjection;
using System;

namespace DokumentTre.Factory;

public sealed class DiFactory : IFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DiFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Create<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public T? TryCreate<T>() where T : class
    {
        return _serviceProvider.GetService<T>();
    }
}