/*
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Swashbuckle.AspNetCore.Annotations;
*/

// namespace Reception.Database.ModelsBackup;

/*
[Table("logs", Schema = "magedb")]
[Index("CreatedAt", Name = "idx_logs_created_at")]
*/
/*
public class LogEntry
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? UserEmail { get; set; }

    public string? UserUsername { get; set; }

    public string? UserFullName { get; set; }

    public string? RequestAddress { get; set; }

    public string? RequestUserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Severity? LogLevel { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Source? Source { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Method? Method { get; set; }

    public string Action { get; set; } = null!;

    public string? Log { get; set; }

    [JsonIgnore, SwaggerIgnore]
    public LogFormat Format => new(this);

    /// <summary>
    /// Set the <see cref="Reception.Database.Models.Method"/> of this entity using a string (<paramref name="method"/>)
    /// </summary>
    public void SetMethod(string? method) => this.Method = method?.ToUpper() switch
    {
        "HEAD" => Entities.Method.HEAD,
        "GET" => Entities.Method.GET,
        "POST" => Entities.Method.POST,
        "PUT" => Entities.Method.PUT,
        "PATCH" => Entities.Method.PATCH,
        "DELETE" => Entities.Method.DELETE,
        _ => Entities.Method.UNKNOWN
    };

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
                .HasDefaultValue(Reception.Database.Models.Severity.INFORMATION)
                .HasSentinel(null)
                /* .HasConversion(
                    x => x.ToString() ?? Reception.Database.Models.Severity.ERROR.ToString(),
                    y => Enum.Parse<Severity>(y, true)
                ) *//*;
            entity.Property(e => e.Source)
                .HasColumnName("source")
                .HasDefaultValue(Reception.Database.Models.Source.INTERNAL)
                .HasSentinel(null)
                /* .HasConversion(
                    x => x.ToString() ?? Reception.Database.Models.Source.INTERNAL.ToString(),
                    y => Enum.Parse<Source>(y, true)
                ) *//*;
            entity.Property(e => e.Method)
                .HasColumnName("method")
                .HasDefaultValue(Reception.Database.Models.Method.UNKNOWN)
                .HasSentinel(null)
                /* .HasConversion(
                    x => x.ToString() ?? Reception.Database.Models.Method.UNKNOWN.ToString(),
                    y => Enum.Parse<Method>(y, true)
                ) *//*;
            entity.Property(e => e.Log).HasColumnName("log");
            entity.Property(e => e.RequestAddress)
                .HasMaxLength(255)
                .HasColumnName("request_address");
            entity.Property(e => e.RequestUserAgent)
                .HasMaxLength(1023)
                .HasColumnName("request_user_agent");
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
*/
