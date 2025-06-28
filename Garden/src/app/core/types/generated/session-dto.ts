import { IClientDTO } from './client-dto';
import { IAccountDTO } from './account-dto';

export interface ISessionDTO {
    id?: number | null,
    accountId: number,
    clientId: number,
    /**
     * Max Length: 36
     * Min Length: 0
     */
    code?: string | null,
    createdAt: Date,
    expiresAt: Date,
    account: IAccountDTO,
    client: IClientDTO,
}