import { Client } from './client';
import { Account } from './account';
import { ISessionDTO } from './session-dto';

export type Session = ISessionDTO & {
    id: number,
    accountId: number,
    clientId: number,
    /**
     * Max Length: 36
     * Min Length: 0
     */
    code: string | null,
    createdAt: Date,
    expiresAt: Date,
    account: Account,
    client: Client,
}