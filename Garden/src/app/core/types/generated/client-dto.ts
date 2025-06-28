import { IAccountDTO } from './account-dto';
import { ISessionDTO } from './session-dto';
import { BanEntry } from './ban-entry';

export interface IClientDTO {
    id?: number | null,
    trusted: boolean,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    address?: string | null,
    /**
     * Max Length: 1023
     * Min Length: 0
     */
    userAgent?: string | null,
    logins: number,
    failedLogins: number,
    createdAt: Date,
    lastVisit: Date,
    banEntries?: BanEntry[] | null,
    sessions?: ISessionDTO[] | null,
    accounts?: IAccountDTO[] | null,
}