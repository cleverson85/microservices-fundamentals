using CommandService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommandService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        { }

        public DbSet<Platform> Platform { get; set; }
        public DbSet<Command> Command { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Platform>()
                        .HasMany(c => c.Commands)
                        .WithOne(c => c.Platform!)
                        .HasForeignKey(c => c.PlatformId);

            modelBuilder.Entity<Command>()
                        .HasOne(c => c.Platform)
                        .WithMany(c => c.Commands)
                        .HasForeignKey(c => c.PlatformId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
