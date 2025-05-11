using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reception.Database.Models;

/// <summary>
/// The <see cref="Tag"/> db-entity.
/// </summary>
[Table("tags", Schema = "magedb")]
[Index("Name", Name = "idx_tags_name")]
[Index("Name", Name = "tags_name_key", IsUnique = true)]
public partial class Tag : IDatabaseEntity<Tag>
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(127)]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("required_privilege")]
    public byte RequiredPrivilege { get; set; }
}

/// <summary>
/// Inverse properties & static methods of the <see cref="Tag"/> db-entity.
/// </summary>
public partial class Tag
{
    [InverseProperty("Tag")]
    public virtual ICollection<AlbumTagRelation> UsedByAlbums { get; set; } = new List<AlbumTagRelation>();

    [InverseProperty("Tag")]
    public virtual ICollection<PhotoTagRelation> UsedByPhotos { get; set; } = new List<PhotoTagRelation>();

    /// <summary>
    /// Construct / Initialize an <see cref="EntityTypeBuilder{TEntity}"/> of type <see cref="Tag"/>
    /// </summary>
    public static Action<EntityTypeBuilder<Tag>> Build => (
        entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.Property(e => e.RequiredPrivilege).HasDefaultValue((short)0);
        }
    );
}
