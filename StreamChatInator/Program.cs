using Microsoft.EntityFrameworkCore;
using StreamChatInator.Database;
using StreamChatInator.Hubs;
using StreamChatInator.Services;

namespace StreamChatInator
{
    public class Program
    {
        const int vitePort = 53401;
        const int port = 17455;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlite($"Data Source=db.sqlite").ConfigureWarnings(w => w.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
            builder.Services.AddControllers();
            builder.Services.AddHostedService<ChatReaderService>();
            builder.Services.AddSingleton<ChatHubData>();
            builder.Services.AddSingleton<EmoteProviderService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient("emotes", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("StreamChatInator/1.0");
            });
            builder.Services.AddSignalR();
            builder.Services.AddCors(options => {
                options.AddPolicy("AllowReact", builder =>
                    builder.WithOrigins("http://localhost:"+vitePort)
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials());
            });
            builder.WebHost.UseUrls("http://0.0.0.0:"+port);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var db = services.GetRequiredService<DatabaseContext>();
                db.Database.Migrate();
            }

            app.UseRouting();
            app.UseCors("AllowReact");
            app.MapStaticAssets();
            app.MapRazorPages().WithStaticAssets();
            app.MapControllers();
            app.MapHub<ChatHub>("/hubs/chat");

            app.Run();
        }
    }
}
