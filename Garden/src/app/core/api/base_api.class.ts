import { Injectable, signal } from '@angular/core';
import {
    ApiInitializationOptions,
    defaultCachingOptions,
    ApiCacheOptions,
    ApiCacheItem,
    ApiCache,
    Methods,
    BlobResponse
} from '../types/generic.types';
import { AuthService } from './auth.service';

export default class ApiBase {
    private initialized: boolean = false;
    private cache: ApiCache|null = null;
    private authService: AuthService|null = null;
    private apiUrl: string = '/reception';
    private basePath: string = '';

    /**
     * Is this {ApiBase} initialized?
     */
    public IsInitialized: () => boolean = () => this.initialized;
    /**
     * Initialize this {ApiBase} instance.
     */
    public Init = (opts: ApiInitializationOptions): boolean => {
        this.basePath = opts.basePath;
        this.authService = opts.auth;

        if (opts.caching && opts.caching.enabled !== false) {
            this.cache = {
                options: {
                    ...defaultCachingOptions,
                    ...opts.caching,
                    enabled: true
                },
                store: {},
                watch: function() {
                    setInterval(
                        this.clear,
                        this.options.checkInterval * 1000
                    );
                },
                clear: function() {
                    let expiry = (Date.now() / 1000) - this.options.lifetime;
                    let cacheEntries = Object.entries(this.store);
                    for(let entry of cacheEntries) {
                        let [ key, value ] = entry;
                        if (value.cachedAt < expiry) {
                            delete this.store[key];
                        }
                    }
                },
                write: function(key: string, item: ApiCacheItem|Response) {
                    if (!('cachedAt' in item)) {
                        item = {
                            ...item,
                            cachedAt: Date.now() / 1000
                        };
                    }

                    this.store[key] = (
                        item.clone() as ApiCacheItem
                    );
                },
                read: function(key: string) {
                    if (!(key in this.store)) {
                        return null;
                    }

                    let item = this.store[key];
                    let expiry = (Date.now() / 1000) - this.options.lifetime;

                    if (item.cachedAt < expiry) {
                        delete this.store[key];
                        return null;
                    }

                    return item;
                }
            };
        }

        return true;
    }

    /**
     * Get the hardcoded (..or configured?) API Url
     */
    public getApiUrl = (): string => this.apiUrl + this.basePath;

    /**
     * Get the token.
     */
    public getToken = (): string|null => {
        if (!this.IsInitialized() || !this.authService) {
            console.debug('this.authService -', this.authService);
            console.debug('this.IsInitialized() - ' + (this.IsInitialized() ? 'true' : 'false'));
            throw new Error('Call to `.getToken()` before initialization and/or whithout injecting `AuthService`');
        }

        return this.authService.getToken();
    };

    /**
     * Create a {RequestInit} options instance for fetches.
     */
    public requestOptions = (method: Methods): RequestInit => {
        if (!this.IsInitialized() || !this.authService) {
            console.debug('this.authService -', this.authService);
            console.debug('this.IsInitialized() - ' + (this.IsInitialized() ? 'true' : 'false'));
            throw new Error('Call to `.requestOptions()` before initialization and/or whithout injecting `AuthService`');
        }

        const token: string = this.authService.getToken() ?? '';
        return {
            method: method,
            headers: { // TODO! Create auth - get token from there. 
                // -- Don't worry, the fact this is not gitignored is intentional, its just a random GUID :p
                "x-mage-token": token
            }
        }
    };

    /**
     * Perform a fetch {Request} to a given API Endpoint
     */
    public request = (method: Methods, endpoint: string, opts?: RequestInit): Promise<Response> => {
        if (!this.IsInitialized() || !this.authService) {
            console.debug('this.authService -', this.authService);
            console.debug('this.IsInitialized() - ' + (this.IsInitialized() ? 'true' : 'false'));
            throw new Error('Call to `.request('+method+', ..)` before initialization and/or whithout injecting `AuthService`');
        }

        const callsign: string = `${method}:${endpoint}`;
        if (this.cache?.options?.enabled) {
            let result = this.cache.read(callsign);

            if (result !== null) {
                return Promise.resolve(result);
            }
        }

        let requestInit = this.requestOptions(method);
        if (opts) {
            requestInit = {
                ...requestInit,
                ...(opts ?? {})
            };
        }

        const futureResponse = fetch(this.getApiUrl() + endpoint, requestInit);

        if (this.cache?.options?.enabled) {
            return futureResponse.then(res => {
                if (res?.status === 401) {
                    this.authService!.fallbackToAuth(res);
                    Promise.reject('Unauthorized, falling back to auth...');
                }

                if (res && res.status > 199 && res.status < 300) {
                    this.cache!.write(callsign, res);
                }

                return res;
            });
        }

        return futureResponse;
    };

    /**
     * Perform a `HEAD` request to the given API Endpoint.
     */
    public head = (endpoint: string, opts?: RequestInit): Promise<Response> =>
        this.request('HEAD', endpoint, opts);

    /**
     * Perform a `GET` request to the given API Endpoint.
     */
    public get = (endpoint: string, opts?: RequestInit): Promise<Response> =>
        this.request('GET', endpoint, opts);

    /**
     * Perform a `PUT` request to the given API Endpoint.
     */
    public put = (endpoint: string, opts?: RequestInit): Promise<Response> =>
        this.request('PUT', endpoint, opts);

    /**
     * Perform a `PATCH` request to the given API Endpoint.
     */
    public patch = (endpoint: string, opts?: RequestInit): Promise<Response> =>
        this.request('PATCH', endpoint, opts);

    /**
     * Perform a `POST` request to the given API Endpoint.
     */
    public post = (endpoint: string, opts?: RequestInit): Promise<Response> =>
        this.request('POST', endpoint, opts);

    /**
     * Perform a `DELETE` request to the given API Endpoint.
     */
    public delete = (endpoint: string, opts?: RequestInit): Promise<Response> =>
        this.request('DELETE', endpoint, opts);
}
