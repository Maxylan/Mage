import { Component, inject, signal, effect, input, output, untracked } from '@angular/core';
import { PhotosService } from '../../../core/api/photos.service';
import { Photo } from '../../../core/types/photos.types';
import { CardThumbnailComponent } from '../../../shared/cards/thumbnail/card-thumbnail.component';
import { CardComponent } from '../../../shared/cards/card.component';
import { SelectionObserver, SelectionState } from '../../../layout/toolbar/selection-observer.component';

@Component({
    selector: 'photo-card',
    imports: [
        CardThumbnailComponent,
        CardComponent
    ],
    templateUrl: 'photo-card.component.html'
})
export class PhotoCardComponent {
    private readonly photoService = inject(PhotosService);

    public readonly photo = input.required<Photo>();
    public readonly isHandset = input.required<boolean|undefined>();
    public readonly deselect = input.required<SelectionObserver['deselectItems']>();
    public readonly select = input.required<SelectionObserver['selectItems']>();
    public readonly selectionState = input.required<SelectionState>();

    public readonly shareUrl = input<string>();
    public readonly linkUrl = input<string>();

    public readonly imageContentLength = signal<number|null>(null);
    public readonly imageContentType = signal<string|null>(null);
    public readonly image = signal<string|null>(null);

    public readonly isFavorite = input<() => boolean>();

    public readonly blob = effect(() => {
        if (untracked(this.image) !== null) {
            this.image.set(null);
        }
        this.photoService
            .getPhotoBlob(this.photo().photoId, 'thumbnail')
            .then(blob => {
                const contentType = blob.contentType?.trim()?.normalize() ?? 'application/octet-stream';
                this.imageContentType.set(contentType);

                if (!contentType.startsWith('image')) {
                    return Promise.reject('Bad content type!');
                }
                if (!blob.file) {
                    return Promise.reject('Response is not an image!');
                }

                return blob.file.arrayBuffer();
            })
            .then(buffer => {
                this.imageContentLength.set(buffer.byteLength);
                this.image.set(
                    `data:${this.imageContentType()};base64,` + // ..i wonder, at what point is @ts-ignore acceptable :p
                    (new Uint8Array(buffer) as Uint8Array & { toBase64: () => string|null}).toBase64()
                );
            })
            .catch(err => {
                console.error('[PhotoCardComponent.onLoad] Error!', err);
            });
    });
}
