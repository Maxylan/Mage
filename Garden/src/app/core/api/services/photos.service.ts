import { Injectable } from '@angular/core';
import ApiBase from '../../classes/base.class';
import { Photo } from '../../types/generated/photo';
import { DisplayPhoto } from '../../types/generated/display-photo';
import { Dimension } from '../../types/generated/dimension';
import { BlobResponse } from '../../types/blob-response';
import { FilterPhotosParameters } from '../../types/filter-photos-parameters';
import { SearchPhotosParameters } from '../../types/search-photos-parameters';
import { MutatePhoto } from '../../types/generated/mutate-photo';
import { PhotoTagCollection } from '../../types/generated/photo-tag-collection';
import { ITag } from '../../types/generated/i-tag';
import { ITagDTO } from '../../types/generated/tag-dto';

@Injectable({
    providedIn: 'root'
})
export class PhotosService extends ApiBase {
    /**
     * Get a single <see cref="Photo"/> (single source) by its <paramref name="photoId"/> (PK, uint).
     *
     * [HttpGet("{photo_id:int}")]
     */
    public async getSourcePhotoById(photoId: number): Promise<Photo> {
        return await this.get('/photos/' + photoId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getSourcePhotoById] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single <see cref="Photo"/> (single source) by its <paramref name="slug"/> (string).
     *
     * [HttpGet("slug/{slug}")]
     */
    public async getSourcePhotoBySlug(slug: string): Promise<Photo> {
        return await this.get('/photos/slug/' + slug)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getSourcePhotoById] Error!', err);
                    return err;
                }
            );
    }


    /**
     * Get a single <see cref="DisplayPhoto"/> by its <paramref name="photoId"/> (PK, uint).
     *
     * [HttpGet("{photo_id:int}/display")]
     */
    public async getPhotoById(photoId: number): Promise<DisplayPhoto> {
        return await this.get('/photos/' + photoId + '/display')
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getSourcePhotoById] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get a single <see cref="DisplayPhoto"/> by its <paramref name="slug"/> (string).
     *
     * [HttpGet("slug/{slug}/display")]
     */
    public async getPhotoBySlug(slug: string): Promise<DisplayPhoto> {
        return await this.get('/photos/slug/' + slug)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getSourcePhotoById] Error!', err);
                    return err;
                }
            );
    }


    /**
     * Get the Image `File` (blob) of the Photo with the given `photoId` (PK, uint)
     *
     * [HttpGet("{photo_id:int}/blob/source")]
     * [HttpGet("{photo_id:int}/blob/medium")]
     * [HttpGet("{photo_id:int}/blob/thumbnail")]
     */
    public async getPhotoBlob(photoId: number, dimension: Dimension|'source'|'medium'|'thumbnail' = 'source'): Promise<BlobResponse> {
        let dimensionParameter = 'source';
        switch(dimension) {
            case 'source':
            case Dimension.SOURCE:
                dimensionParameter = 'source';
                break;
            case 'medium':
            case Dimension.MEDIUM:
                dimensionParameter = 'medium';
                break;
            case 'thumbnail':
            case Dimension.THUMBNAIL:
                dimensionParameter = 'thumbnail';
                break;
        }

        var blobResponse: BlobResponse = {
            contentType: null,
            contentLength: null,
            file: null
        };

        return await this.get(`/photos/${photoId}/blob/${dimensionParameter}`)
            .then(res => {
                if (!res) {
                    return Promise.reject(res);
                }

                blobResponse.contentType = 
                    res.headers.get('Content-Type') || res.headers.get('content-type');
                blobResponse.contentLength =
                    res.headers.get('Content-Length') || res.headers.get('content-length');
                
                return res.blob();
            })
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
     *
     * [HttpGet("slug/{slug}/blob/source")]
     * [HttpGet("slug/{slug}/blob/medium")]
     * [HttpGet("slug/{slug}/blob/thumbnail")]
     */
    public async getPhotoBlobBySlug(slug: string, dimension: Dimension|'source'|'medium'|'thumbnail' = 'source'): Promise<BlobResponse> {
        let dimensionParameter = 'source';
        switch(dimension) {
            case 'source':
            case Dimension.SOURCE:
                dimensionParameter = 'source';
                break;
            case 'medium':
            case Dimension.MEDIUM:
                dimensionParameter = 'medium';
                break;
            case 'thumbnail':
            case Dimension.THUMBNAIL:
                dimensionParameter = 'thumbnail';
                break;
        }

        var blobResponse: BlobResponse = {
            contentType: null,
            contentLength: null,
            file: null
        };

        return await this.get(`/photos/slug/${slug}/blob/${dimensionParameter}`)
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


    /**
     * Filter many <see cref="Photo"/>'s matching a number of given criterias passed
     * by URL/Query Parameters.
     *
     * [HttpGet]
     */
    public async filterPhotos(
        params: FilterPhotosParameters|string
    ): Promise<Photo[]> {
        return this._filterOrSearchPhotos(
            '/photos',
            params
        );
    }

    /**
     * Filter many <see cref="Photo"/>'s matching a number of given criterias passed
     * by URL/Query Parameters.
     *
     * [HttpGet("display")]
     */
    public async filterDisplayPhotos(
        params: FilterPhotosParameters|string
    ): Promise<DisplayPhoto[]> {
        return this._filterOrSearchPhotos(
            '/photos/display',
            params
        );
    }

    /**
     * Get many <see cref="Photo"/>'s (display) matching a number of given criterias
     * passed by URL/Query Parameters.
     *
     * [HttpGet("search")]
     */
    public async searchPhotos(
        params: SearchPhotosParameters|string
    ): Promise<Photo[]> {
        return this._filterOrSearchPhotos(
            '/photos/search',
            params
        );
    }

    /**
     * Get many <see cref="Photo"/>'s (display) matching a number of given criterias
     * passed by URL/Query Parameters.
     *
     * [HttpGet("search/display")]
     */
    public async searchDisplayPhotos(
        params: SearchPhotosParameters|string
    ): Promise<DisplayPhoto[]> {
        return this._filterOrSearchPhotos(
            '/photos/search/display',
            params
        );
    }

    /** Internal */
    private async _filterOrSearchPhotos<TParams extends object, TReturn extends object>(
        baseEndpoint: string,
        params: TParams|string
    ): Promise<TReturn[]> {
        let queryParameters: string = baseEndpoint;

        if (typeof params === 'string') {
            if (!params.startsWith('?')) {
                throw new Error('[_filterOrSearchPhotos] Incorrectly formatted string `params`');
            }
            
            queryParameters += params;
        }
        else {
            let parameters = Array.from(Object.entries(params))
                .filter(kvp => !(
                    kvp[0] === null
                    || kvp[1] === null
                    || kvp[0] === undefined
                    || kvp[1] === undefined
                ))
                .map(kvp => `${kvp[0]}=${kvp[1]!.toString().trim()}`);

            queryParameters += parameters.length > 0
                ? '?' + parameters.join('&')
                : '';
        }

        return await this.get(queryParameters)
            .then(res => {
                if ('json' in res) {
                    return res.json()
                }

                console.warn('[_filterOrSearchPhotos] Recieved a bad response!', res);
                return Promise.resolve([]);
            })
            .catch(
                err => {
                    console.error('[_filterOrSearchPhotos] Error!', err);
                    return err;
                }
            );
    }


    /**
     * Upload any amount of photos/files by streaming them one-by-one to disk.
     *
     * [HttpPost("upload")]
     * [RequestTimeout(milliseconds: 60000)]
     */
    public async uploadPhotos(file: File): Promise<DisplayPhoto> {
        return await this.post('/photos/upload', {
            body: file,
            headers: {
                'Content-Type': file.type
            } 
        })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[uploadPhotos] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Update a single <see cref="Photo"/> by its ID (PK, uint).
     *
     * [HttpPut]
     */
    public async updatePhoto(mut: MutatePhoto): Promise<Photo> {
        const body = JSON.stringify(mut);

        return await this.put('/photos', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[updatePhoto] Error!', err);
                    return err;
                }
            );
    }


    /**
     * Toggles the 'Favorite' status of a <see cref="Reception.Database.Models.Photo"/> for a single user.
     *
     * [HttpPatch("{photo_id:int}/favorite")]
     */
    public async toggleFavorite(photoId: number): Promise<Response> {
        return await this.patch('/photos/' + photoId + '/favorite')
            .catch(
                err => {
                    console.error('[toggleFavorite] Error!', err);
                    return err;
                }
            );
    }


    /**
     * Edit tags associated with this <see cref="PhotoEntity"/>.
     *
     * [HttpPut("{photo_id:int}/tags")]
     */
    public async mutateTags(photoId: number, tags: ITag[]): Promise<PhotoTagCollection> {
        const body = JSON.stringify(tags);

        return await this.put('/photos/' + photoId + '/tags', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[mutateTags] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Add <see cref="Tag"/>(s) (<paramref name="tags"/>) ..to a <see cref="Photo"/> identified by PK '<paramref ref="photoId"/>' (int)
     *
     * [HttpPatch("{photo_id:int}/tags/add")]
     */
    public async addTags(photoId: number, tags: ITag[]): Promise<ITagDTO[]> {
        const body = JSON.stringify(tags);

        return await this.patch('/photos/' + photoId + '/tags/add', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[addTags] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Remove <see cref="Tag"/>(s) (<paramref name="tags"/>) ..from a <see cref="Photo"/> identified by PK '<paramref ref="photoId"/>' (int)
     *
     * [HttpPatch("{photo_id:int}/tags/remove")]
     */
    public async removeTags(photoId: number, tags: ITag[]): Promise<ITagDTO[]> {
        const body = JSON.stringify(tags);

        return await this.patch('/photos/' + photoId + '/tags/remove', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[removeTags] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Delete the <see cref="PhotoEntity"/> with '<paramref ref="photoId"/>' (int).
     *
     * [HttpDelete("{photo_id:int}")]
     */
    public async deletePhoto(photoId: number): Promise<Response> {
        return await this.delete('/photos/' + photoId)
            .catch(
                err => {
                    console.error('[deletePhoto] Error!', err);
                    return err;
                }
            );
    }
}
