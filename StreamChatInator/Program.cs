using Microsoft.EntityFrameworkCore;
using StreamChatInator.Database;
using StreamChatInator.Hubs;
using StreamChatInator.Services;

namespace StreamChatInator
{
    public class Program
    {
        const int vitePort = 53401;
        const int defaultPort = 17455;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configurable port (env `Port` or appsettings) so the published app
            // doesn't hard-code a fixed address; defaults to 17455.
            var port = int.TryParse(builder.Configuration["Port"], out var p) ? p : defaultPort;
            var displayUrl = $"http://localhost:{port}";

            // Fancy console UI (info panel + scrolling log area). Falls back to
            // the normal console logger when the UI can't be used (e.g. piped).
            if (ConsoleUi.Init("StreamChatInator", displayUrl))
            {
                builder.Logging.ClearProviders();
                builder.Logging.AddProvider(new ConsoleUiLoggerProvider());
            }

            // Keep the sqlite file in the per-user data dir so installs in
            // read-only locations (Program Files, /usr, etc) still work. Falls
            // back to the exe folder when no user dir is available.
            var dbPath = GetDatabasePath();
            builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlite($"Data Source={dbPath}").ConfigureWarnings(w => w.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
            builder.Services.AddControllers();
            builder.Services.AddHostedService<ChatReaderService>();
            builder.Services.AddSingleton<ChatHubData>();
            builder.Services.AddSingleton<TwitchTokenService>();
            builder.Services.AddSingleton<EmoteProviderService>();
            builder.Services.AddSingleton<BadgeProviderService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient("emotes", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("StreamChatInator/1.0");
            });
            builder.Services.AddHttpClient("twitch", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("StreamChatInator/1.0");
            });
            builder.Services.AddSignalR();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReact", policy =>
                    policy.WithOrigins($"http://localhost:{vitePort}")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });

            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
#pragma warning disable XUnit1013
                db.Database.Migrate();
#pragma warning restore XUnit1013
                DatabaseSeeder.Seed(db);
            }

            app.UseRouting();
            app.UseCors("AllowReact");
#pragma warning disable CS1998
            app.MapStaticAssets();
#pragma warning restore CS1998

            app.MapControllers();
            app.MapHub<ChatHub>("/hubs/chat");

            // The React SPA owns all non-API routes. Only mapped when the built
            // frontend exists (i.e. after publish; in dev SpaProxy/Vite handles it).
            var webRoot = app.Environment.WebRootPath;
            if (webRoot is not null && File.Exists(Path.Combine(webRoot, "index.html")))
            {
                app.MapFallbackToFile("index.html");
            }

            app.Lifetime.ApplicationStopping.Register(ConsoleUi.Shutdown);

            ConsoleUi.SetStatus("Starting…");

            app.Run();
        }

        /// <summary>Resolves the sqlite file to the per-user data directory, falling back to the executable's folder.</summary>
        static string GetDatabasePath()
        {
            try
            {
                var userDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(userDir))
                {
                    var appDir = Path.Combine(userDir, "StreamChatInator");
                    Directory.CreateDirectory(appDir);
                    return Path.Combine(appDir, "db.sqlite");
                }
            }
            catch
            {
                // Fall back to the exe folder below.
            }
            return Path.Combine(AppContext.BaseDirectory, "db.sqlite");
        }
    }
}
