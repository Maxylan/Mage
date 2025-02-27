import { Component, Input, inject, signal, effect, computed, ElementRef, afterRender } from '@angular/core';
import { MatCard } from '@angular/material/card';
import { PhotosService } from '../../../core/api/photos.service';
import { MatRipple } from '@angular/material/core';
import { MatProgressBar } from '@angular/material/progress-bar';
import { Dimension, Photo } from '../../../core/types/photos.types';
import { BlobResponse } from '../../../core/types/generic.types';

@Component({
    selector: 'shared-photo-card',
    imports: [
        MatCard,
        /* MatRipple, */
        MatProgressBar
    ],
    templateUrl: 'photo-card.component.html',
    styleUrl: 'photo-card.component.css'
})
export class PhotoCardComponent {
    @Input()
    photo!: Photo;

    imageContentType: string|null = null;
    imageContentLength: number|null = null;
    imageIsLoading = signal<boolean>(false);
    imageEncoded: string|null = null;

    private currentPhotoId: number = this.photo?.photoId ?? 0;
    private photoService = inject(PhotosService);
    logger = effect(
        () => console.log('lemme see', this.imageIsLoading(), this.imageEncoded, this.imageContentType, this.imageContentLength)
    );
    ngOnInit() {
        // try {
            this.imageIsLoading.set(true);
            this.photoService
                .getPhotoBlob(this.photo.photoId, 'thumbnail')
                .then(blobResponse => {
                    this.imageContentType = blobResponse.contentType ?? 'application/octet-stream';
                    this.imageContentLength = blobResponse.file?.size ?? 0;
                    
                        console.log('???');
                    if (!this.imageContentType.startsWith('image')) {
                        console.log('huh');
                        return Promise.reject('Response is not an image!');
                    }
                    if (!blobResponse.file) {
                        console.log('bruhv', blobResponse);
                        return Promise.reject('Response is not an image!');
                    }

                    return blobResponse.file.arrayBuffer();
                })
                .then(buffer => { // ..i wonder, at what point is @ts-ignore acceptable :p
                    let encoded = (
                        `data:${this.imageContentType};base64,` +
                        (new Uint8Array(buffer) as Uint8Array & { toBase64: () => string|null}).toBase64()
                    );

                    console.log('lemme have a looksie', encoded);
                    this.imageEncoded = encoded;
                    return Promise.resolve(encoded);
                })
                .finally(() => this.imageIsLoading.set(false));
        /* }
        catch (err) {
            console.error('[PhotoCardComponent.onLoad] Error!', err);
            this.imageIsLoading.set(false);
        }
        finally {
            this.currentPhotoId = this.photo.photoId;
        } */
    };
}
