import { Component, inject, signal, effect, input, output, untracked } from '@angular/core';
import { PhotosService } from '../../../core/api/photos.service';
import { Photo } from '../../../core/types/photos.types';
import { CardThumbnailComponent } from '../../../shared/cards/thumbnail/card-thumbnail.component';
import { CardMenuItemComponent } from '../../../shared/cards/menu-item/card-menu-item.component';
import { CardComponent } from '../../../shared/cards/card.component';

@Component({
    selector: 'photo-card',
    imports: [
        CardThumbnailComponent,
        CardMenuItemComponent,
        CardComponent
    ],
    templateUrl: 'photo-card.component.html'
})
export class PhotoCardComponent {
    private readonly photoService = inject(PhotosService);

    public readonly photo = input.required<Photo>();
    public readonly isHandset = input.required<boolean|undefined>();

    public readonly isSelected = input<boolean>();
    public readonly isInSelectMode = input<boolean>();
    public readonly select = input<(isSelected: boolean) => void>();
    public readonly shareUrl = input<string|undefined>(undefined, { alias: 'share' });
    public readonly linkUrl = input<string|undefined>(undefined, { alias: 'link' });

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
                    `data:${this.imageContentType};base64,` + // ..i wonder, at what point is @ts-ignore acceptable :p
                    (new Uint8Array(buffer) as Uint8Array & { toBase64: () => string|null}).toBase64()
                );
            })
            .catch(err => {
                console.error('[PhotoCardComponent.onLoad] Error!', err);
            });
    });

    /**
     * Emits when `{linked}`
     */
    public readonly onLink = output<URL>();
    /**
     * Callback firing when this card gets copied.
     */
    public readonly linked = (): void => {
        const link = this.linkUrl();
        if (link) {
            this.onLink.emit(new URL(link));
        }
    }

    /**
     * Emits when `{shared}`
     */
    public readonly onShare = output<URL>();
    /**
     * Callback firing when this card gets shared.
     */
    public readonly shared = (): void => {
        const share = this.shareUrl();
        if (share) {
            this.onShare.emit(new URL(share));
        }
    }
}
