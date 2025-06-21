import { AccountDTO } from './account-dto';
import { TagDTO } from './tag-dto';
import { PhotoAlbumRelationDTO } from './photo-album-relation-dto';
import { PublicLinkDTO } from './public-link-dto';
import { FilepathDTO } from './filepath-dto';

export type DisplayPhoto = {
    readonly favorites: number,
    readonly isFavorite: boolean,
    source: FilepathDTO,
    medium: FilepathDTO,
    thumbnail: FilepathDTO,
    readonly hasMedium: boolean,
    readonly hasThumbnail: boolean,
    readonly publicLinks: PublicLinkDTO[] | null,
    readonly relatedAlbums: PhotoAlbumRelationDTO[] | null,
    readonly tags: TagDTO[] | null,
    updatedByUser: AccountDTO,
    uploadedByUser: AccountDTO,
}