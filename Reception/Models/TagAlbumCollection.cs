using Reception.Database.Models;

namespace Reception.Models
{
    public struct TagAlbumCollection()
    {
        /// <summary>
        /// <see cref="Tag"/>
        /// </summary>
        public TagDTO Tag { get; set; } = null!;
        /// <summary>
        /// <see cref="IEnumerable{Album}"/> Collection
        /// </summary>
        public IEnumerable<AlbumDTO> Albums { get; set; } = null!;
    }

    public struct AlbumTagCollection()
    {
        /// <summary>
        /// <see cref="Album"/>
        /// </summary>
        public AlbumDTO Album { get; set; } = null!;
        /// <summary>
        /// <see cref="IEnumerable{Tag}"/> Collection
        /// </summary>
        public IEnumerable<TagDTO> Tags { get; set; } = null!;
    }
}
