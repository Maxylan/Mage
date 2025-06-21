import { Account } from './account';
import { Album } from './album';
import { ICategoryDTO } from './category-dto';

export type Category = ICategoryDTO & {
    id: number,
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
    createdBy: number | null,
    createdAt: Date,
    updatedAt: Date,
    updatedBy: number | null,
    requiredPrivilege: number,
    albums: Album[] | null,
    createdByNavigation: Account,
    updatedByNavigation: Account,
}