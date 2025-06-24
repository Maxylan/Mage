import { Injectable } from '@angular/core';
import ApiBase from '../../classes/base.class';
import { ITagDTO } from '../../types/generated/tag-dto';
import { TagAlbumCollection } from '../../types/generated/tag-album-collection';
import { TagPhotoCollection } from '../../types/generated/tag-photo-collection';
import { MutateTag } from '../../types/generated/mutate-tag';

@Injectable({
    providedIn: 'root'
})
export class TagsService extends ApiBase {
    /**
     * Get all tags as a <see cref="ITagDTO[]"/> of unique tag names.
     *
     * [HttpGet]
     */
    public async getTags(): Promise<ITagDTO[]> {
        return await this.get('/tags/')
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getTags] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="Tag"/> with Unique '<paramref ref="name"/>' (string)
     *
     * [HttpGet("name/{name}")]
     */
    public async getTag(name: string): Promise<ITagDTO> {
        return await this.get('/tags/name/' + name)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getTag] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along with a collection of all associated Albums.
     *
     * [HttpGet("name/{name}/albums")]
     */
    public async getTagAlbumCollection(name: string): Promise<TagAlbumCollection> {
        return await this.get(`/tags/name/${name}/albums`)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getTagAlbumCollection] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="Tag"/> with '<paramref ref="name"/>' (string) along
     * with a collection of all associated Photos.
     *
     * [HttpGet("name/{name}/photos")]
     */
    public async getTagPhotoCollection(name: string): Promise<TagPhotoCollection> {
        return await this.get(`/tags/name/${name}/photos`)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getTagPhotoCollection] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Create all non-existing tags in the '<paramref ref="tagNames"/>' (string[]) array.
     *
     * [HttpPost]
     */
    public async createTags(tagNames: string[]): Promise<ITagDTO[]> {
        const body = JSON.stringify(tagNames);

        return await this.post('/tags', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[createTags] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Update the properties of the <see cref="Tag"/> with '<paramref ref="name"/>' (string), *not* its members (i.e Photos or Albums).
     *
     * [HttpPut("name/{name}")]
     */
    public async updateTag(name: string, mut: MutateTag): Promise<ITagDTO> {
        const body = JSON.stringify(mut);

        return await this.put('/tags/name/' + name, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[updateTag] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Delete the <see cref="Tag"/> with '<paramref ref="name"/>' (string).
     *
     * [HttpDelete("name/{name}")]
     */
    public async deleteTag(name: string): Promise<void> {
        return await this.delete('/tags/name/' + name)
            .catch(
                err => {
                    console.error('[deleteTag] Error!', err);
                    return err;
                }
            );
    }
}
