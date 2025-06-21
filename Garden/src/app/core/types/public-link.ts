import { Photo } from './photo';
import { Account } from './account';
import { IPublicLinkDTO } from './public-link-dto';

export type PublicLink = IPublicLinkDTO & {
    id: number,
    photoId: number,
    /**
     * Max Length: 32
     * Min Length: 0
     */
    code: string | null,
    createdBy: number | null,
    createdAt: Date,
    expiresAt: Date,
    accessLimit: number | null,
    accessed: number,
    createdByNavigation: Account,
    photo: Photo,
}