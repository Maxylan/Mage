import { Client } from './client';
import { IBanEntryDTO } from './ban-entry-dto';

export type BanEntry = IBanEntryDTO & {
    id: number,
    clientId: number,
    expiresAt: Date | null,
    reason: string | null,
    client: Client,
}