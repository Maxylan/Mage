using System.Text;
using Npgsql.NameTranslation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Reception.Database.Models;
using Npgsql;

namespace Reception.Database;

public partial class MageDb : DbContext
{
    private (CancellationTokenSource, CancellationToken) cancelOnDispose;

    public MageDb()
    {
        CancellationTokenSource source = new();
        CancellationToken token = source.Token;
        this.cancelOnDispose = (
            source,
            token
        );
    }

    public MageDb(DbContextOptions<MageDb> options)
        : base(options)
    {
        CancellationTokenSource source = new();
        CancellationToken token = source.Token;
        this.cancelOnDispose = (
            source,
            token
        );
    }

    public override void Dispose()
    {
        this.cancelOnDispose.Item1.Cancel();
        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        this.cancelOnDispose.Item1.Cancel();
        return base.DisposeAsync();
    }

    public CancellationToken CancellationToken
    {
        get => this.cancelOnDispose.Item2;
    }

    public virtual DbSet<Account> Accounts { get; set; } = null!;
    public virtual DbSet<Album> Albums { get; set; } = null!;
    public virtual DbSet<AlbumTagRelation> AlbumTags { get; set; } = null!;
    public virtual DbSet<BanEntry> BannedClients { get; set; } = null!;
    public virtual DbSet<Category> Categories { get; set; } = null!;
    public virtual DbSet<Client> Clients { get; set; } = null!;
    public virtual DbSet<FavoriteAlbumRelation> FavoriteAlbums { get; set; } = null!;
    public virtual DbSet<FavoritePhotoRelation> FavoritePhotos { get; set; } = null!;
    public virtual DbSet<Filepath> Filepaths { get; set; } = null!;
    public virtual DbSet<LogEntry> Logs { get; set; } = null!;
    public virtual DbSet<Photo> Photos { get; set; } = null!;
    public virtual DbSet<PublicLink> Links { get; set; } = null!;
    public virtual DbSet<PhotoAlbumRelation> PhotoAlbums { get; set; } = null!;
    public virtual DbSet<PhotoTagRelation> PhotoTags { get; set; } = null!;
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

            // I give up..
            optionsBuilder.ConfigureWarnings(opts =>
            {
                opts.Log(CoreEventId.ManyServiceProvidersCreatedWarning);
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
        /* modelBuilder
            .HasPostgresEnum("magedb", "dimension", new[] { "THUMBNAIL", "MEDIUM", "SOURCE" })
            .HasPostgresEnum("magedb", "method", new[] { "UNKNOWN", "HEAD", "GET", "POST", "PUT", "PATCH", "DELETE" })
            .HasPostgresEnum("magedb", "severity", new[] { "TRACE", "DEBUG", "INFORMATION", "SUSPICIOUS", "WARNING", "ERROR", "CRITICAL" })
            .HasPostgresEnum("magedb", "source", new[] { "INTERNAL", "EXTERNAL" }); */

        modelBuilder.Entity(Account.Build);
        modelBuilder.Entity(Album.Build);
        modelBuilder.Entity(AlbumTagRelation.Build);
        modelBuilder.Entity(BanEntry.Build);
        modelBuilder.Entity(Category.Build);
        modelBuilder.Entity(Client.Build);
        modelBuilder.Entity(FavoriteAlbumRelation.Build);
        modelBuilder.Entity(FavoritePhotoRelation.Build);
        modelBuilder.Entity(Filepath.Build);
        modelBuilder.Entity(LogEntry.Build);
        modelBuilder.Entity(Photo.Build);
        modelBuilder.Entity(PublicLink.Build);
        modelBuilder.Entity(PhotoAlbumRelation.Build);
        modelBuilder.Entity(PhotoTagRelation.Build);
        modelBuilder.Entity(Session.Build);
        modelBuilder.Entity(Tag.Build);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
