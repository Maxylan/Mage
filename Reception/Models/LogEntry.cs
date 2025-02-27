using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Reception.Models;

public class LogEntry
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserUsername { get; set; }
    public string? UserFullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DataTypes.Severity? LogLevel { get; set; }
    public DataTypes.Source? Source { get; set; }
    public DataTypes.Method? Method { get; set; }
    public string? Action { get; set; }
    public string? Log { get; set; }

    public static Action<EntityTypeBuilder<LogEntry>> Build => (
        entity =>
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
            entity.Property(e => e.LogLevel)
                .HasColumnName("log_level")
                .HasDefaultValue(DataTypes.Severity.INFORMATION)
                .HasSentinel(null)
                .HasConversion(
                    x => x.ToString() ?? DataTypes.Severity.ERROR.ToString(),
                    y => Enum.Parse<DataTypes.Severity>(y, true)
                );
            entity.Property(e => e.Source)
                .HasColumnName("source")
                .HasDefaultValue(DataTypes.Source.INTERNAL)
                .HasSentinel(null)
                .HasConversion(
                    x => x.ToString() ?? DataTypes.Source.INTERNAL.ToString(),
                    y => Enum.Parse<DataTypes.Source>(y, true)
                );
            entity.Property(e => e.Method)
                .HasColumnName("method")
                .HasDefaultValue(DataTypes.Method.UNKNOWN)
                .HasSentinel(null)
                .HasConversion(
                    x => x.ToString() ?? DataTypes.Method.UNKNOWN.ToString(),
                    y => Enum.Parse<DataTypes.Method>(y, true)
                );
            entity.Property(e => e.Log).HasColumnName("log");
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
        }
    );
}
