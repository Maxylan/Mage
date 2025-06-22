import { PhotoTagRelation } from './photo-tag-relation';
import { AlbumTagRelation } from './album-tag-relation';
import { ITagDTO } from './tag-dto';

export type Tag = ITagDTO & {
    id: number,
    /**
     * Max Length: 127
     * Min Length: 0
     */
    name: string | null,
    description: string | null,
    requiredPrivilege: number,
    usedByAlbums: AlbumTagRelation[] | null,
    usedByPhotos: PhotoTagRelation[] | null,
    readonly items: number,
}