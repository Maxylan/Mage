import { PhotoTagRelation } from './photo-tag-relation';
import { PhotoAlbumRelation } from './photo-album-relation';
import { PublicLink } from './public-link';
import { Filepath } from './filepath';
import { FavoritePhotoRelation } from './favorite-photo-relation';
import { Album } from './album';
import { Account } from './account';
import { IPhotoDTO } from './photo-dto';

export type Photo = IPhotoDTO & {
    id: number,
    /**
     * Max Length: 127
     * Min Length: 0
     */
    slug: string | null,
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
    uploadedBy: number | null,
    uploadedAt: Date,
    updatedBy: number | null,
    updatedAt: Date,
    createdAt: Date,
    isAnalyzed: boolean,
    analyzedAt: Date | null,
    requiredPrivilege: number,
    usedAsAvatar: Account[] | null,
    usedAsThumbnail: Album[] | null,
    favoritedBy: FavoritePhotoRelation[] | null,
    filepaths: Filepath[] | null,
    publicLinks: PublicLink[] | null,
    albums: PhotoAlbumRelation[] | null,
    tags: PhotoTagRelation[] | null,
}