using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="LogEntry"/> db-entity.
/// </summary>
[Table("logs", Schema = "magedb")]
[Index("CreatedAt", Name = "idx_logs_created_at")]
public partial class LogEntry : IDatabaseEntity<LogEntry>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("user_email")]
    [StringLength(255)]
    public string? UserEmail { get; set; }

    [Column("user_username")]
    [StringLength(127)]
    public string? UserUsername { get; set; }

    [Column("user_full_name")]
    [StringLength(255)]
    public string? UserFullName { get; set; }

    [Column("request_address")]
    [StringLength(255)]
    public string? RequestAddress { get; set; }

    [Column("request_user_agent")]
    [StringLength(1023)]
    public string? RequestUserAgent { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("log_level")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Severity? LogLevel { get; set; }

    [Column("source")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Source? Source { get; set; }

    [Column("method")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Method? Method { get; set; }

    [Column("action")]
    [StringLength(255)]
    public string Action { get; set; } = null!;

    [Column("log")]
    public string? Message { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="LogEntry"/> db-entity.
/// </summary>
public partial class LogEntry
{
    [JsonIgnore, SwaggerIgnore]
    public LogFormat Format => new(this);

    /// <summary>
    /// Set the <see cref="Reception.Database.Models.Method"/> of this entity using a string (<paramref name="method"/>)
    /// </summary>
    public void SetMethod(string? method) => this.Method = method?.ToUpper() switch
    {
        "HEAD" => Reception.Database.Method.HEAD,
        "GET" => Reception.Database.Method.GET,
        "POST" => Reception.Database.Method.POST,
        "PUT" => Reception.Database.Method.PUT,
        "PATCH" => Reception.Database.Method.PATCH,
        "DELETE" => Reception.Database.Method.DELETE,
        _ => Reception.Database.Method.UNKNOWN
    };

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="LogEntry"/>
    /// </summary>
    public static Action<EntityTypeBuilder<LogEntry>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("logs_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        }
    );
}
