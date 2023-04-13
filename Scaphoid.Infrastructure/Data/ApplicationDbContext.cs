using Microsoft.EntityFrameworkCore;
using Scaphoid.Core.Model;

namespace Scaphoid.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Order>()
                .HasOne(e => e.Localization)
                .WithOne(e => e.Order)
                .HasForeignKey<Localization>(e => e.OrderId);

            builder.Entity<Localization>().HasKey(e => e.OrderId);
            builder.Entity<Beam>().HasKey(e => e.OrderId);
            builder.Entity<Localization>().OwnsOne(e => e.DeflectionLimit);
            builder.Entity<Localization>().OwnsOne(e => e.DesignParameters);

            builder.Entity<Loading>().HasKey(e => e.OrderId);
            builder.Entity<Loading>().OwnsOne(e => e.PermanentLoads);
            builder.Entity<Loading>().OwnsOne(e => e.VariableLoads);
            builder.Entity<Loading>().OwnsOne(e => e.UltimateLoads);

            base.OnModelCreating(builder);
        }
    }
}
