using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Scaphoid.Core.Model;

namespace Scaphoid.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Question> Questions { get; set; }

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

            builder.Entity<Restraint>().HasKey(e => e.OrderId);

            var intArrayValueConverter = new ValueConverter<List<double>, string>(
                i => string.Join(",", i),
                s => string.IsNullOrWhiteSpace(s) ? new List<double>() : s.Split(new[] { ',' }).Select(double.Parse)
                .ToList());

            builder.Entity<Restraint>().Property(e => e.TopFlangeRestraints)
                 .HasConversion(intArrayValueConverter);

            builder.Entity<Restraint>().Property(e => e.BottomFlangeRestraints)
                 .HasConversion(intArrayValueConverter);

            base.OnModelCreating(builder);
        }
    }
}
