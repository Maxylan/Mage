import { Injectable } from '@angular/core';
import ApiBase from '../../classes/base.class';
import { ICategoryDTO } from '../../types/generated/category-dto';
import { DisplayCategory } from '../../types/generated/display-category';
import { MutateCategory } from '../../types/generated/mutate-category';

@Injectable({
    providedIn: 'root'
})
export class CategoriesService extends ApiBase {
    /**
     * Get all categories.
     *
     * [HttpGet]
     */
    public async getCategories(): Promise<ICategoryDTO[]> {
        return await this.get('/categories/')
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getCategories] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="Category"/> with Primary Key '<paramref ref="id"/>' (int)
     *
     * [HttpGet("{id:int}")]
     */
    public async getCategory(id: number): Promise<ICategoryDTO> {
        return await this.get('/categories/' + id)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getCategory] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="Category"/> with unique '<paramref ref="title"/>' (string)
     *
     * [HttpGet("title/{title}")]
     */
    public async getCategoryByTitle(title: string): Promise<ICategoryDTO> {
        return await this.get('/categories/title/' + title)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getCategoryByTitle] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="Category"/> with PK <paramref ref="categoryId"/> (int), along with a collection of all associated Albums.
     *
     * [HttpGet("{category_id:int}/albums")]
     */
    public async getCategoryAlbums(categoryId: number): Promise<DisplayCategory> {
        return await this.get(`/categories/${categoryId}/albums`)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getCategoryAlbums] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Create a new <see cref="Category"/>.
     *
     * [HttpPost]
     */
    public async createCategory(mut: MutateCategory): Promise<ICategoryDTO> {
        const body = JSON.stringify(mut);

        return await this.post('/categories', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[createCategory] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Update the properties of a <see cref="Category"/>.
     *
     * [HttpPut]
     */
    public async updateCategory(mut: MutateCategory): Promise<ICategoryDTO> {
        const body = JSON.stringify(mut);

        return await this.put('/categories', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[updateCategory] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Remove a single <see cref="Album"/> (<paramref name="albumId"/>, int) ..from a single <see cref="Category"/> identified by PK '<paramref ref="categoryId"/>' (int)
     *
     * [HttpPatch("{category_id:int}/remove/album/{album_id:int}")]
     */
    public async removeAlbumFromCategory(categoryId: number, albumId: number): Promise<void> {
        return await this.patch(`/categories/${categoryId}/remove/album/${albumId}`)
            .catch(
                err => {
                    console.error('[removeAlbumFromCategory] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Delete the <see cref="Category"/> with PK <paramref ref="categoryId"/> (int).
     *
     * [HttpDelete("{category_id:int}")]
     */
    public async deleteCategory(categoryId: number): Promise<void> {
        return await this.delete('/categories/' + categoryId)
            .catch(
                err => {
                    console.error('[deleteCategory] Error!', err);
                    return err;
                }
            );
    }
}
