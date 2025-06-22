import { Client } from './client';
import { Session } from './session';
import { PublicLink } from './public-link';
import { FavoritePhotoRelation } from './favorite-photo-relation';
import { FavoriteAlbumRelation } from './favorite-album-relation';
import { Category } from './category';
import { Photo } from './photo';
import { Album } from './album';
import { IAccountDTO } from './account-dto';

export type Account = IAccountDTO & {
    id: number,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    email: string | null,
    /**
     * Max Length: 127
     * Min Length: 0
     */
    username: string | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    fullName: string | null,
    createdAt: Date,
    lastLogin: Date,
    privilege: number,
    avatarId: number | null,
    albumsCreated: Album[] | null,
    albumsUpdated: Album[] | null,
    avatar: Photo,
    createdCategories: Category[] | null,
    updatedCategories: Category[] | null,
    favoriteAlbums: FavoriteAlbumRelation[] | null,
    favoritePhotos: FavoritePhotoRelation[] | null,
    linksCreated: PublicLink[] | null,
    photosUpdated: Photo[] | null,
    photosUploaded: Photo[] | null,
    sessions: Session[] | null,
    clients: Client[] | null,
}