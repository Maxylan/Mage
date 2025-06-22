import { IAccountDTO } from './account-dto';
import { ITagDTO } from './tag-dto';
import { DisplayPhoto } from './display-photo';

export type DisplayAlbum = {
    readonly photos: DisplayPhoto[] | null,
    readonly count: number,
    readonly favorites: number,
    readonly isFavorite: boolean,
    readonly tags: ITagDTO[] | null,
    updatedByUser: IAccountDTO,
    createdByUser: IAccountDTO,
}