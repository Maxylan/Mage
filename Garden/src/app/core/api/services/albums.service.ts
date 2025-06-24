import { Injectable } from '@angular/core';
import ApiBase from '../../classes/base.class';
import { DisplayAlbum } from '../../types/generated/display-album';
import { FilterAlbumsParameters } from '../../types/filter-albums-parameters';
import { SearchAlbumsParameters } from '../../types/search-albums-parameters';
import { MutateAlbum } from '../../types/generated/mutate-album';
import { ITag } from '../../types/generated/i-tag';
import { ITagDTO } from '../../types/generated/tag-dto';

@Injectable({
    providedIn: 'root'
})
export class AlbumsService extends ApiBase {
    /**
     * Get a single <see cref="AlbumDTO"/> by its <paramref name="albumId: "/> (PK, uint).
     *
     * [HttpGet("{album_id:int}")]
     */
    public async getAlbum(albumId: number): Promise<DisplayAlbum> {
        return await this.get('/albums/' + albumId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getAlbum] Error!', err);
                    return err;
                }
            );
    }


    /**
     * Get / Query for many <see cref="DisplayAlbum"/> instances filtered by the
     * given parameters passed.
     *
     * [HttpGet]
     */
    public async filterDisplayPhotos(
        params: FilterAlbumsParameters|string
    ): Promise<DisplayAlbum[]> {
        return this._filterOrSearchAlbums(
            '/albums',
            params
        );
    }

    /**
     * Get / Query for many <see cref="DisplayAlbum"/> instances that match provided
     * search criterias passed as URL/Query Parameters.
     *
     * [HttpGet("search")]
     */
    public async searchDisplayPhotos(
        params: SearchAlbumsParameters|string
    ): Promise<DisplayAlbum[]> {
        return this._filterOrSearchAlbums(
            '/albums/search',
            params
        );
    }

    /** Internal */
    private async _filterOrSearchAlbums<TParams extends object, TReturn extends object>(
        baseEndpoint: string,
        params: TParams|string
    ): Promise<TReturn[]> {
        let queryParameters: string = baseEndpoint;

        if (typeof params === 'string') {
            if (!params.startsWith('?')) {
                throw new Error('[_filterOrSearchAlbums] Incorrectly formatted string `params`');
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

                console.warn('[_filterOrSearchAlbums] Recieved a bad response!', res);
                return Promise.resolve([]);
            })
            .catch(
                err => {
                    console.error('[_filterOrSearchAlbums] Error!', err);
                    return err;
                }
            );
    }


    /**
     * Create a new <see cref="Album"/>.
     *
     * [HttpPost]
     */
    public async createAlbum(mut: MutateAlbum): Promise<DisplayAlbum> {
        const body = JSON.stringify(mut);

        return await this.post('/albums', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[createAlbum] Error!', err);
                    return err;
                }
            );
    }


    /**
     * Update the properties of the <see cref="Album"/> with '<paramref ref="albumId"/>' (int).
     *
     * [HttpPut("{album_id:int}")]
     */
    public async updateAlbum(albumId: number, mut: MutateAlbum): Promise<DisplayAlbum> {
        const body = JSON.stringify(mut);

        return await this.put('/albums/' + albumId, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[updateAlbum] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Add <see cref="Tag"/>(s) (<paramref name="tags"/>) ..to a <see cref="Album"/> identified by PK '<paramref ref="albumId"/>' (int)
     *
     * [HttpPatch("{album_id:int}/favorite")]
     */
    public async toggleFavorite(albumId: number): Promise<void> {
        return await this.patch(`/albums/${albumId}/favorite`)
            .catch(
                err => {
                    console.error('[toggleFavorite] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Add photos (<paramref name="photo_ids"/>, int[]) to a given
     * <see cref="Album"/> (<paramref name="albumId: "/>).
     *
     * [HttpPatch("{album_id:int}/add/photos")]
     */
    public async mutatePhotos(albumId: number, photoIds: number[]): Promise<DisplayAlbum> {
        const body = JSON.stringify(photoIds);

        return await this.patch(`/albums/${albumId}/add/photos`, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[mutatePhotos] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Remove photos (<paramref name="photo_ids"/>, int[]) from a given 
     * <see cref="Album"/> (<paramref name="albumId: "/>).
     *
     * [HttpPatch("{album_id:int}/remove/photos")]
     */
    public async removePhotos(albumId: number, photoIds: number[]): Promise<DisplayAlbum> {
        const body = JSON.stringify(photoIds);

        return await this.patch(`/albums/${albumId}/remove/photos`, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[removePhotos] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Add tags (<paramref name="tags"/>, <see cref="IEnumerable{ITag}"/>) to a
     * given <see cref="Album"/> (<paramref name="albumId: "/>).
     *
     * [HttpPatch("{album_id:int}/add/tags")]
     */
    public async mutateTags(albumId: number, tags: ITag[]): Promise<ITagDTO[]> {
        const body = JSON.stringify(tags);

        return await this.patch(`/albums/${albumId}/add/tags`, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[mutateTags] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Remove tags (<paramref name="tags"/>, <see cref="IEnumerable{ITag}"/>) from
     * a given <see cref="Album"/> (<paramref name="albumId: "/>).
     *
     * [HttpPatch("{album_id:int}/remove/tags")]
     */
    public async removeTags(albumId: number, tags: ITag[]): Promise<ITagDTO[]> {
        const body = JSON.stringify(tags);

        return await this.patch(`/albums/${albumId}/remove/tags`, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[removeTags] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Delete the <see cref="Album"/> with '<paramref ref="albumId"/>' (int).
     *
     * [HttpDelete("{album_id:int}")]
     */
    public async deleteAlbum(albumId: number): Promise<void> {
        return await this.delete('/albums/' + albumId)
            .catch(
                err => {
                    console.error('[deleteAlbum] Error!', err);
                    return err;
                }
            );
    }
}
