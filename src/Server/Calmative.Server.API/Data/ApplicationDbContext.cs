using Calmative.Server.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Calmative.Server.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Portfolio> Portfolios { get; set; } = null!;
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<PriceHistory> PriceHistories { get; set; } = null!;
        public DbSet<CustomAssetType> CustomAssetTypes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Portfolio configuration
            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Portfolios)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Asset configuration
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,8)");
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18,2)");
                
                entity.HasOne(e => e.Portfolio)
                    .WithMany(p => p.Assets)
                    .HasForeignKey(e => e.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PriceHistory configuration
            modelBuilder.Entity<PriceHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Source).HasMaxLength(50);
                
                entity.HasIndex(e => new { e.Symbol, e.Timestamp });
            });

            // CustomAssetType configuration
            modelBuilder.Entity<CustomAssetType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed sample price data for common assets
            modelBuilder.Entity<PriceHistory>().HasData(
                new PriceHistory { Id = 1, Symbol = "USD", AssetType = AssetType.Currency, Price = 1.0m, Timestamp = DateTime.UtcNow, Source = "System" },
                new PriceHistory { Id = 2, Symbol = "EUR", AssetType = AssetType.Currency, Price = 1.1m, Timestamp = DateTime.UtcNow, Source = "System" },
                new PriceHistory { Id = 3, Symbol = "GBP", AssetType = AssetType.Currency, Price = 1.25m, Timestamp = DateTime.UtcNow, Source = "System" },
                new PriceHistory { Id = 4, Symbol = "GOLD", AssetType = AssetType.Gold, Price = 2000.0m, Timestamp = DateTime.UtcNow, Source = "System" },
                new PriceHistory { Id = 5, Symbol = "SILVER", AssetType = AssetType.Silver, Price = 25.0m, Timestamp = DateTime.UtcNow, Source = "System" },
                new PriceHistory { Id = 6, Symbol = "BTC", AssetType = AssetType.Crypto, Price = 45000.0m, Timestamp = DateTime.UtcNow, Source = "System" },
                new PriceHistory { Id = 7, Symbol = "ETH", AssetType = AssetType.Crypto, Price = 3000.0m, Timestamp = DateTime.UtcNow, Source = "System" }
            );
        }
    }
} 