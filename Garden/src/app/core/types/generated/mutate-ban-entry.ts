import { Client } from './client';

export type MutateBanEntry = {
    id: number | null,
    clientId: number,
    expiresAt: Date | null,
    reason: string | null,
    client: Client,
}