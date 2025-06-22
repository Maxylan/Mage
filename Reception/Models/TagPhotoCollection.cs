using Reception.Database.Models;

namespace Reception.Models
{
    public struct TagPhotoCollection()
    {
        /// <summary>
        /// <see cref="Tag"/>
        /// </summary>
        public TagDTO Tag { get; set; } = null!;
        /// <summary>
        /// <see cref="IEnumerable{Photo}"/> Collection
        /// </summary>
        public IEnumerable<PhotoDTO> Photos { get; set; } = null!;
    }

    public struct PhotoTagCollection()
    {
        /// <summary>
        /// <see cref="Photo"/>
        /// </summary>
        public PhotoDTO Photo { get; set; } = null!;
        /// <summary>
        /// <see cref="IEnumerable{Tag}"/> Collection
        /// </summary>
        public IEnumerable<TagDTO> Tags { get; set; } = null!;
    }
}
