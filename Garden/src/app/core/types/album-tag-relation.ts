import { Tag } from './tag';
import { Album } from './album';

export type AlbumTagRelation = {
    albumId: number,
    tagId: number,
    added: Date,
    album: Album,
    tag: Tag,
}