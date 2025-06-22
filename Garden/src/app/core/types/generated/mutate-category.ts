import { Account } from './account';

export type MutateCategory = {
    id: number | null,
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
    albums: number | null,
    createdByNavigation: Account,
    updatedByNavigation: Account,
}