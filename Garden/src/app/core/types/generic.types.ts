export type Methods = 'GET'|'POST'|'PUT'|'PATCH'|'HEAD';

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
