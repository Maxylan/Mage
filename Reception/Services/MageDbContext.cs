﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;
using Reception.Models;

namespace Reception.Services;

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
    public virtual DbSet<LogEntry> Logs { get; set; } = null!;
    public virtual DbSet<Photo> Photos { get; set; } = null!;
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
            optionsBuilder.UseNpgsql(sb.ToString());

            _logger.LogTrace("Configured Database '{}'.", databaseName);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<DataTypes.Dimension>("magedb", "dimension") // new[] { "THUMBNAIL", "MEDIUM", "SOURCE" }
            .HasPostgresEnum<DataTypes.Method>("magedb", "method") // new[] { "HEAD", "GET", "POST", "PUT", "PATCH", "DELETE" }
            .HasPostgresEnum<DataTypes.Severity>("magedb", "severity") // new[] { "TRACE", "DEBUG", "INFORMATION", "SUSPICIOUS", "WARNING", "ERROR", "CRITICAL" }
            .HasPostgresEnum<DataTypes.Source>("magedb", "source", new NpgsqlNullNameTranslator()); // new[] { "INTERNAL", "EXTERNAL" }

        modelBuilder.Entity(Account.Build);
        modelBuilder.Entity(Album.Build);
        modelBuilder.Entity(Category.Build);
        modelBuilder.Entity(Filepath.Build);
        modelBuilder.Entity(LogEntry.Build);
        modelBuilder.Entity(Photo.Build);
        modelBuilder.Entity(Session.Build);
        modelBuilder.Entity(Tag.Build);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
