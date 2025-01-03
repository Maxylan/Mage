using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace Reception.Models;

public partial class MageDbContext : DbContext
{
    ILogger<MageDbContext> _logger;

    public MageDbContext(ILogger<MageDbContext> logger)
    {
        _logger = logger;
    }

    public MageDbContext(DbContextOptions<MageDbContext> options, ILogger<MageDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    public virtual DbSet<Account> Accounts { get; set; } = null!;
    public virtual DbSet<Album> Albums { get; set; } = null!;
    public virtual DbSet<Category> Categories { get; set; } = null!;
    public virtual DbSet<Filepath> Filepaths { get; set; } = null!;
    public virtual DbSet<Log> Logs { get; set; } = null!;
    public virtual DbSet<Photo> Photos { get; set; } = null!;
    public virtual DbSet<Session> Sessions { get; set; } = null!;
    public virtual DbSet<Tag> Tags { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            StringBuilder sb = new();
            sb.AppendFormat("Host={0};", Environment.GetEnvironmentVariable("STORAGE_URL"));
            sb.AppendFormat("Database={0};", Environment.GetEnvironmentVariable("POSTGRES_DB"));
            sb.AppendFormat("Username={0};", Environment.GetEnvironmentVariable("POSTGRES_USER"));
            sb.AppendFormat("Password={0}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"));
            optionsBuilder.UseNpgsql(sb.ToString());
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<DataTypes.Dimension>("magedb", "dimension") // new[] { "THUMBNAIL", "MEDIUM", "SOURCE" }
            .HasPostgresEnum<DataTypes.Method>("magedb", "method") // new[] { "HEAD", "GET", "POST", "PUT", "PATCH", "DELETE" }
            .HasPostgresEnum<DataTypes.Severity>("magedb", "severity") // new[] { "TRACE", "DEBUG", "INFORMATION", "SUSPICIOUS", "WARNING", "ERROR", "CRITICAL" }
            .HasPostgresEnum<DataTypes.Source>("magedb", "source", new NpgsqlNullNameTranslator()); // new[] { "INTERNAL", "EXTERNAL" }

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("accounts_pkey");

            entity.ToTable("accounts", "magedb");

            entity.HasIndex(e => e.Email, "accounts_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "accounts_username_key").IsUnique();

            entity.HasIndex(e => e.Email, "idx_accounts_email").IsUnique();

            entity.HasIndex(e => e.LastVisit, "idx_accounts_last_visit");

            entity.HasIndex(e => e.Username, "idx_accounts_username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(127)
                .HasColumnName("full_name");
            entity.Property(e => e.LastVisit)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_visit");
            entity.Property(e => e.Password)
                .HasMaxLength(127)
                .HasColumnName("password");
            entity.Property(e => e.Permissions)
                .HasDefaultValue((short)0)
                .HasColumnName("permissions");
            entity.Property(e => e.Username)
                .HasMaxLength(63)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("albums_pkey");

            entity.ToTable("albums", "magedb");

            entity.HasIndex(e => e.Title, "albums_title_key").IsUnique();

            entity.HasIndex(e => e.Title, "idx_albums_title");

            entity.HasIndex(e => e.UpdatedAt, "idx_albums_updated_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Summary)
                .HasMaxLength(255)
                .HasColumnName("summary");
            entity.Property(e => e.ThumbnailId).HasColumnName("thumbnail_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Albums)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_category");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Albums)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user");

            entity.HasOne(d => d.Thumbnail).WithMany(p => p.Albums)
                .HasForeignKey(d => d.ThumbnailId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_thumbnail");

            entity.HasMany(d => d.Tags).WithMany(p => p.Albums)
                .UsingEntity<Dictionary<string, object>>(
                    "AlbumTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("fk_tag"),
                    l => l.HasOne<Album>().WithMany()
                        .HasForeignKey("AlbumId")
                        .HasConstraintName("fk_album"),
                    j =>
                    {
                        j.HasKey("AlbumId", "TagId").HasName("album_tags_pkey");
                        j.ToTable("album_tags", "magedb");
                        j.HasIndex(new[] { "AlbumId" }, "idx_album_tags_album_id");
                        j.HasIndex(new[] { "TagId" }, "idx_album_tags_tag_id");
                        j.IndexerProperty<int>("AlbumId").HasColumnName("album_id");
                        j.IndexerProperty<int>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories", "magedb");

            entity.HasIndex(e => e.Title, "categories_title_key").IsUnique();

            entity.HasIndex(e => e.Title, "idx_categories_title");

            entity.HasIndex(e => e.UpdatedAt, "idx_categories_updated_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Summary)
                .HasMaxLength(255)
                .HasColumnName("summary");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Categories)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user");
        });

        modelBuilder.Entity<Filepath>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("filepaths_pkey");

            entity.ToTable("filepaths", "magedb");

            entity.HasIndex(e => e.Filename, "filepaths_filename_key").IsUnique();

            entity.HasIndex(e => e.Filename, "idx_filepaths_filename");

            entity.HasIndex(e => e.PhotoId, "idx_filepaths_photo_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Filename)
                .HasMaxLength(127)
                .HasColumnName("filename");
            entity.Property(e => e.Filesize).HasColumnName("filesize");
            entity.Property(e => e.PhotoId).HasColumnName("photo_id");

            entity.HasOne(d => d.Photo).WithMany(p => p.Filepaths)
                .HasForeignKey(d => d.PhotoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_photo");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("logs_pkey");

            entity.ToTable("logs", "magedb");

            entity.HasIndex(e => e.CreatedAt, "idx_logs_created_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(255)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Log1).HasColumnName("log");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("user_email");
            entity.Property(e => e.UserFullName)
                .HasMaxLength(127)
                .HasColumnName("user_full_name");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserUsername)
                .HasMaxLength(63)
                .HasColumnName("user_username");
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("photos_pkey");

            entity.ToTable("photos", "magedb");

            entity.HasIndex(e => e.Name, "idx_photos_name");

            entity.HasIndex(e => e.UpdatedAt, "idx_photos_updated_at");

            entity.HasIndex(e => e.Name, "photos_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(127)
                .HasColumnName("name");
            entity.Property(e => e.Summary)
                .HasMaxLength(255)
                .HasColumnName("summary");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Photos)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user");

            entity.HasMany(d => d.AlbumsNavigation).WithMany(p => p.Photos)
                .UsingEntity<Dictionary<string, object>>(
                    "PhotoAlbum",
                    r => r.HasOne<Album>().WithMany()
                        .HasForeignKey("AlbumId")
                        .HasConstraintName("fk_album"),
                    l => l.HasOne<Photo>().WithMany()
                        .HasForeignKey("PhotoId")
                        .HasConstraintName("fk_photo"),
                    j =>
                    {
                        j.HasKey("PhotoId", "AlbumId").HasName("photo_albums_pkey");
                        j.ToTable("photo_albums", "magedb");
                        j.HasIndex(new[] { "AlbumId" }, "idx_photo_albums_album_id");
                        j.HasIndex(new[] { "PhotoId" }, "idx_photo_albums_photo_id");
                        j.IndexerProperty<int>("PhotoId").HasColumnName("photo_id");
                        j.IndexerProperty<int>("AlbumId").HasColumnName("album_id");
                    });

            entity.HasMany(d => d.Tags).WithMany(p => p.Photos)
                .UsingEntity<Dictionary<string, object>>(
                    "PhotoTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("fk_tag"),
                    l => l.HasOne<Photo>().WithMany()
                        .HasForeignKey("PhotoId")
                        .HasConstraintName("fk_photo"),
                    j =>
                    {
                        j.HasKey("PhotoId", "TagId").HasName("photo_tags_pkey");
                        j.ToTable("photo_tags", "magedb");
                        j.HasIndex(new[] { "PhotoId" }, "idx_photo_tags_photo_id");
                        j.HasIndex(new[] { "TagId" }, "idx_photo_tags_tag_id");
                        j.IndexerProperty<int>("PhotoId").HasColumnName("photo_id");
                        j.IndexerProperty<int>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sessions_pkey");

            entity.ToTable("sessions", "magedb");

            entity.HasIndex(e => e.UserId, "idx_sessions_user_id");

            entity.HasIndex(e => e.Code, "sessions_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(36)
                .IsFixedLength()
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.ToTable("tags", "magedb");

            entity.HasIndex(e => e.Name, "idx_tags_name");

            entity.HasIndex(e => e.Name, "tags_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(127)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
