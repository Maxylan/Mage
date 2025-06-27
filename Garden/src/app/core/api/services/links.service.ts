import { Injectable } from '@angular/core';
import ApiBase from '../../classes/base.class';
import { IPublicLinkDTO } from '../../types/generated/public-link-dto';
import { MutateLink } from '../../types/generated/mutate-link';

@Injectable({
    providedIn: 'root'
})
export class LinksService extends ApiBase {
    public static readonly VIEW_SOURCE_URL: string = '/links/view/source/';
    public static readonly VIEW_MEDIUM_URL: string = '/links/view/medium/';
    public static readonly VIEW_THUMBNAIL_URL: string = '/links/view/thumbnail/';

    /**
     * Get the <see cref="Link"/> with Primary Key '<paramref ref="linkId"/>'
     *
     * [HttpGet("{link_id:int}")]
     */
    public async getLinkById(linkId: number): Promise<IPublicLinkDTO> {
        return await this.get('/links/' + linkId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getLinkById] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get the <see cref="Link"/> with Unique '<paramref ref="code"/>' (string)
     *
     * [HttpGet("code/{code}")]
     */
    public async getLinkByCode(code: string): Promise<IPublicLinkDTO> {
        return await this.get('/links/code/' + code)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getLinkByCode] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get all <see cref="Link"/> entries.
     *
     * [HttpGet]
     */
    public async getLinks(limit: number = 99, offset: number = 0): Promise<IPublicLinkDTO[]> {
        return await this.get('/links' + this.queryParameters({ limit, offset }))
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getLinks] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Get all <strong>*active*</string> <see cref="Link"/> entries.
     *
     * [HttpGet("active")]
     */
    public async getActiveLinks(limit: number = 99, offset: number = 0): Promise<IPublicLinkDTO[]> {
        return await this.get('/links/active' + this.queryParameters({ limit, offset }))
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getActiveLinks] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Create a <see cref="Link"/> to the <see cref="PhotoEntity"/> with ID '<paramref name="photo_id"/>'.
     *
     * [HttpPost("{photo_id:int}")]
     */
    public async createLink(photoId: number, mut: MutateLink): Promise<IPublicLinkDTO> {
        const body = JSON.stringify(mut);

        return await this.post('/links/' + photoId, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[createLink] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/>.
     *
     * [HttpPut("{link_id:int}")]
     */
    public async updateLink(linkId: number, mut: MutateLink): Promise<IPublicLinkDTO> {
        const body = JSON.stringify(mut);

        return await this.put('/links/' + linkId, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[updateLink] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Update the properties of a <see cref="Link"/> to a <see cref="PhotoEntity"/> by 
     * GUID '<paramref name="code"/>'.
     *
     * [HttpPut("code/{code}")]
     */
    public async updateLinkByCode(code: string, mut: MutateLink): Promise<IPublicLinkDTO> {
        const body = JSON.stringify(mut);

        return await this.put('/links/code/' + code, { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[updateLinkByCode] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Delete the <see cref="Link"/> with Primary Key '<paramref ref="linkId"/>'
     *
     * [HttpDelete("{link_id:int}")]
     */
    public async deleteLinkById(linkId: number): Promise<IPublicLinkDTO> {
        return await this.delete('/links/' + linkId)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[deleteLinkById] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Delete the <see cref="Link"/> with GUID '<paramref ref="code"/>'
     *
     * [HttpDelete("code/{code}")]
     */
    public async deleteLinkByCode(code: string): Promise<IPublicLinkDTO> {
        return await this.delete('/links/code/' + code)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[deleteLinkByCode] Error!', err);
                    return err;
                }
            );

    }
}
