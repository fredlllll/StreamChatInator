using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StreamChatInator.Auth;
using StreamChatInator.Hubs;
using StreamChatInator.Services;
using StreamChatInator.Services.Emotes;
using StreamChatInator.Services.Twitch;

namespace StreamChatInator;

/// <summary>
/// The single composition root for app services. Program.cs uses it at
/// startup and the test project uses it to build the same container, so a
/// constructor change only has to be reflected here - never in the tests.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Port the Vite dev server runs on (CORS origin).</summary>
    internal const int VitePort = 53401;

    /// <summary>Name of the CORS policy allowing the Vite dev server origin.</summary>
    internal const string CorsPolicyName = "AllowReact";

    /// <summary>
    /// Registers every application service. Expects the caller to have
    /// registered DatabaseContext (the sqlite location is host-specific).
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ConfigService resolves IConfiguration; the host registers it itself,
        // bare containers (tests) get it from here.
        services.TryAddSingleton(configuration);
        services.AddLogging();

        // Bind config sections to typed options; ConfigService is the facade over them.
        services.Configure<TwitchOptions>(configuration.GetSection(TwitchOptions.SectionName));
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        services.AddControllers(options =>
        {
            // Gate every controller action behind the LAN PIN by default.
            // LanAccessHandler lets everything through when Auth:Enabled=false.
            options.Filters.Add(new AuthorizeFilter());
        });
        services.AddHostedService<ChatReaderService>();
        services.AddSingleton<ChatHubData>();
        services.AddSingleton<TwitchAuthService>();
        services.AddSingleton<EmoteProviderService>();
        // Typed HTTP clients: each service/fetcher gets its own pre-configured
        // HttpClient injected, so there are no named-client strings to keep in
        // sync. The fetchers end up captured by the singleton EmoteProvider-
        // Service; that's safe (handlers are kept alive while referenced) at
        // the cost of a long-lived handler.
        services.AddHttpClient<TwitchApiService>(ConfigureEmotesAndTwitchClient);
        services.AddHttpClient<IEmoteFetcher, BttvEmoteFetcher>(ConfigureEmotesAndTwitchClient);
        services.AddHttpClient<IEmoteFetcher, SevenTvEmoteFetcher>(ConfigureEmotesAndTwitchClient);
        services.AddHttpClient<IEmoteFetcher, FfzEmoteFetcher>(ConfigureEmotesAndTwitchClient);
        services.AddSingleton<BadgeProviderService>();
        services.AddSingleton<AccessControlService>();
        services.AddSingleton<IAuthorizationHandler, AccessControlHandler>();
        services.AddSingleton<EventRecorder>();
        services.AddSingleton<EventHistoryService>();
        services.AddSingleton<ConfigService>();
        services.AddSingleton<TwitchTokenService>();
        services.AddSingleton<TwitchUsernameService>();
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new AccessControlRequirement())
                .Build();
        });
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "StreamChatInator.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.LoginPath = "/login";
                options.Events = new CookieAuthenticationEvents
                {
                    // API calls get a 401 instead of a redirect to /login,
                    // so fetch() and SignalR fail cleanly when unauthenticated.
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }
                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddMemoryCache();
        services.AddSignalR();
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
                policy.WithOrigins($"http://localhost:{VitePort}")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        return services;
    }

    private static void ConfigureEmotesAndTwitchClient(HttpClient client)
    {
        client.Timeout = TimeSpan.FromSeconds(15);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("StreamChatInator/1.0");
    }
}