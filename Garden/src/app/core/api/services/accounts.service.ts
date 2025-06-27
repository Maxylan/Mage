import { Injectable } from '@angular/core';
import ApiBase from '../../classes/base.class';
import { IAccountDTO } from '../../types/generated/account-dto';
import { MutateAccount } from '../../types/generated/mutate-account';
import { IBanEntryDTO } from '../../types/generated/ban-entry-dto';
import { DisplayClient } from '../../types/generated/display-client';
import { IClientDTO } from '../../types/generated/client-dto';
import { FilterClients } from '../../types/filter-clients';
import { FilterBanEntries } from '../../types/filter-ban-entries';
import { MutateBanEntry } from '../../types/generated/mutate-ban-entry';

@Injectable({
    providedIn: 'root'
})
export class AccountsService extends ApiBase {
    /**
     * Get a single <see cref="AccountDTO"/> (user) by its <paramref name="account_id"/> (PK, uint).
     *
     * [HttpGet("{account_id:int}")]
     */
    public async getAccount(accountId: number): Promise<IAccountDTO> {
        return await this.get('/accounts/' + accountId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getAccount] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get all <see cref="AccountDTO"/> (user) -instances, optionally filtered and/or paginated by a few query parameters.
     *
     * [HttpGet]
     */
    public async getAllAccounts(
        /* [FromQuery] */ limit?: number,
        /* [FromQuery] */ offset?: number,
        /* [FromQuery] */ lastVisit?: DateTime,
        /* [FromQuery] */ fullName?: string
    ): Promise<IAccountDTO[]> {
        return await this.get('/accounts' + this.queryParameters({
            limit,
            offset,
            lastVisit,
            fullName
        }))
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getAllAccounts] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Update a single <see cref="AccountDTO"/> (user) in the database.
     *
     * [HttpPut("{account_id:int}")]
     */
    public async updateAccount(accountId: number, mut: MutateAccount): Promise<IAccountDTO> {
        const body = JSON.stringify(mut);

        return await this.put('/accounts', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[updateAccount] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Update the avatar of a single <see cref="AccountDTO"/> (user).
     *
     * [HttpPatch("{account_id:int}/avatar/{photo_id:int}")]
     */
    public async UpdateAvatar(accountId: number, photoId: number): Promise<IAccountDTO> {
        return await this.patch(`/accounts/${accountId}/avatar/${photoId}`)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[updateAccount] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="Client"/> with Primary Key '<paramref ref="clientId"/>'
     *
     * [HttpGet("{client_id:int}")]
     */
    public async getClient(clientId: number): Promise<DisplayClient> {
        return await this.get('/clients/' + clientId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getClient] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="Client"/> with Fingerprint '<paramref ref="address"/>' & '<paramref ref="userAgent"/>'.
     *
     * [HttpGet("{address}")]
     */
    public async getClientByFingerprint(address: string, userAgent?: string): Promise<DisplayClient> {
        return await this.get('/clients/' + address + this.queryParameters({ userAgent }))
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getClientByFingerprint] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get all <see cref="Client"/>-entries matching a few optional filtering / pagination parameters.
     *
     * [HttpGet]
     */
    public async getClients(opts: FilterClients): Promise<IClientDTO[]> {
        return await this.get('/clients/' + this.queryParameters(opts))
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getClients] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="BanEntry"/> with Primary Key '<paramref ref="id"/>'
     *
     * [HttpGet("ban/{entry_id:int}")]
     */
    public async getBanEntry(entryId: number): Promise<IBanEntryDTO> {
        return await this.get('/clients/ban/' + entryId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getBanEntry] Error!', err);
                    return err;
                }
            );

    }

    /**
     * Get all <see cref="BanEntry"/>-entries matching a few optional filtering / pagination parameters.
     *
     * [HttpGet("ban")]
     */
    public async getBannedClients(filters: FilterBanEntries): Promise<IBanEntryDTO[]> {
         return await this.get('/clients' + this.queryParameters(filters))
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getBannedClients] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Update a <see cref="BanEntry"/> in the database.
     *
     * [HttpPut("ban/{entry_id:int}")]
     */
    public async updateBanEntry(entryId: number, mut: MutateBanEntry): Promise<IBanEntryDTO> {
        const body = JSON.stringify(mut); 

        return await this.put('/clients/ban/' + entryId, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[updateBanEntry] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Create a <see cref="BanEntry"/> in the database.
     * Equivalent to banning a single client (<see cref="Client"/>).
     *
     * [HttpPost("ban/{client_id:int}")]
     */
    public async banClient(clientId: number, mut?: MutateBanEntry): Promise<IBanEntryDTO> {
        const body = JSON.stringify(mut); 

        return await this.post('/clients/ban/' + clientId, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[banClient] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Delete / Remove a <see cref="BanEntry"/> from the database.
     * Equivalent to unbanning a single client (<see cref="Client"/>).
     *
     * [HttpDelete("ban/{client_id:int}/account/{account_id:int}")]
     */
    public async unbanClient(clientId: number, accountId: number): Promise<void> {
        return await this.delete(`/clients/ban/${clientId}/account/${accountId}`)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[unbanClient] Error!', err);
                    return err;
                }
            );
    }
}
