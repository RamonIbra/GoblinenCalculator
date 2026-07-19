using GoblinenCalculator.Models;
using Microsoft.EntityFrameworkCore;

namespace GoblinenCalculator.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PriceBracket> PriceBrackets => Set<PriceBracket>();

    public DbSet<CardEntry> CardEntries => Set<CardEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PriceBracket>()
            .Property(b => b.Factor)
            .HasPrecision(10, 4);
        modelBuilder.Entity<PriceBracket>()
            .Property(b => b.UpperBoundEur)
            .HasPrecision(10, 2);

        modelBuilder.Entity<CardEntry>()
            .Property(c => c.BasePriceEur)
            .HasPrecision(10, 2);
    }
}
