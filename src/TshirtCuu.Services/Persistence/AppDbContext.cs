using Microsoft.EntityFrameworkCore;
using TshirtCuu.Domain.Features.Admin.Models;
using TshirtCuu.Domain.Features.Catalog.Models;
using TshirtCuu.Domain.Features.Quotes.Models;

namespace TshirtCuu.Services.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<ShirtLine> ShirtLines => Set<ShirtLine>();
    public DbSet<CatalogCategory> Categories => Set<CatalogCategory>();
    public DbSet<CatalogDesign> Designs => Set<CatalogDesign>();
    public DbSet<ShirtSize> Sizes => Set<ShirtSize>();
    public DbSet<ShirtColor> Colors => Set<ShirtColor>();
    public DbSet<PricingRules> PricingRules => Set<PricingRules>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatalogCategory>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<CatalogDesign>(e =>
        {
            e.ToTable("Designs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CategoryId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Tagline).HasMaxLength(300).IsRequired();
            e.Property(x => x.AccentHex).HasMaxLength(16);
            e.Property(x => x.InkHex).HasMaxLength(16);
            e.HasIndex(x => x.CategoryId);
        });

        modelBuilder.Entity<ShirtSize>(e =>
        {
            e.ToTable("Sizes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.Label).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<ShirtColor>(e =>
        {
            e.ToTable("Colors");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Hex).HasMaxLength(16).IsRequired();
        });

        modelBuilder.Entity<PricingRules>(e =>
        {
            e.ToTable("PricingRules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<AdminUser>(e =>
        {
            e.ToTable("AdminUsers");
            e.HasKey(x => x.Username);
            e.Property(x => x.Username).HasMaxLength(100);
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Role).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<Quotation>(e =>
        {
            e.ToTable("Quotations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(64).IsRequired();
            e.Property(x => x.Status).HasMaxLength(64).IsRequired();
            e.Ignore(x => x.TotalUnits);
            e.HasMany(x => x.Shirts)
                .WithOne()
                .HasForeignKey(s => s.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShirtLine>(e =>
        {
            e.ToTable("ShirtLines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.QuotationId).HasMaxLength(32).IsRequired();
            e.Property(x => x.ColorId).HasMaxLength(64).IsRequired();
            e.Property(x => x.ColorName).HasMaxLength(100).IsRequired();
            e.Property(x => x.ColorHex).HasMaxLength(16).IsRequired();
            e.Property(x => x.SizeId).HasMaxLength(32).IsRequired();
            e.Property(x => x.SizeLabel).HasMaxLength(32).IsRequired();
            e.Property(x => x.FrontLabel).HasMaxLength(300);
            e.Property(x => x.FrontCatalogId).HasMaxLength(64);
            e.Property(x => x.BackLabel).HasMaxLength(300);
            e.Property(x => x.BackCatalogId).HasMaxLength(64);
            e.Property(x => x.FrontSource).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.BackSource).HasConversion<string>().HasMaxLength(32);

            e.OwnsOne(x => x.FrontPlacement, p =>
            {
                p.Property(a => a.Dx).HasColumnName("FrontDx");
                p.Property(a => a.Dy).HasColumnName("FrontDy");
                p.Property(a => a.Scale).HasColumnName("FrontScale");
            });
            e.OwnsOne(x => x.BackPlacement, p =>
            {
                p.Property(a => a.Dx).HasColumnName("BackDx");
                p.Property(a => a.Dy).HasColumnName("BackDy");
                p.Property(a => a.Scale).HasColumnName("BackScale");
            });
            e.Navigation(x => x.FrontPlacement).IsRequired();
            e.Navigation(x => x.BackPlacement).IsRequired();
        });
    }
}
