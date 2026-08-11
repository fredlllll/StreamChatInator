using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StreamChatInator.Database
{
    /// <summary>
    /// Lets `dotnet ef` create a DatabaseContext at design time (migrations,
    /// scaffold) without running the full web host. Uses an in-memory SQLite
    /// connection since migrations never touch real data.
    /// </summary>
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;
            return new DatabaseContext(options);
        }
    }
}