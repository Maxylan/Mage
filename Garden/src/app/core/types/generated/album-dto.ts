import { Photo } from './photo';
import { PhotoAlbumRelation } from './photo-album-relation';
import { FavoriteAlbumRelation } from './favorite-album-relation';
import { Account } from './account';
import { Category } from './category';
import { AlbumTagRelation } from './album-tag-relation';

export interface IAlbumDTO {
    id?: number | null,
    categoryId?: number | null,
    thumbnailId?: number | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    title?: string | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    summary?: string | null,
    description?: string | null,
    createdBy?: number | null,
    createdAt: Date,
    updatedBy?: number | null,
    updatedAt: Date,
    requiredPrivilege: number,
    tags?: AlbumTagRelation[] | null,
    category: Category,
    createdByNavigation: Account,
    favoritedBy?: FavoriteAlbumRelation[] | null,
    photos?: PhotoAlbumRelation[] | null,
    thumbnail: Photo,
    updatedByNavigation: Account,
}