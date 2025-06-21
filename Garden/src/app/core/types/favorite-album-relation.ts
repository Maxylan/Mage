import { Album } from './album';
import { Account } from './account';

export type FavoriteAlbumRelation = {
    accountId: number,
    albumId: number,
    added: Date,
    account: Account,
    album: Album,
}