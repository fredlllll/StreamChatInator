using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StreamChatInator;

namespace StreamChatInator.Tests;

/// <summary>
/// Builds the same DI container the app uses in production
/// (AddApplicationServices), so tests resolve services exactly like the
/// running app does. A constructor change then only touches the service class
/// itself - never these tests. Register a test DatabaseContext (and anything
/// else test-specific) via <paramref name="configure"/> before building.
/// </summary>
internal sealed class TestHost : IDisposable
{
    private readonly ServiceProvider _provider;

    public TestHost(Action<IServiceCollection>? configure = null, Dictionary<string, string?>? config = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config ?? new Dictionary<string, string?>())
            .Build();

        var services = new ServiceCollection();
        services.AddApplicationServices(configuration);
        configure?.Invoke(services);

        _provider = services.BuildServiceProvider();
    }

    public ServiceProvider Provider => _provider;

    /// <summary>Resolves a service from a fresh scope (scoped services like DatabaseContext live per scope).</summary>
    public IServiceScope CreateScope() => _provider.CreateScope();

    public void Dispose() => _provider.Dispose();
}