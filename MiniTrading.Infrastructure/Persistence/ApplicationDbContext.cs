using Microsoft.EntityFrameworkCore;
using MiniTrading.Domain.Constants;
using MiniTrading.Domain.Entities;

namespace MiniTrading.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Trade> Trades => Set<Trade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trade>(builder =>
        {
            builder.ToTable("Trades");
            builder.HasKey(t => t.TradeId);

            builder.Property(t => t.TradeId)
                .HasMaxLength(AppConstant.TradeRules.TradeIdMaxLength)
                .IsRequired();

            builder.Property(t => t.Symbol)
                .HasMaxLength(AppConstant.TradeRules.SymbolMaxLength)
                .IsRequired();

            builder.Property(t => t.Side)
                .IsRequired();

            builder.Property(t => t.Quantity)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(t => t.Price)
                .HasPrecision(18, 5)
                .IsRequired();

            builder.Property(t => t.Timestamp)
                .IsRequired();

            builder.Property(t => t.Status)
                .IsRequired();
        });
    }
}