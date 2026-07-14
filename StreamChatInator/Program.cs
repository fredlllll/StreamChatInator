using Microsoft.EntityFrameworkCore;
using StreamChatInator.Database;
using StreamChatInator.Services;

namespace StreamChatInator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlite($"Data Source=db.sqlite").ConfigureWarnings(w => w.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
            builder.Services.AddControllers();
            builder.Services.AddHostedService<ChatReaderService>();
            builder.WebHost.UseUrls("http://0.0.0.0:17455");

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
            app.MapStaticAssets();
            app.MapRazorPages().WithStaticAssets();
            app.MapControllers();

            app.Run();
        }
    }
}
