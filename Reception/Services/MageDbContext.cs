using System.Collections.Generic;
using System;
using System.Text;
using Npgsql.NameTranslation;
using Microsoft.EntityFrameworkCore;
using Reception.Models.Entities;
using Npgsql;

namespace Reception.Services;

public partial class MageDbContext : DbContext
{
    public MageDbContext()
    { }

    public MageDbContext(DbContextOptions<MageDbContext> options)
        : base(options) { }

    public virtual DbSet<Account> Accounts { get; set; } = null!;
    public virtual DbSet<Album> Albums { get; set; } = null!;
    public virtual DbSet<Category> Categories { get; set; } = null!;
    public virtual DbSet<Filepath> Filepaths { get; set; } = null!;
    public virtual DbSet<Link> Links { get; set; } = null!;
    public virtual DbSet<LogEntry> Logs { get; set; } = null!;
    public virtual DbSet<PhotoEntity> Photos { get; set; } = null!;
    public virtual DbSet<Session> Sessions { get; set; } = null!;
    public virtual DbSet<Tag> Tags { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string? databaseName = Environment.GetEnvironmentVariable("POSTGRES_DB");
            StringBuilder sb = new();
            sb.AppendFormat("Database={0};", databaseName);
            sb.AppendFormat("Host={0};", Environment.GetEnvironmentVariable("STORAGE_URL"));
            sb.AppendFormat("Username={0};", Environment.GetEnvironmentVariable("POSTGRES_USER"));
            sb.AppendFormat("Password={0}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"));

            optionsBuilder.UseNpgsql(sb.ToString(), opts =>
            {
                INpgsqlNameTranslator nameTranslator = new NpgsqlNullNameTranslator();
                opts.MapEnum<Dimension>("dimension", "magedb", nameTranslator);
                opts.MapEnum<Method>("method", "magedb", nameTranslator);
                opts.MapEnum<Severity>("severity", "magedb", nameTranslator);
                opts.MapEnum<Source>("source", "magedb", nameTranslator);
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<Dimension>("magedb", "dimension") // new[] { "THUMBNAIL", "MEDIUM", "SOURCE" }
            .HasPostgresEnum<Method>("magedb", "method") // new[] { "HEAD", "GET", "POST", "PUT", "PATCH", "DELETE" }
            .HasPostgresEnum<Severity>("magedb", "severity") // new[] { "TRACE", "DEBUG", "INFORMATION", "SUSPICIOUS", "WARNING", "ERROR", "CRITICAL" }
            .HasPostgresEnum<Source>("magedb", "source", new NpgsqlNullNameTranslator()); // new[] { "INTERNAL", "EXTERNAL" }

        modelBuilder.Entity(Account.Build);
        modelBuilder.Entity(Album.Build);
        modelBuilder.Entity(Category.Build);
        modelBuilder.Entity(Filepath.Build);
        modelBuilder.Entity(Link.Build);
        modelBuilder.Entity(LogEntry.Build);
        modelBuilder.Entity(PhotoEntity.Build);
        modelBuilder.Entity(Session.Build);
        modelBuilder.Entity(Tag.Build);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
