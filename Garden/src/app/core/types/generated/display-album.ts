import { IAccountDTO } from './account-dto';
import { ITagDTO } from './tag-dto';
import { ICategoryDTO } from './category-dto';
import { DisplayPhoto } from './display-photo';

export type DisplayAlbum = {
    readonly photos: DisplayPhoto[] | null,
    readonly count: number,
    readonly favorites: number,
    readonly isFavorite: boolean,
    albumId: number | null,
    thumbnailId: number | null,
    thumbnail: DisplayPhoto,
    categoryId: number | null,
    category: ICategoryDTO,
    title: string | null,
    summary: string | null,
    description: string | null,
    createdAt: Date,
    updatedAt: Date,
    requiredPrivilege: number,
    readonly tags: ITagDTO[] | null,
    updatedByUser: IAccountDTO,
    createdByUser: IAccountDTO,
}