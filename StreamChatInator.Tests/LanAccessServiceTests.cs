using Microsoft.Extensions.DependencyInjection;
using StreamChatInator.Services;

namespace StreamChatInator.Tests;

public class LanAccessServiceTests : IDisposable
{
    private TestHost? _host;

    public void Dispose() => _host?.Dispose();

    private AccessControlService NewService(string? pin = "123456")
    {
        _host = new TestHost(config: new Dictionary<string, string?> { ["Auth:Pin"] = pin });
        return _host.Provider.GetRequiredService<AccessControlService>();
    }

    [Fact]
    public void InitiallyNotLockedOut()
    {
        var service = NewService();
        Assert.False(service.IsLockedOut("192.168.1.10"));
    }

    [Fact]
    public void FiveFailures_LockOutThatIpOnly()
    {
        var service = NewService();
        for (int i = 0; i < 5; i++) service.RegisterFailure("192.168.1.10");

        Assert.True(service.IsLockedOut("192.168.1.10"));
        Assert.False(service.IsLockedOut("192.168.1.11"));
    }

    [Fact]
    public void FailedBucketOnOneIp_DoesNotCountTowardAnother()
    {
        var service = NewService();
        for (int i = 0; i < 5; i++) service.RegisterFailure("10.0.0.1");

        // A fresh IP gets a full five attempts of its own.
        for (int i = 0; i < 4; i++) service.RegisterFailure("10.0.0.2");
        Assert.False(service.IsLockedOut("10.0.0.2"));
        service.RegisterFailure("10.0.0.2");
        Assert.True(service.IsLockedOut("10.0.0.2"));
    }

    [Fact]
    public void SuccessfulPin_ResetsThatIpsFailures()
    {
        var service = NewService();
        for (int i = 0; i < 4; i++) service.RegisterFailure("192.168.1.10");

        service.ResetFailures("192.168.1.10");
        for (int i = 0; i < 4; i++) service.RegisterFailure("192.168.1.10");

        // Without the reset the total (8) would have locked out long ago.
        Assert.False(service.IsLockedOut("192.168.1.10"));
    }

    [Fact]
    public void ValidatePin_AcceptsConfiguredPin_RejectsOthers()
    {
        var service = NewService("987654");
        Assert.True(service.ValidatePin("987654"));
        Assert.False(service.ValidatePin("000000"));
    }
}
