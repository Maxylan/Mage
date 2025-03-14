import { Component, EventEmitter, Input, Output, Signal, signal } from '@angular/core';
import { MatRipple } from '@angular/material/core';
import { AsyncPipe, NgClass } from '@angular/common';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { Observable } from 'rxjs';
import {
    CardDetails,
    CardLinkDetails,
    CardSelectDetails
} from './card-with-thumbnail.types';

@Component({
    selector: 'shared-card-with-thumbnail',
    imports: [
        NgClass,
        MatRipple,
        MatCheckboxModule,
        MatMenuModule,
        MatIconModule,
        AsyncPipe
    ],
    templateUrl: 'card-with-thumbnail.component.html',
    styleUrl: 'card-with-thumbnail.component.css'
})
export class ThumbnailCardComponent {
    @Input({ required: true })
    public isHandset!: Observable<boolean>;

    @Input({ required: true })
    public key!: string;

    @Input({ required: true })
    public title!: string;

    @Input()
    public summary?: string;

    @Input()
    public link?: string;

    @Input()
    public shareLink?: string;

    @Input()
    public showSelect: boolean = false;

    @Input()
    public initialIsFavorite: boolean = false;
    public isFavorite: Signal<boolean> = signal(this.initialIsFavorite);

    @Input()
    public initialIsSelected: boolean = false;
    public isSelected: Signal<boolean> = signal(this.initialIsSelected);

    private isKebabOpen: Signal<boolean> = signal(false);

    /**
     * Get all the internal properties of this card as an object instance.
     */
    public getCardDetails = (): CardDetails => ({
        key: this.key,
        title: this.title,
        summary: this.summary || null,
        link: this.link || null
    });

    /**
     * Selects this card.
     */
    @Output()
    public select = (): CardSelectDetails => this.selected();
    /**
     * Emits when `{selected}`
     */
    @Output()
    public selectedEvent = new EventEmitter();
    /**
     * Callback firing when this card gets selected/de-selected
     */
    public selected = (): CardSelectDetails => {
        const cardDetails: CardSelectDetails = {
            selected: this.isSelected(),
            card: this.getCardDetails()
        };

        this.selectedEvent.emit(cardDetails);
        return cardDetails;
    }

    /**
     * Copies (returns) this card's link
     */
    @Output()
    public copy = (): string|null => this.copied();
    /**
     * Emits when `{copied}`
     */
    @Output()
    public copiedEvent = new EventEmitter();
    /**
     * Callback firing when this card gets copied.
     */
    public copied = (): string|null => {
        if (this.link) {
            this.copiedEvent.emit({
                link: this.link,
                card: this.getCardDetails()
            } as CardLinkDetails);

            return this.link;
        }

        return null;
    }

    /**
     * Copies (returns) a link used to *share* this card
     */
    @Output()
    public share = (): string|null => this.shared();
    /**
     * Emits when `{shared}`
     */
    @Output()
    public shareEvent = new EventEmitter();
    /**
     * Callback firing when this card gets shared.
     */
    public shared = (): string|null => {
        if (this.shareLink) {
            this.copiedEvent.emit({
                link: this.shareLink,
                card: this.getCardDetails()
            } as CardLinkDetails);
        }

        return null;
    }

    /*
    imageEncoded: string|null = null;
    imageContentType: string|null = null;
    imageContentLength: number|null = null;
    imageIsLoading = signal<boolean>(false);

    private getPhoto = effect(() => {
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
    }); */
}
