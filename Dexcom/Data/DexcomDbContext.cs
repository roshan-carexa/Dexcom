using Microsoft.EntityFrameworkCore;
using Dexcom.Models;

namespace Dexcom.Data;

public class DexcomDbContext : DbContext
{
    public DexcomDbContext(DbContextOptions<DexcomDbContext> options) : base(options)
    {
    }

    public DbSet<DexcomUserToken> DexcomUserTokens => Set<DexcomUserToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DexcomUserToken>(entity =>
        {
            entity.ToTable("DexcomUserTokens");
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.AccessToken).IsRequired();
            entity.Property(e => e.RefreshToken).IsRequired();
            entity.Property(e => e.ExpiresAtUtc).IsRequired();
        });
    }
}
