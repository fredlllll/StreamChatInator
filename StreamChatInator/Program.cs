using Microsoft.EntityFrameworkCore;
using StreamChatInator.Database;
using StreamChatInator.Hubs;
using StreamChatInator.Services;

namespace StreamChatInator
{
    public class Program
    {
        const int DefaultPort = 17455;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configurable port (env `Port` or appsettings) so the published app
            // doesn't hard-code a fixed address; defaults to 17455.
            var port = int.TryParse(builder.Configuration["Port"], out var configuredPort) ? configuredPort : DefaultPort;
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
            builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlite($"Data Source={dbPath}")
                .ConfigureWarnings(w =>
                {
                    if (builder.Environment.IsDevelopment())
                        w.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
                    else
                        w.Throw(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
                }));
            builder.Services.AddApplicationServices(builder.Configuration);

            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                db.Database.Migrate();
                DatabaseSeeder.Seed(db);
            }

            app.UseRouting();
            app.UseCors("AllowReact");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();

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

            var lanAccess = app.Services.GetRequiredService<AccessControlService>();
            if (lanAccess.Enabled)
            {
                if (ConsoleUi.IsEnabled)
                {
                    // Fixed panel line (never scrolls away). SetPin also appends
                    // the PIN to the shown link (?pin=…) so clicking/copying it
                    // unlocks the UI without typing anything.
                    ConsoleUi.SetPin(lanAccess.Pin);
                }
                else
                {
                    // No panel to show it on (redirected/service mode) - the log is
                    // the only channel left, and there's no scrolling UI to lose it in.
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("LAN access link: {Url} (PIN: {Pin}) — open it on any device in your network to unlock the UI (set Auth:Enabled=false or Auth:Pin to change this)", $"{displayUrl}/?pin={lanAccess.Pin}", lanAccess.Pin);
                }
            }
            else
            {
                ConsoleUi.SetPin(null);
                ConsoleUi.SetStatus("LAN access PIN disabled");
            }

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
