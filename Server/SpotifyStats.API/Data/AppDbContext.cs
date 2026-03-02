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

            // Row version for concurrency (SQLite doesn't support rowversion, use manual incrementing)
            entity.Property(x => x.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValue(new byte[] { 0 });

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
