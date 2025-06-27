import { inject, signal } from '@angular/core';
import { TokenService } from '../api/services/token.service';

export default abstract class ApiBase {
    public static readonly API_URL: string = '/reception';

    public readonly isLoading = signal<boolean>(false);

    protected readonly tokenService = inject(TokenService);

    /**
     * Get the token from `{TokenService}`.
     */
    protected get token(): string|null {
        return this.tokenService.getToken();
    };

    /**
     * Create a {RequestInit} options instance for fetches.
     */
    protected queryParameters(
        parameters: object
    ): string {
        if (!parameters) {
            return '';
        }

        let queryParams = [];
        for (const [key, value] of Object.entries(parameters)) {
            queryParams.push(`${key}=${value}`);
        }

        if (queryParams.length === 0) {
            return '';
        }

        return '?' + queryParams.join('&');
    };

    /**
     * Create a {RequestInit} options instance for fetches.
     */
    protected requestOptions(
        method: 'HEAD'|'GET'|'PATCH'|'PUT'|'POST'|'DELETE',
        opts?: RequestInit
    ): RequestInit {
        const token = this.token || '';
        if (!token) {
            console.warn('requestOptions failed to acquire user session token!');
        }

        const defaultRequestOptions = {
            method: method,
            headers: {  
                'Accepts': 'application/json',
                'Content-Type': 'application/json',
                [TokenService.HEADER]: token
            }
        };

        if (opts) {
            return {
                ...defaultRequestOptions,
                ...opts
            }
        }

        return defaultRequestOptions; 
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

        let requestInit = this.requestOptions(method, opts);

        if (this.isLoading() === false) {
            this.isLoading.set(true);
        }

        return await fetch(ApiBase.API_URL + endpoint, requestInit)
            .then(res => {
                if (res?.status === 401) {
                    console.warn('401 Unauthorized, probably session expiry, falling back on auth...');
                    this.tokenService!.fallbackToAuth(res);
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
                    this.tokenService!.fallbackToAuth(res);
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
    protected get(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('GET', endpoint, opts);
    }

    /**
     * Perform a `PUT` request to the given API Endpoint.
     */
    protected put(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('PUT', endpoint, opts);
    }

    /**
     * Perform a `PATCH` request to the given API Endpoint.
     */
    protected patch(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('PATCH', endpoint, opts);
    }

    /**
     * Perform a `POST` request to the given API Endpoint.
     */
    protected post(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('POST', endpoint, opts);
    }

    /**
     * Perform a `DELETE` request to the given API Endpoint.
     */
    protected delete(endpoint: string, opts?: RequestInit): Promise<Response> {
        return this.sendRequest('DELETE', endpoint, opts);
    }
}
