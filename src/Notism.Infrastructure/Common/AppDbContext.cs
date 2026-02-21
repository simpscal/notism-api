using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Domain.Cart;
using Notism.Domain.Common;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.RefreshToken;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Extensions;

namespace Notism.Infrastructure.Common;

public class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Food> Foods { get; set; }
    public DbSet<FoodImage> FoodImages { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<DeliveryStatusHistory> DeliveryStatusHistories { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = CollectDomainEvents();
        var result = await base.SaveChangesAsync(cancellationToken);

        await DispatchDomainEventsAsync(domainEvents, cancellationToken);

        return result;
    }

    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUser(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigurePasswordResetToken(modelBuilder);
        ConfigureFood(modelBuilder);
        ConfigureFoodImage(modelBuilder);
        ConfigureCartItem(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureOrderItem(modelBuilder);
        ConfigureDeliveryStatusHistory(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email)
                .HasConversion(
                    email => email.Value,
                    value => Email.Create(value))
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.Password)
                .HasConversion(
                    password => password.Value,
                    hash => Password.Create(hash))
                .HasColumnName("PasswordHash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.Role)
                .HasConversion(
                    role => role.ToCamelCase(),
                    value => value.FromCamelCase<UserRole>() ?? UserRole.User)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(u => u.FirstName)
                .HasMaxLength(50);

            entity.Property(u => u.LastName)
                .HasMaxLength(50);

            entity.Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            entity.Property(u => u.CreatedAt)
                .IsRequired();

            entity.Property(u => u.UpdatedAt)
                .IsRequired();
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.Token)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(rt => rt.Token)
                .IsUnique();

            entity.Property(rt => rt.UserId)
                .IsRequired();

            entity.Property(rt => rt.ExpiresAt)
                .IsRequired();

            entity.Property(rt => rt.IsRevoked)
                .IsRequired();

            entity.Property(rt => rt.CreatedAt)
                .IsRequired();

            entity.HasIndex(rt => rt.UserId);
            entity.HasIndex(rt => new { rt.UserId, rt.IsRevoked });
        });
    }

    private static void ConfigurePasswordResetToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(prt => prt.Id);

            entity.Property(prt => prt.Token)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(prt => prt.Token)
                .IsUnique();

            entity.Property(prt => prt.UserId)
                .IsRequired();

            entity.Property(prt => prt.ExpiresAt)
                .IsRequired();

            entity.Property(prt => prt.IsUsed)
                .IsRequired();

            entity.Property(prt => prt.CreatedAt)
                .IsRequired();

            entity.HasIndex(prt => prt.UserId);
            entity.HasIndex(prt => new { prt.UserId, prt.IsUsed, prt.ExpiresAt });
        });
    }

    private static void ConfigureFood(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(f => f.Description)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(f => f.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(f => f.DiscountPrice)
                .HasPrecision(18, 2);

            entity.Property(f => f.Category)
                .HasConversion(
                    category => category.GetStringValue(),
                    value => value.ToEnum<FoodCategory>())
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(f => f.IsAvailable)
                .IsRequired();

            entity.Property(f => f.QuantityUnit)
                .HasConversion(
                    quantityUnit => quantityUnit.GetStringValue(),
                    value => value.ToEnum<QuantityUnit>())
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(f => f.StockQuantity)
                .IsRequired();

            entity.Property(f => f.CreatedAt)
                .IsRequired();

            entity.Property(f => f.UpdatedAt)
                .IsRequired();

            entity.HasIndex(f => f.Category);
            entity.HasIndex(f => f.IsAvailable);
            entity.HasIndex(f => new { f.Category, f.IsAvailable });

            entity.HasMany(f => f.Images)
                .WithOne(i => i.Food)
                .HasForeignKey(i => i.FoodId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureFoodImage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FoodImage>(entity =>
        {
            entity.HasKey(fi => fi.Id);

            entity.Property(fi => fi.FileKey)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(fi => fi.FoodId)
                .IsRequired();

            entity.Property(fi => fi.DisplayOrder)
                .IsRequired();

            entity.Property(fi => fi.AltText)
                .HasMaxLength(200);

            entity.Property(fi => fi.CreatedAt)
                .IsRequired();

            entity.Property(fi => fi.UpdatedAt)
                .IsRequired();

            entity.HasIndex(fi => fi.FoodId);
            entity.HasIndex(fi => new { fi.FoodId, fi.DisplayOrder });
        });
    }

    private static void ConfigureCartItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);

            entity.Property(ci => ci.UserId)
                .IsRequired();

            entity.Property(ci => ci.FoodId)
                .IsRequired();

            entity.Property(ci => ci.Quantity)
                .IsRequired();

            entity.Property(ci => ci.CreatedAt)
                .IsRequired();

            entity.Property(ci => ci.UpdatedAt)
                .IsRequired();

            entity.HasIndex(ci => ci.UserId);
            entity.HasIndex(ci => new { ci.UserId, ci.FoodId })
                .IsUnique();

            entity.HasOne(ci => ci.Food)
                .WithMany()
                .HasForeignKey(ci => ci.FoodId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.UserId)
                .IsRequired();

            entity.Property(o => o.SlugId)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(o => o.PaymentMethod)
                .HasConversion(
                    method => method.GetStringValue(),
                    value => value.ToEnum<PaymentMethod>())
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(o => o.DeliveryStatus)
                .HasConversion(
                    status => status.GetStringValue(),
                    value => value.ToEnum<DeliveryStatus>())
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(o => o.CreatedAt)
                .IsRequired();

            entity.Property(o => o.UpdatedAt)
                .IsRequired();

            entity.HasIndex(o => o.UserId);
            entity.HasIndex(o => o.SlugId)
                .IsUnique();
            entity.HasIndex(o => o.DeliveryStatus);
            entity.HasIndex(o => new { o.UserId, o.CreatedAt });

            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.StatusHistory)
                .WithOne(h => h.Order)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrderItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);

            entity.Property(oi => oi.OrderId)
                .IsRequired();

            entity.Property(oi => oi.FoodId)
                .IsRequired();

            entity.Property(oi => oi.FoodName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(oi => oi.DiscountPrice)
                .HasPrecision(18, 2);

            entity.Property(oi => oi.Quantity)
                .IsRequired();

            entity.Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(oi => oi.CreatedAt)
                .IsRequired();

            entity.Property(oi => oi.UpdatedAt)
                .IsRequired();

            entity.HasIndex(oi => oi.OrderId);
            entity.HasIndex(oi => oi.FoodId);

            entity.HasOne(oi => oi.Food)
                .WithMany()
                .HasForeignKey(oi => oi.FoodId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDeliveryStatusHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeliveryStatusHistory>(entity =>
        {
            entity.HasKey(dsh => dsh.Id);

            entity.Property(dsh => dsh.OrderId)
                .IsRequired();

            entity.Property(dsh => dsh.Status)
                .HasConversion(
                    status => status.GetStringValue(),
                    value => value.ToEnum<DeliveryStatus>())
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(dsh => dsh.StatusChangedAt)
                .IsRequired();

            entity.Property(dsh => dsh.CreatedAt)
                .IsRequired();

            entity.Property(dsh => dsh.UpdatedAt)
                .IsRequired();

            entity.HasIndex(dsh => dsh.OrderId);
            entity.HasIndex(dsh => new { dsh.OrderId, dsh.StatusChangedAt });
        });
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var aggregatesWithEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        var domainEvents = aggregatesWithEvents
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        aggregatesWithEvents.ForEach(aggregate => aggregate.ClearDomainEvents());

        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(List<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            if (domainEvent is INotification notification)
            {
                await mediator.Publish(notification, cancellationToken);
            }
        }
    }
}