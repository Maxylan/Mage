import { Component, Input, inject, signal, effect, computed, ElementRef, afterRender } from '@angular/core';
import { MatCard, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { PhotosService } from '../../../core/api/photos.service';
import { MatRipple } from '@angular/material/core';
import { MatProgressBar } from '@angular/material/progress-bar';
import { Dimension, Photo } from '../../../core/types/photos.types';
import { BlobResponse } from '../../../core/types/generic.types';
import { Observable } from 'rxjs';

@Component({
    selector: 'shared-photo-card',
    imports: [
        MatCard,
        MatCardContent,
        MatCardSubtitle,
        /* MatRipple, */
        MatProgressBar
    ],
    templateUrl: 'photo-card.component.html',
    styleUrl: 'photo-card.component.css'
})
export class PhotoCardComponent {
    private photoService = inject(PhotosService);

    @Input({ required: true })
    photo!: Photo;

    imageEncoded: string|null = null;
    imageContentType: string|null = null;
    imageContentLength: number|null = null;
    imageIsLoading = signal<boolean>(false);

    @Input({ required: true })
    isHandset!: Observable<boolean>;

    imageMaxWidth = signal<6|8>(6);
    imageMaxHeight = signal<6|8>(6);

    cardInlineStyling = computed<string>(() => `width: ${this.imageMaxWidth()}rem; height: ${this.imageMaxHeight() + 2}rem;`);
    imageInlineStyling = computed<string>(() => `object-fit: contain; min-width: ${this.imageMaxWidth()}rem; min-height: ${this.imageMaxHeight()}rem;`);

    ngOnInit() {
        this.imageIsLoading.set(true);
        this.photoService
            .getPhotoBlob(this.photo.photoId, 'thumbnail')
            .then(blobResponse => {
                this.imageContentType = blobResponse.contentType ?? 'application/octet-stream';
                this.imageContentLength = blobResponse.file?.size ?? 0;

                if (!this.imageContentType.startsWith('image')) {
                    return Promise.reject('Response is not an image!');
                }
                if (!blobResponse.file) {
                    return Promise.reject('Response is not an image!');
                }

                return blobResponse.file.arrayBuffer();
            })
            .then(buffer => {
                this.imageEncoded = (
                    `data:${this.imageContentType};base64,` + // ..i wonder, at what point is @ts-ignore acceptable :p
                    (new Uint8Array(buffer) as Uint8Array & { toBase64: () => string|null}).toBase64()
                );
            })
            .catch(err => {
                console.error('[PhotoCardComponent.onLoad] Error!', err);
                this.imageIsLoading.set(false);
            })
            .finally(() => this.imageIsLoading.set(false));

        this.isHandset.subscribe(isHandset => {
            this.imageMaxWidth.set(isHandset ? 6 : 8);
            this.imageMaxHeight.set(isHandset ? 6 : 8);
        });
    };
}
