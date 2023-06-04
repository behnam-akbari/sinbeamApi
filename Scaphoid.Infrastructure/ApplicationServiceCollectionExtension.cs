using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scaphoid.Infrastructure.Data;

namespace Scaphoid.Infrastructure
{
    public static class ApplicationServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            //var folder = Environment.SpecialFolder.LocalApplicationData;
            //var path = Environment.GetFolderPath(folder);
            //var dbPath = Path.Join(path, "scaphoid.db");

            var dbPath = "scaphoid.db";

            services.AddDbContextPool<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            return services;
        }
    }
}
