using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Utilities;
using Notism.Domain.AuditLog;
using Notism.Domain.Blog;
using Notism.Domain.BlogEventMention;
using Notism.Domain.BlogMedia;
using Notism.Domain.ContentVersion;
using Notism.Domain.Event;
using Notism.Domain.EventMedia;
using Notism.Domain.MediaAsset;
using Notism.Domain.Period;
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
    public DbSet<Period> Periods { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<EventMedia> EventMedia { get; set; }
    public DbSet<BlogMedia> BlogMedia { get; set; }
    public DbSet<BlogEventMention> BlogEventMentions { get; set; }
    public DbSet<ContentVersion> ContentVersions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

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

        ConfigurePeriod(modelBuilder);
        ConfigureEvent(modelBuilder);
        ConfigureBlog(modelBuilder);
        ConfigureMediaAsset(modelBuilder);
        ConfigureEventMedia(modelBuilder);
        ConfigureBlogMedia(modelBuilder);
        ConfigureBlogEventMention(modelBuilder);
        ConfigureContentVersion(modelBuilder);
        ConfigureAuditLog(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigurePeriod(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Period>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(p => p.StartYear)
                .IsRequired();

            entity.Property(p => p.EndYear)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasColumnType("text");

            entity.Property(p => p.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(p => p.IsPublished)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(p => p.CreatedAt)
                .IsRequired();

            entity.Property(p => p.UpdatedAt)
                .IsRequired();

            entity.Property(p => p.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(p => new { p.IsPublished, p.IsDeleted, p.DisplayOrder });

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<MediaAsset>()
                .WithMany()
                .HasForeignKey(p => p.ThumbnailMediaAssetId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PeriodId)
                .IsRequired();

            entity.Property(e => e.Title)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(e => e.ShortDescription)
                .HasMaxLength(500);

            entity.Property(e => e.Description)
                .HasColumnType("text");

            entity.Property(e => e.EventDate)
                .HasColumnType("date");

            entity.Property(e => e.EventYear)
                .IsRequired();

            entity.Property(e => e.IsApproximateDate)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.Latitude)
                .HasPrecision(10, 8);

            entity.Property(e => e.Longitude)
                .HasPrecision(11, 8);

            entity.Property(e => e.LocationName)
                .HasMaxLength(200);

            entity.Property(e => e.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.IsPublished)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(e => e.PeriodId);
            entity.HasIndex(e => new { e.EventYear, e.DisplayOrder });
            entity.HasIndex(e => new { e.PeriodId, e.IsPublished, e.IsDeleted });

            // Geographic index (PostgreSQL GIST)
            if (modelBuilder.IsConfiguredForPostgres())
            {
                entity.HasIndex(e => new { e.Latitude, e.Longitude })
                    .HasMethod("gist");
            }

            entity.HasOne<Period>()
                .WithMany()
                .HasForeignKey(e => e.PeriodId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<MediaAsset>()
                .WithMany()
                .HasForeignKey(e => e.ThumbnailMediaAssetId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureBlog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Title)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(b => b.Slug)
                .HasMaxLength(350)
                .IsRequired();

            entity.Property(b => b.ShortDescription)
                .HasMaxLength(500);

            entity.Property(b => b.Content)
                .HasColumnType("text")
                .IsRequired();

            entity.Property(b => b.AuthorName)
                .HasMaxLength(200);

            entity.Property(b => b.MetaTitle)
                .HasMaxLength(200);

            entity.Property(b => b.MetaDescription)
                .HasMaxLength(500);

            entity.Property(b => b.ViewCount)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(b => b.IsPublished)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(b => b.CreatedAt)
                .IsRequired();

            entity.Property(b => b.UpdatedAt)
                .IsRequired();

            entity.Property(b => b.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(b => b.Tags)
                .HasColumnType("varchar[]")
                .HasConversion(
                    v => v.ToArray(),
                    v => v.ToList());

            entity.HasIndex(b => b.Slug)
                .IsUnique();

            entity.HasIndex(b => b.AuthorId);
            entity.HasIndex(b => new { b.IsPublished, b.IsDeleted, b.PublishedAt });

            // Full-text search index (PostgreSQL GIN)
            if (modelBuilder.IsConfiguredForPostgres())
            {
                entity.HasIndex(b => new { b.Title, b.Content })
                    .HasMethod("gin")
                    .HasOperators("gin_trgm_ops");
            }

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(b => b.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(b => b.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<MediaAsset>()
                .WithMany()
                .HasForeignKey(b => b.ThumbnailMediaAssetId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureMediaAsset(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.FileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(m => m.StoragePath)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(m => m.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(m => m.MediaType)
                .HasMaxLength(20)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(m => m.FileSize)
                .IsRequired();

            entity.Property(m => m.ThumbnailPath)
                .HasMaxLength(1000);

            entity.Property(m => m.AltText)
                .HasMaxLength(500);

            entity.Property(m => m.Caption)
                .HasMaxLength(500);

            entity.Property(m => m.Tags)
                .HasColumnType("varchar[]")
                .HasConversion(
                    v => v.ToArray(),
                    v => v.ToList());

            entity.Property(m => m.CreatedAt)
                .IsRequired();

            entity.Property(m => m.UpdatedAt)
                .IsRequired();

            entity.Property(m => m.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(m => m.CreatedBy);

            // Full-text search index on tags (PostgreSQL GIN)
            if (modelBuilder.IsConfiguredForPostgres())
            {
                entity.HasIndex(m => m.Tags)
                    .HasMethod("gin");
            }

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureEventMedia(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventMedia>(entity =>
        {
            entity.HasKey(em => em.Id);

            entity.Property(em => em.EventId)
                .IsRequired();

            entity.Property(em => em.MediaAssetId)
                .IsRequired();

            entity.Property(em => em.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(em => em.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(em => em.UsageType)
                .HasMaxLength(20)
                .HasConversion<string>()
                .IsRequired()
                .HasDefaultValue("General");

            entity.Property(em => em.CreatedAt)
                .IsRequired();

            entity.Property(em => em.UpdatedAt)
                .IsRequired();

            entity.Property(em => em.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(em => em.EventId);
            entity.HasIndex(em => em.MediaAssetId);
            entity.HasIndex(em => new { em.EventId, em.DisplayOrder });
            entity.HasIndex(em => new { em.EventId, em.MediaAssetId })
                .IsUnique();

            entity.HasOne<Event>()
                .WithMany()
                .HasForeignKey(em => em.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<MediaAsset>()
                .WithMany()
                .HasForeignKey(em => em.MediaAssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBlogMedia(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogMedia>(entity =>
        {
            entity.HasKey(bm => bm.Id);

            entity.Property(bm => bm.BlogId)
                .IsRequired();

            entity.Property(bm => bm.MediaAssetId)
                .IsRequired();

            entity.Property(bm => bm.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(bm => bm.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(bm => bm.UsageType)
                .HasMaxLength(20)
                .HasConversion<string>()
                .IsRequired()
                .HasDefaultValue("Content");

            entity.Property(bm => bm.CreatedAt)
                .IsRequired();

            entity.Property(bm => bm.UpdatedAt)
                .IsRequired();

            entity.Property(bm => bm.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(bm => bm.BlogId);
            entity.HasIndex(bm => bm.MediaAssetId);
            entity.HasIndex(bm => new { bm.BlogId, bm.DisplayOrder });
            entity.HasIndex(bm => new { bm.BlogId, bm.MediaAssetId })
                .IsUnique();

            entity.HasOne<Blog>()
                .WithMany()
                .HasForeignKey(bm => bm.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<MediaAsset>()
                .WithMany()
                .HasForeignKey(bm => bm.MediaAssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBlogEventMention(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogEventMention>(entity =>
        {
            entity.HasKey(bem => bem.Id);

            entity.Property(bem => bem.BlogId)
                .IsRequired();

            entity.Property(bem => bem.EventId)
                .IsRequired();

            entity.Property(bem => bem.MentionOrder)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(bem => bem.Context)
                .HasMaxLength(500);

            entity.Property(bem => bem.CreatedAt)
                .IsRequired();

            entity.Property(bem => bem.UpdatedAt)
                .IsRequired();

            entity.Property(bem => bem.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(bem => bem.BlogId);
            entity.HasIndex(bem => bem.EventId);
            entity.HasIndex(bem => new { bem.BlogId, bem.MentionOrder });
            entity.HasIndex(bem => new { bem.BlogId, bem.EventId })
                .IsUnique();

            entity.HasOne<Blog>()
                .WithMany()
                .HasForeignKey(bem => bem.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Event>()
                .WithMany()
                .HasForeignKey(bem => bem.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureContentVersion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentVersion>(entity =>
        {
            entity.HasKey(cv => cv.Id);

            entity.Property(cv => cv.EntityType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(cv => cv.EntityId)
                .IsRequired();

            entity.Property(cv => cv.VersionNumber)
                .IsRequired();

            entity.Property(cv => cv.Content)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => v.RootElement.GetRawText(),
                    v => JsonDocument.Parse(v))
                .IsRequired();

            entity.Property(cv => cv.ChangeDescription)
                .HasMaxLength(500);

            entity.Property(cv => cv.CreatedAt)
                .IsRequired();

            entity.Property(cv => cv.UpdatedAt)
                .IsRequired();

            entity.Property(cv => cv.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(cv => new { cv.EntityType, cv.EntityId, cv.VersionNumber });

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(cv => cv.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(al => al.Id);

            entity.Property(al => al.Action)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(al => al.EntityType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(al => al.Changes)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : v.RootElement.GetRawText(),
                    v => v == null ? null : JsonDocument.Parse(v));

            entity.Property(al => al.IpAddress)
                .HasMaxLength(45);

            entity.Property(al => al.UserAgent)
                .HasMaxLength(500);

            entity.Property(al => al.CreatedAt)
                .IsRequired();

            entity.Property(al => al.UpdatedAt)
                .IsRequired();

            entity.Property(al => al.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(al => al.UserId);
            entity.HasIndex(al => new { al.EntityType, al.EntityId });
            entity.HasIndex(al => new { al.UserId, al.CreatedAt });

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

// Extension method to check if PostgreSQL is configured
internal static class ModelBuilderExtensions
{
    public static bool IsConfiguredForPostgres(this ModelBuilder modelBuilder)
    {
        // This is a simple check - in practice, you might want to check the actual database provider
        // For now, we'll assume PostgreSQL is being used based on the project setup
        return true;
    }
}