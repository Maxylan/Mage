import { inject, signal } from '@angular/core';
import { AuthService } from '../services/auth.service';

export default abstract class ApiBase {
    public static readonly API_URL: string = '/reception';

    public readonly isLoading = signal<boolean>(false);

    protected readonly authService = inject(AuthService);

    /**
     * Get the token from `{AuthService}`.
     */
    protected get token(): string|null {
        return this.authService.getToken();
    };

    /**
     * Create a {RequestInit} options instance for fetches.
     */
    protected requestOptions(
        method: 'HEAD'|'GET'|'PATCH'|'PUT'|'POST'|'DELETE',
        contentType: string = 'application/json'
    ): RequestInit {
        const token = this.token || '';
        if (!token) {
            console.warn('requestOptions failed to acquire user session token!');
        }

        return {
            method: method,
            headers: {  
                'Accepts': 'application/json',
                'Content-Type': contentType,
                [AuthService.HEADER]: token
            }
        }
    };

    /**
     * Perform a fetch {Request} to a given API Endpoint
     */
    protected async sendRequest(
        method: 'HEAD'|'GET'|'PATCH'|'PUT'|'POST'|'DELETE',
        endpoint: string,
        opts?: RequestInit
    ): Promise<Response> {
        /* const callsign: string = `${method}:${endpoint}`;

        if (this.cache?.options?.enabled) {
            let result = this.cache.read(callsign);

            if (result !== null) {
                return Promise.resolve(result);
            }
        } */

        let requestInit = this.requestOptions(method);
        if (opts) {
            requestInit = {
                ...requestInit,
                ...(opts ?? {})
            };
        }

        if (this.isLoading() === false) {
            this.isLoading.set(true);
        }

        return await fetch(ApiBase.API_URL + endpoint, requestInit)
            .then(res => {
                if (res?.status === 401) {
                    console.warn('401 Unauthorized, probably session expiry, falling back on auth...');
                    this.authService!.fallbackToAuth(res);
                }

                return res;
            })
            .finally(() => {
                if (this.isLoading() === true) {
                    this.isLoading.set(false);
                }
            });

        /* if (this.cache?.options?.enabled) {
            return requestFuture.then(res => {
                if (res?.status === 401) {
                    this.authService!.fallbackToAuth(res);
                    Promise.reject('Unauthorized, falling back to auth...');
                }

                if (res && res.status > 199 && res.status < 300) {
                    this.cache!.write(callsign, res.clone());
                }

                return res;
            });
        }

        return requestFuture; */
    };

    /**
     * Perform a `HEAD` request to the given API Endpoint.
     */
    protected head(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('HEAD', endpoint, opts);
    }

    /**
     * Perform a `GET` request to the given API Endpoint.
     */
    public get(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('GET', endpoint, opts);
    }

    /**
     * Perform a `PUT` request to the given API Endpoint.
     */
    public put(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('PUT', endpoint, opts);
    }

    /**
     * Perform a `PATCH` request to the given API Endpoint.
     */
    public patch(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('PATCH', endpoint, opts);
    }

    /**
     * Perform a `POST` request to the given API Endpoint.
     */
    public post(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('POST', endpoint, opts);
    }

    /**
     * Perform a `DELETE` request to the given API Endpoint.
     */
    public delete(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('DELETE', endpoint, opts);
    }
}
