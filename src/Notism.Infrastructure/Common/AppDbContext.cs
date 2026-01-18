using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Utilities;
using Notism.Domain.RefreshToken;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Domain.User.ValueObjects;

namespace Notism.Infrastructure.Common;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

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
                    role => EnumConverter.ToCamelCase(role),
                    value => EnumConverter.FromString<UserRole>(value) ?? UserRole.User)
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
}