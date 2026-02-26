using Microsoft.EntityFrameworkCore;
using SpotifyStats.API.Models;

namespace SpotifyStats.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<SpotifyAccount> SpotifyAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SpotifyAccount configuration
        modelBuilder.Entity<SpotifyAccount>(entity =>
        {
            entity.HasKey(x => x.Id);

            // Unique index on SpotifyUserId
            entity.HasIndex(x => x.SpotifyUserId)
                .IsUnique();

            // Row version for concurrency
            entity.Property(x => x.RowVersion)
                .IsRowVersion();

            // Relationship with AppUser
            entity.HasOne(x => x.User)
                .WithOne(x => x.SpotifyAccount)
                .HasForeignKey<SpotifyAccount>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AppUser configuration
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(x => x.Id);
        });
    }
}
