export type Methods = 'GET'|'POST'|'PUT'|'PATCH'|'HEAD';

export interface BlobResponse {
    contentType: string|null,
    contentLength: string|null,
    file?: File
}
