using Microsoft.EntityFrameworkCore;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentRequest> PaymentRequests => Set<PaymentRequest>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShippingQuote> ShippingQuotes => Set<ShippingQuote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(120);
            e.Property(p => p.Description).IsRequired().HasMaxLength(500);
            e.Property(p => p.Price).HasColumnType("decimal(12,2)");
            e.Property(p => p.ImageUrl).HasMaxLength(500);
            e.HasOne(p => p.Category)
              .WithMany(c => c.Products!)
              .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired().HasMaxLength(255);
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            e.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            e.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            e.Property(u => u.Role).HasConversion<int>();
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.Property(c => c.Phone).HasMaxLength(20);
            e.Property(c => c.ShippingAddress).HasMaxLength(500);
        }); modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.ShippingCost).HasColumnType("decimal(12,2)");
            e.Property(o => o.ShippingAddress).HasMaxLength(500);
            e.Property(o => o.ShippingService).HasMaxLength(100);
            e.HasOne(o => o.Customer)
              .WithMany(c => c.Orders)
              .HasForeignKey(o => o.CustomerId);
            e.HasMany(o => o.Items)
              .WithOne()
              .HasForeignKey(oi => oi.OrderId);
        }); modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(oi => oi.Id);
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(12,2)");
            e.HasOne(oi => oi.Product)
              .WithMany()
              .HasForeignKey(oi => oi.ProductId);
        }); modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Amount).HasColumnType("decimal(12,2)");
            e.Property(p => p.ExternalId).HasMaxLength(100);
        });

        modelBuilder.Entity<PaymentRequest>(e =>
        {
            e.HasKey(pr => pr.Id);
            e.Property(pr => pr.Amount).HasColumnType("decimal(12,2)");
            e.Property(pr => pr.Currency).IsRequired().HasMaxLength(3);
            e.Property(pr => pr.PaymentMethod).IsRequired().HasMaxLength(50);
            e.Property(pr => pr.CustomerEmail).IsRequired().HasMaxLength(255);
            e.Property(pr => pr.ErrorMessage).HasMaxLength(1000);
            e.Property(pr => pr.ExternalPaymentId).HasMaxLength(100);
            e.Property(pr => pr.Status).HasConversion<int>();
        });

        modelBuilder.Entity<Shipment>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Address).IsRequired().HasMaxLength(500);
            e.Property(s => s.TrackingCode).IsRequired().HasMaxLength(50);
        });
    }
}