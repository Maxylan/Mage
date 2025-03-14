import { Component, Input, inject, signal, effect } from '@angular/core';
import { PhotosService } from '../../../core/api/photos.service';
import { MatProgressBar } from '@angular/material/progress-bar';
import { MatCardImage } from '@angular/material/card';

@Component({
    selector: 'shared-photo-thumbnail',
    imports: [
        MatCardImage,
        MatProgressBar
    ],
    templateUrl: 'photo-thumbnail.component.html',
    styleUrl: 'photo-thumbnail.component.css'
})
export class PhotoThumbnailComponent {
    private photoService = inject(PhotosService);

    private imageContentType: string|null = null;
    private imageContentLength: string|null = null;

    public imageIsLoading = signal<boolean>(false);
    public imageEncoded: string|null = null;

    @Input()
    public alt?: string;

    @Input()
    public link = '#';

    public getPhotoBlob = () => {
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

    public linkEffect = effect(this.getPhotoBlob);
}
