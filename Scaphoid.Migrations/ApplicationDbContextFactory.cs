using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Scaphoid.Infrastructure.Data;

namespace Scaphoid.Migrations
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var dbPath = Path.Join(path, "scaphoid.db");

            var connectionString = $"Data Source={dbPath}";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(connectionString, b => b.MigrationsAssembly("Scaphoid.Migrations"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}