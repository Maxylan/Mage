import { Photo } from './photo';
import { FavoriteAlbumRelation } from './favorite-album-relation';
import { Account } from './account';
import { Category } from './category';
import { MutateTag } from './mutate-tag';

export type MutateAlbum = {
    id: number | null,
    categoryId: number | null,
    thumbnailId: number | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    title: string | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    summary: string | null,
    description: string | null,
    createdAt: Date,
    updatedAt: Date,
    requiredPrivilege: number,
    tags: MutateTag[] | null,
    category: Category,
    createdByNavigation: Account,
    favoritedBy: FavoriteAlbumRelation[] | null,
    photos: number | null,
    thumbnail: Photo,
    updatedByNavigation: Account,
}