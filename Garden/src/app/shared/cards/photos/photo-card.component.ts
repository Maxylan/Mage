import { Component, Input, inject, signal, computed } from '@angular/core';
import { PhotosService } from '../../../core/api/photos.service';
import { MatRipple } from '@angular/material/core';
import { MatProgressBar } from '@angular/material/progress-bar';
import { Dimension, Photo } from '../../../core/types/photos.types';
import { BlobResponse } from '../../../core/types/generic.types';
import { AsyncPipe, NgClass } from '@angular/common';
import { Observable } from 'rxjs';

@Component({
    selector: 'shared-photo-card',
    imports: [
        NgClass,
        MatRipple,
        MatProgressBar,
        AsyncPipe
    ],
    templateUrl: 'photo-card.component.html',
    styleUrl: 'second-attempt-photo-card.component.css'
})
export class PhotoCardWrapperComponent {
    private photoService = inject(PhotosService);

    private imageEncoded: string|null = null;
    private imageContentType: string|null = null;
    private imageContentLength: string|null = null;
    private imageIsLoading = signal<boolean>(false);

    @Input()
    public link = '#';

    public getPhotoBlob = () => {
        var blobResponse: BlobResponse = {
            contentType: null,
            contentLength: null
        };

        this.imageIsLoading.set(true);
        this.photoService
            .get(this.link)
            .then(
                res => {
                    this.imageContentType = 
                        res.headers.get('Content-Type') || res.headers.get('content-type');
                    this.imageContentLength =
                        res.headers.get('Content-Length') || res.headers.get('content-length');
                    
                    return res.blob();
                }
            )
            .then(blob => {/*
                this.imageContentType = blob.contentType ?? 'application/octet-stream';

                if (!this.imageContentType.startsWith('image')) {
                    return Promise.reject('Bad content type!');
                }
                if (!blobResponse.file) {
                    return Promise.reject('Response is not an image!');
                } */
                return blob.arrayBuffer();
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
    }

    ngOnInit() {
        this.getPhotoBlob();
    };
}
