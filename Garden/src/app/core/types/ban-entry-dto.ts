import { Client } from './client';

export interface IBanEntryDTO {
    id?: number | null,
    clientId: number,
    expiresAt?: Date | null,
    reason?: string | null,
    client: Client,
}