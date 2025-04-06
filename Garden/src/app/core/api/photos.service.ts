import { Injectable } from '@angular/core';
import { Dimension, IPhotoQueryParameters, Photo, PhotoCollection } from '../types/photos.types';
import { BlobResponse, Methods } from '../types/generic.types';
import { AuthService } from './auth.service';
import ApiBase from './base_api.class';
import { SearchQueryParameters } from '../../shared/blocks/search-bar/search-bar.component';

@Injectable({
    providedIn: 'root' /* ,
    imports: [AuthService] */
})
export class PhotosService extends ApiBase {
    constructor(auth: AuthService) {
        super();
        this.Init({
            auth,
            basePath: '/photos',
            caching: {
                enabled: true,
                checkInterval: 120,
                lifetime: 32
            }
        });
    }

    ngInit(auth: AuthService) {
        this.Init({
            auth,
            basePath: '/photos',
            caching: {
                enabled: true,
                checkInterval: 120,
                lifetime: 32
            }
        });
    }

    /**
     * Get a single `Photo` (source) by its `photoId` (PK, uint)
     */
    public async getSourcePhoto(photoId: number): Promise<Photo> {
        return await this.get('/source/' + photoId)
            .then(res => res.json())
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
    public async getSourcePhotoBySlug(slug: string): Promise<Photo> {
        return await this.get('/source/slug/' + slug)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getSourcePhotoBySlug] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single `Photo` (medium) by its `photoId` (PK, uint)
     */
    public async getMediumPhoto(photoId: number): Promise<Photo> {
        return await this.get('/medium/' + photoId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getMediumPhoto] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single `Photo` (medium) by its `slug` (unique, string)
     */
    public async getMediumPhotoBySlug(slug: string): Promise<Photo> {
        return await this.get('/medium/slug/' + slug)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getMediumPhotoBySlug] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single `Photo` (thumbnail) by its `photoId` (PK, uint)
     */
    public async getThumbnailPhoto(photoId: number): Promise<Photo> {
        return await this.get('/thumbnail/' + photoId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getThumbnailPhoto] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single `Photo` (thumbnail) by its `slug` (unique, string)
     */
    public async getThumbnailPhotoBySlug(slug: string): Promise<Photo> {
        return await this.get('/thumbnail/slug/' + slug)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getThumbnailPhotoBySlug] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a `PhotoCollection` (all sizes) of the Photo with `photoId` (PK, uint)
     */
    public async getPhoto(photoId: number): Promise<PhotoCollection> {
        return await this.get('/' + photoId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getPhoto] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a `PhotoCollection` (all sizes) of the Photo with `slug` (unique, string)
     */
    public async getPhotoBySlug(slug: string): Promise<PhotoCollection> {
        return await this.get('/slug/' + slug)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getPhotoBySlug] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Parse incomming `SearchQueryParameters` URL/Query Parameters into a supported `IPhotoQueryParameters` collection.
     */
    public parseSearchQueryParameters(params: SearchQueryParameters, { offset, limit }: {
        offset: IPhotoQueryParameters['offset'],
        limit: IPhotoQueryParameters['limit']
    }): IPhotoQueryParameters {
        let supportedParameters: IPhotoQueryParameters = {
            search: params.search,
            summary: params.search,
            title: params.search,
            slug: params.search,
            offset,
            limit
        }

        if ('t' in params && Array.isArray(params['t'])) {
            supportedParameters.tags = params['t'];
        }

        return supportedParameters;
    }

    /**
     * Query for all `PhotoCollection`'s that match all criterias passed as URL/Query Parameters.
     */
    public async getPhotos(params: string|IPhotoQueryParameters): Promise<PhotoCollection[]> {
        let queryParameters: string = '';
        if (typeof params === 'string' && params.startsWith('?')) {
            queryParameters = params;
        }
        else {
            let parameters = Array.from(Object.entries(params))
                .filter(kvp => !(
                    kvp[0] === null
                    || kvp[1] === null
                    || kvp[0] === undefined
                    || kvp[1] === undefined
                ))
                // 2025-04-05 - TODO: Ugly hack, only use 'search', other params are "filters". Remove & Fix this.
                .filter(kvp => ['search', 'offset', 'limit'].includes(kvp[0]))
                .map(kvp => `${kvp[0]}=${kvp[1].toString().trim()}`);

            queryParameters = parameters.length > 0
                ? '?' + parameters.join('&')
                : '';
        }

        return await this.get(queryParameters)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getPhotos] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Query for all Source `Photo`'s that match all criterias passed as URL/Query Parameters.
     */
    public async getSourcePhotos(params: IPhotoQueryParameters): Promise<Photo[]> {
        return await this._queryForSingleDimensionPhotos(params, 'source');
    }
    /**
     * Query for all Medium `Photo`'s that match all criterias passed as URL/Query Parameters.
     */
    public async getMediumPhotos(params: IPhotoQueryParameters): Promise<Photo[]> {
        return await this._queryForSingleDimensionPhotos(params, 'medium');
    }
    /**
     * Query for all Thumbnail `Photo`'s that match all criterias passed as URL/Query Parameters.
     */
    public async getThumbnailPhotos(params: IPhotoQueryParameters): Promise<Photo[]> {
        return await this._queryForSingleDimensionPhotos(params, 'thumbnail');
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

        return this.get('/' + dimension + queryParameters)
            .then(res => res.json())
            .catch(
                err => {
                    console.error(`[_queryForSingleDimensionPhotos] (${dimension}) Error!`, err);
                    return err;
                }
            );
    }


    /**
     * Get the Image `File` (blob) of the Photo with the given `photoId` (PK, uint)
     */
    public async getPhotoBlob(photoId: number, dimension: Dimension|'source'|'medium'|'thumbnail' = 'source'): Promise<BlobResponse> {
        let dimensionParameter = '';
        switch(dimension) {
            case 'source':
            case Dimension.SOURCE:
                dimensionParameter += 'source';
                break;
            case 'medium':
            case Dimension.MEDIUM:
                dimensionParameter += 'medium';
                break;
            case 'thumbnail':
            case Dimension.THUMBNAIL:
                dimensionParameter += 'thumbnail';
                break;
        }

        var blobResponse: BlobResponse = {
            contentType: null,
            contentLength: null
        };

        return await this.get(`/${dimensionParameter}/${photoId}/blob`)
            .then(
                res => {
                    blobResponse.contentType = 
                        res.headers.get('Content-Type') || res.headers.get('content-type');
                    blobResponse.contentLength =
                        res.headers.get('Content-Length') || res.headers.get('content-length');
                    
                    return res.blob();
                }
            )
            .then(blob => {
                blobResponse.file = new File([blob], `${dimensionParameter}_${photoId}`);
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
    public async getPhotoBlobBySlug(slug: string, dimension: Dimension|'source'|'medium'|'thumbnail' = 'source'): Promise<BlobResponse> {
        let dimensionParameter = '';
        switch(dimension) {
            case 'source':
            case Dimension.SOURCE:
                dimensionParameter += 'source';
                break;
            case 'medium':
            case Dimension.MEDIUM:
                dimensionParameter += 'medium';
                break;
            case 'thumbnail':
            case Dimension.THUMBNAIL:
                dimensionParameter += 'thumbnail';
                break;
        }

        var blobResponse: BlobResponse = {
            contentType: null,
            contentLength: null
        };

        return await this.get(`/${dimensionParameter}/slug/${slug}/blob`)
            .then(
                res => {
                    blobResponse.contentType = 
                        res.headers.get('Content-Type') || res.headers.get('content-type');
                    blobResponse.contentLength =
                        res.headers.get('Content-Length') || res.headers.get('content-length');
                    
                    return res.blob();
                }
            )
            .then(blob => {
                blobResponse.file = new File([blob], `${dimensionParameter}_${slug}`);
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
