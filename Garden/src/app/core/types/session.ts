import { Client } from './client';
import { Account } from './account';

export type Session = {
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