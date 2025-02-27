import { Injectable } from '@angular/core';
import { Dimension, IPhotoQueryParameters, Photo, PhotoCollection } from '../types/photos.types';
import { BlobResponse, Methods } from '../types/generic.types';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root' /* ,
    imports: [AuthService] */
})
export class PhotosService {
    private authService!: AuthService;
    private apiUrl: string = '/reception';
    private generateRequestInit = (method: Methods): RequestInit => {
        const token = this.authService.getToken();
        if (!token) {
            throw new Error('Failed to get token from `AuthService`!');
        }

        return {
            method: method,
            headers: { // TODO! Create auth - get token from there. 
                // -- Don't worry, the fact this is not gitignored is intentional, its just a random GUID :p
                "x-mage-token": token
            }
        }
    };

    constructor(auth: AuthService) {
        this.authService = auth;
    }

    /**
     * Get a single `Photo` (source) by its `photoId` (PK, uint)
     */
    public getSourcePhoto(photoId: number): Promise<Photo> {
        return fetch(this.apiUrl + '/photos/source/' + photoId, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            )
            .catch(
                err => {
                    console.error('[getSourcePhoto] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single `Photo` (source) by its `slug` (unique, string)
     */
    public getSourcePhotoBySlug(slug: string): Promise<Photo> {
        return fetch(this.apiUrl + '/photos/source/slug/' + slug, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            ).catch(
                err => {
                    console.error('[getSourcePhotoBySlug] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single `Photo` (medium) by its `photoId` (PK, uint)
     */
    public getMediumPhoto(photoId: number): Promise<Photo> {
        return fetch(this.apiUrl + '/photos/medium/' + photoId, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            ).catch(
                err => {
                    console.error('[getMediumPhoto] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single `Photo` (medium) by its `slug` (unique, string)
     */
    public getMediumPhotoBySlug(slug: string): Promise<Photo> {
        return fetch(this.apiUrl + '/photos/medium/slug/' + slug, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            ).catch(
                err => {
                    console.error('[getMediumPhotoBySlug] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single `Photo` (thumbnail) by its `photoId` (PK, uint)
     */
    public getThumbnailPhoto(photoId: number): Promise<Photo> {
        return fetch(this.apiUrl + '/photos/thumbnail/' + photoId, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            ).catch(
                err => {
                    console.error('[getThumbnailPhoto] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single `Photo` (thumbnail) by its `slug` (unique, string)
     */
    public getThumbnailPhotoBySlug(slug: string): Promise<Photo> {
        return fetch(this.apiUrl + '/photos/thumbnail/slug/' + slug, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            ).catch(
                err => {
                    console.error('[getThumbnailPhotoBySlug] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a `PhotoCollection` (all sizes) of the Photo with `photoId` (PK, uint)
     */
    public getPhoto(photoId: number): Promise<PhotoCollection> {
        return fetch(this.apiUrl + '/photos/' + photoId, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            ).catch(
                err => {
                    console.error('[getPhoto] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a `PhotoCollection` (all sizes) of the Photo with `slug` (unique, string)
     */
    public getPhotoBySlug(slug: string): Promise<PhotoCollection> {
        return fetch(this.apiUrl + '/photos/slug/' + slug, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            ).catch(
                err => {
                    console.error('[getPhotoBySlug] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Query for all `PhotoCollection`'s that match all criterias passed as URL/Query Parameters.
     */
    public getPhotos(params: IPhotoQueryParameters): Promise<PhotoCollection[]> {
        let parameters = Array.from(Object.entries(params))
            .filter(kvp => !(
                kvp[0] === null ||
                kvp[1] === null ||
                kvp[0] === undefined ||
                kvp[1] === undefined
            ))
            .map(kvp => `${kvp[0]}=${kvp[1].toString().trim()}`);

        console.debug('params', params, parameters);

        let queryParameters = parameters.length > 0
            ? '?' + parameters.join('&')
            : '';

        console.debug('queryParameters', queryParameters);

        return fetch(this.apiUrl + '/photos' + queryParameters, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            ).catch(
                err => {
                    console.error('[getThumbnailPhotoBySlug] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Query for all Source `Photo`'s that match all criterias passed as URL/Query Parameters.
     */
    public getSourcePhotos(params: IPhotoQueryParameters): Promise<Photo[]> {
        return this._queryForSingleDimensionPhotos(params, 'source');
    }
    /**
     * Query for all Medium `Photo`'s that match all criterias passed as URL/Query Parameters.
     */
    public getMediumPhotos(params: IPhotoQueryParameters): Promise<Photo[]> {
        return this._queryForSingleDimensionPhotos(params, 'medium');
    }
    /**
     * Query for all Thumbnail `Photo`'s that match all criterias passed as URL/Query Parameters.
     */
    public getThumbnailPhotos(params: IPhotoQueryParameters): Promise<Photo[]> {
        return this._queryForSingleDimensionPhotos(params, 'thumbnail');
    }

    private _queryForSingleDimensionPhotos(params: IPhotoQueryParameters, dimension: 'source'|'medium'|'thumbnail'): Promise<Photo[]> {
        if (params.dimension !== undefined) {
            delete params.dimension;
        }

        let parameters = Array.from(Object.entries(params))
            .filter(kvp => !(
                kvp[0] === null ||
                kvp[1] === null ||
                kvp[0] === undefined ||
                kvp[1] === undefined
            ))
            .map(kvp => `${kvp[0]}=${kvp[1].toString().trim()}`);

        let queryParameters = parameters.length > 0
            ? '?' + parameters.join('&')
            : '';

        return fetch(this.apiUrl + '/photos/' + dimension + queryParameters, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    return res.json();
                }
            ).catch(
                err => {
                    console.error('[_queryForSingleDimensionPhotos] Error!', err);
                    return err;
                }
            );
    }


    /**
     * Get the Image `File` (blob) of the Photo with the given `photoId` (PK, uint)
     */
    public getPhotoBlob(photoId: number, dimension: Dimension|'source'|'medium'|'thumbnail' = 'source'): Promise<BlobResponse> {
        let url = this.apiUrl + '/photos';
        switch(dimension) {
            case 'source':
            case Dimension.SOURCE:
                url += '/source';
                break;
            case 'medium':
            case Dimension.MEDIUM:
                url += '/medium';
                break;
            case 'thumbnail':
            case Dimension.THUMBNAIL:
                url += '/thumbnail';
                break;
        }

        const requestInit: RequestInit = {
            ...this.generateRequestInit('GET'),
            
        };

        var blobResponse: BlobResponse = {
            contentType: null,
            contentLength: null
        };

        return fetch(`${url}/${photoId}/blob`, this.generateRequestInit('GET'))
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    blobResponse.contentType = 
                        res.headers.get('Content-Type') || res.headers.get('content-type');
                    blobResponse.contentLength =
                        res.headers.get('Content-Length') || res.headers.get('content-length');
                    
                    return res.blob();
                }
            )
            .then(blob => {
                blobResponse.file = new File([blob], 'thumbnail_'+photoId);
                return blobResponse;
            })
            .catch(
                err => {
                    console.error('[getPhotoBlob] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the Image `File` (blob) of the Photo with the given `slug` (unique, string)
     */
    public getPhotoBlobBySlug(slug: string, dimension: Dimension|'source'|'medium'|'thumbnail' = 'source'): Promise<BlobResponse> {
        let url = this.apiUrl + '/photos';
        switch(dimension) {
            case 'source':
            case Dimension.SOURCE:
                url += '/source';
                break;
            case 'medium':
            case Dimension.MEDIUM:
                url += '/medium';
                break;
            case 'thumbnail':
            case Dimension.THUMBNAIL:
                url += '/thumbnail';
                break;
        }

        const requestInit: RequestInit = {
            ...this.generateRequestInit('GET'),
            
        };

        var blobResponse: BlobResponse = {
            contentType: null,
            contentLength: null
        };

        return fetch(url + '/slug/' + slug + '/blob', requestInit)
            .then(
                res => {
                    if (res?.status === 401) {
                        this.authService.fallbackToAuth(res);
                    }

                    blobResponse.contentType = 
                        res.headers.get('Content-Type') || res.headers.get('content-type');
                    blobResponse.contentLength =
                        res.headers.get('Content-Length') || res.headers.get('content-length');
                    
                    return res.blob();
                }
            )
            .then(blob => {
                blobResponse.file = new File([blob], 'thumbnail_'+slug);
                return blobResponse;
            })
            .catch(
                err => {
                    console.error('[getPhotoBlobBySlug] Error!', err);
                    return err;
                }
            );
    }

}
