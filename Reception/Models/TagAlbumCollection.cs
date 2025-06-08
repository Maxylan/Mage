using Reception.Database.Models;

namespace Reception.Models
{
    public struct TagAlbumCollection(
        TagDTO tag,
        IEnumerable<AlbumDTO> albums
    ) {
        /// <summary>
        /// <see cref="Tag"/>
        /// </summary>
        public TagDTO Tag = tag;
        /// <summary>
        /// <see cref="IEnumerable{Album}"/> Collection
        /// </summary>
        public IEnumerable<AlbumDTO> Albums = albums;
    }

    public struct AlbumTagCollection(
        AlbumDTO album,
        IEnumerable<TagDTO> tags
    ) {
        /// <summary>
        /// <see cref="Album"/>
        /// </summary>
        public AlbumDTO Album = album;
        /// <summary>
        /// <see cref="IEnumerable{Tag}"/> Collection
        /// </summary>
        public IEnumerable<TagDTO> Tags = tags;
    }
}
