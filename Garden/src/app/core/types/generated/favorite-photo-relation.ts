import { Photo } from './photo';
import { Account } from './account';

export type FavoritePhotoRelation = {
    accountId: number,
    photoId: number,
    added: Date,
    account: Account,
    photo: Photo,
}