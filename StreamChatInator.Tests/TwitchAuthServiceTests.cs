using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Open.Observable;
using StreamChatInator.Database;
using StreamChatInator.Database.Models;
using StreamChatInator.Services;
using StreamChatInator.Services.Twitch;
using StreamChatInator.Services.Twitch.Settings;
using System.Net;
using System.Reactive;
using System.Text;

namespace StreamChatInator.Tests;

public class TwitchAuthServiceTests : IDisposable
{
    private sealed class FakeTwitchHandler : HttpMessageHandler
    {
        public int TokenRequests;
        public HttpStatusCode TokenStatus = HttpStatusCode.OK;
        public string TokenBody = """{"access_token":"tok2","refresh_token":"r2","expires_in":3600}""";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Post && request.RequestUri!.Host == "id.twitch.tv")
            {
                TokenRequests++;
                return Task.FromResult(new HttpResponseMessage(TokenStatus)
                {
                    Content = new StringContent(TokenBody, Encoding.UTF8, "application/json"),
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }

    private readonly SqliteConnection _connection;
    private readonly TestHost _host;
    private readonly IServiceScope _scope;
    private readonly DatabaseContext _db;
    private readonly FakeTwitchHandler _handler = new();
    private readonly TwitchAuthService _auth;

    public TwitchAuthServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _host = new TestHost(services =>
            services.AddDbContext<DatabaseContext>(options => options.UseSqlite(_connection)));
        _scope = _host.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        _db.Database.EnsureCreated();

        var oauthClient = new TwitchOAuthService(
            new HttpClient(_handler),
            _scope.ServiceProvider.GetRequiredService<ConfigService>());
        _auth = new TwitchAuthService(
            oauthClient,
            _scope.ServiceProvider.GetRequiredService<TwitchTokenSettingService>(),
            _scope.ServiceProvider.GetRequiredService<TwitchRefreshTokenSettingService>(),
            _scope.ServiceProvider.GetRequiredService<TwitchTokenExpiresAtSettingService>(),
            _scope.ServiceProvider.GetRequiredService<ILogger<TwitchAuthService>>());
    }

    public void Dispose()
    {
        _scope.Dispose();
        _host.Dispose();
        _connection.Dispose();
    }

    private void SeedToken(string token, TimeSpan expiresIn, string? refreshToken = "r1")
    {
        _db.SetSettingsValue(SettingValue.SettingOAuthToken, token);
        _db.SetSettingsValue(SettingValue.SettingOAuthTokenExpiresAt, DateTime.UtcNow.Add(expiresIn).ToString("o"));
        if (refreshToken != null)
        {
            _db.SetSettingsValue(SettingValue.SettingOAuthRefreshToken, refreshToken);
        }
        _db.SaveChanges();
    }

    /// <summary>
    /// Reads a setting through a fresh context - the refresh flow writes via
    /// its own scoped context, so _db's change tracker would return stale values.
    /// </summary>
    private string? ReadSetting(string name)
    {
        using var scope = _host.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        return db.GetSettingsValueOrNull(name);
    }

    [Fact]
    public async Task EnsureFreshTokenAsync_Refreshes_WhenTokenIsNearExpiry()
    {
        SeedToken("tok1", TimeSpan.FromMinutes(1));
        var tokenService = _scope.ServiceProvider.GetRequiredService<TwitchTokenSettingService>();
        var observed = new List<string?>();
        using var subscription = tokenService.Token.Subscribe(Observer.ToObserver<string?>(notification =>
        {
            if (notification.Kind == NotificationKind.OnNext)
            {
                observed.Add(notification.Value);
            }
        }));

        await _auth.EnsureFreshTokenAsync();

        Assert.Equal(1, _handler.TokenRequests);
        Assert.Contains("tok2", observed);
        Assert.Equal("tok2", ReadSetting(SettingValue.SettingOAuthToken));
        Assert.Equal("r2", ReadSetting(SettingValue.SettingOAuthRefreshToken));
    }

    [Fact]
    public async Task EnsureFreshTokenAsync_DoesNothing_WhenTokenIsStillFresh()
    {
        SeedToken("tok1", TimeSpan.FromHours(1));

        await _auth.EnsureFreshTokenAsync();

        Assert.Equal(0, _handler.TokenRequests);
        Assert.Equal("tok1", ReadSetting(SettingValue.SettingOAuthToken));
    }

    [Fact]
    public async Task EnsureFreshTokenAsync_DoesNothing_WhenNoCredentialsExist()
    {
        await _auth.EnsureFreshTokenAsync();

        Assert.Equal(0, _handler.TokenRequests);
    }

    [Fact]
    public async Task EnsureFreshTokenAsync_ClearsCredentials_WhenRefreshTokenIsRejected()
    {
        SeedToken("tok1", TimeSpan.FromMinutes(1));
        _handler.TokenStatus = HttpStatusCode.BadRequest;
        _handler.TokenBody = """{"error":"invalid_grant","error_description":"Invalid grant"}""";

        await _auth.EnsureFreshTokenAsync();

        Assert.Equal(1, _handler.TokenRequests);
        Assert.Null(ReadSetting(SettingValue.SettingOAuthToken));
        Assert.Null(ReadSetting(SettingValue.SettingOAuthRefreshToken));
        Assert.Null(ReadSetting(SettingValue.SettingOAuthTokenExpiresAt));
    }

    [Fact]
    public async Task EnsureFreshTokenAsync_KeepsCredentials_WhenRefreshFailsTransiently()
    {
        SeedToken("tok1", TimeSpan.FromMinutes(1), "r1");
        _handler.TokenStatus = HttpStatusCode.InternalServerError;
        _handler.TokenBody = """{"error":"server_error"}""";

        await _auth.EnsureFreshTokenAsync();

        Assert.Equal(1, _handler.TokenRequests);
        Assert.Equal("tok1", ReadSetting(SettingValue.SettingOAuthToken));
        Assert.Equal("r1", ReadSetting(SettingValue.SettingOAuthRefreshToken));
    }
}
