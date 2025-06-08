using Reception.Database.Models;

namespace Reception.Models
{
    public struct TagPhotoCollection(
        TagDTO tag,
        IEnumerable<PhotoDTO> photos
    ) {
        /// <summary>
        /// <see cref="Tag"/>
        /// </summary>
        public TagDTO Tag = tag;
        /// <summary>
        /// <see cref="IEnumerable{Photo}"/> Collection
        /// </summary>
        public IEnumerable<PhotoDTO> Photos = photos;
    }

    public struct PhotoTagCollection(
        PhotoDTO photo,
        IEnumerable<TagDTO> tags
    ) {
        /// <summary>
        /// <see cref="Photo"/>
        /// </summary>
        public PhotoDTO Photo = photo;
        /// <summary>
        /// <see cref="IEnumerable{Tag}"/> Collection
        /// </summary>
        public IEnumerable<TagDTO> Tags = tags;
    }
}
