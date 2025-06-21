import { AccountDTO } from './account-dto';
import { TagDTO } from './tag-dto';
import { DisplayPhoto } from './display-photo';

export type DisplayAlbum = {
    readonly photos: DisplayPhoto[] | null,
    readonly count: number,
    readonly favorites: number,
    readonly isFavorite: boolean,
    readonly tags: TagDTO[] | null,
    updatedByUser: AccountDTO,
    createdByUser: AccountDTO,
}