import { IAccountDTO } from './account-dto';
import { ITagDTO } from './tag-dto';
import { IPhotoAlbumRelationDTO } from './photo-album-relation-dto';
import { IPublicLinkDTO } from './public-link-dto';
import { IFilepathDTO } from './filepath-dto';

export type DisplayPhoto = {
    photoId: number | null,
    slug: string | null,
    title: string | null,
    summary: string | null,
    description: string | null,
    uploadedAt: Date,
    updatedAt: Date,
    createdAt: Date,
    isAnalyzed: boolean,
    analyzedAt: Date | null,
    requiredPrivilege: number,
    readonly favorites: number,
    readonly isFavorite: boolean,
    source: IFilepathDTO,
    medium: IFilepathDTO,
    thumbnail: IFilepathDTO,
    readonly hasMedium: boolean,
    readonly hasThumbnail: boolean,
    readonly publicLinks: IPublicLinkDTO[] | null,
    readonly relatedAlbums: IPhotoAlbumRelationDTO[] | null,
    readonly tags: ITagDTO[] | null,
    updatedByUser: IAccountDTO,
    uploadedByUser: IAccountDTO,
}