using Microsoft.EntityFrameworkCore;

using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Domain.RefreshToken;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Extensions;

namespace Notism.Infrastructure.Common;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Food> Foods { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
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

            entity.Property(f => f.FileKey)
                .HasMaxLength(500)
                .IsRequired();

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
        });
    }
}