import { Account } from './account';
import { Session } from './session';
import { BanEntry } from './ban-entry';
import { IClientDTO } from './client-dto';

export type Client = IClientDTO & {
    id: number,
    trusted: boolean,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    address: string | null,
    /**
     * Max Length: 1023
     * Min Length: 0
     */
    userAgent: string | null,
    logins: number,
    failedLogins: number,
    createdAt: Date,
    lastVisit: Date,
    banEntries: BanEntry[] | null,
    sessions: Session[] | null,
    accounts: Account[] | null,
}