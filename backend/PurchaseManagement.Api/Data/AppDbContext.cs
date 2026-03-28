using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Api.Entities;

namespace PurchaseManagement.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<PurchaseBill> PurchaseBills => Set<PurchaseBill>();
    public DbSet<PurchaseBillItem> PurchaseBillItems => Set<PurchaseBillItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PurchaseBillItem>()
            .HasOne(pi => pi.PurchaseBill)
            .WithMany(b => b.Items)
            .HasForeignKey(pi => pi.PurchaseBillId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseBillItem>()
            .HasOne(pi => pi.Item)
            .WithMany()
            .HasForeignKey(pi => pi.ItemId);

        modelBuilder.Entity<PurchaseBillItem>()
            .HasOne(pi => pi.Location)
            .WithMany()
            .HasForeignKey(pi => pi.LocationId)
            .HasPrincipalKey(l => l.LocationId);
    }
}
