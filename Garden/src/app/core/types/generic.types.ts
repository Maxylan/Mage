import { AuthService } from "../api/auth.service";

export type Methods = 'GET'|'POST'|'PUT'|'PATCH'|'DELETE'|'HEAD';

export interface ApiCacheOptions {
    enabled: boolean,
    checkInterval: number,
    lifetime: number
};
export const defaultCachingOptions: ApiCacheOptions = {
    enabled: false,
    checkInterval: 32,
    lifetime: 16
};

export interface ApiCacheItem extends Response {
    cachedAt: number
}

export interface ApiCache {
    options: ApiCacheOptions,
    store: {
        [key: string]: ApiCacheItem
    },
    watch: () => void,
    clear: () => void,
    write: (key: string, item: ApiCacheItem|Response) => void,
    read: (key: string) => ApiCacheItem|null,
};

export interface ApiInitializationOptions {
    basePath: string,
    auth: AuthService,
    caching?: Partial<ApiCacheOptions>
};

export interface BlobResponse {
    contentType: string|null,
    contentLength: string|null,
    file?: File
}

export type Session = {
    id: number;
    userId: number;
    code: string;
    userAgent?: string|null;
    createdAt: string;
    expiresAt: string;
}

export type RefreshCredentials = {
    username: string,
    hash: string
}
