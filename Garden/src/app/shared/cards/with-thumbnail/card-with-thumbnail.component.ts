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
import { MatButtonModule } from '@angular/material/button';

@Component({
    selector: 'shared-card-with-thumbnail',
    imports: [
        NgClass,
        MatRipple,
        MatCheckboxModule,
        MatButtonModule,
        MatIconModule,
        MatMenuModule,
        AsyncPipe
    ],
    templateUrl: 'card-with-thumbnail.component.html',
    styleUrl: 'card-with-thumbnail.component.css'
})
export class ThumbnailCardComponent {
    @Input({ required: true })
    public isHandsetObservable!: Observable<boolean>;

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
    public isSelected?: boolean;
    @Input()
    public isInSelectMode?: boolean;
    @Input()
    public select?: (isSelected: boolean) => void;

    /**
     * Compute if we should show the 'select' checkbox.
     * Only determines checbox visibility, not 'checked' status.
     */
    public readonly showSelect: boolean = !!(
        this.select !== undefined &&
        this.isSelected !== undefined &&
        this.isInSelectMode !== undefined && (
            this.isSelected || 
            this.isInSelectMode
        )
    );

    @Input()
    public initialIsFavorite: boolean = false;
    public isFavorite: Signal<boolean> = signal(this.initialIsFavorite);

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
     * Emits when `{clicked}`
     */
    @Output()
    public onClick$ = new EventEmitter();
    /**
     * Callback firing when this card gets clicked
     */
    public clicked = (event?: Event): CardDetails|null => {
        if (event) {
            if ('preventDefault' in event) {
                event.preventDefault();
            }

            console.debug('event', event.bubbles, event.target);
        }

        const cardDetails = this.getCardDetails();
        console.debug('Clicked card', cardDetails);
        this.onClick$.emit(cardDetails);
        return cardDetails;
    }

    /**
     * Emits when `{selected}`
     */
    @Output()
    public onSelect$ = new EventEmitter();
    /**
     * Callback firing when this card gets selected/de-selected
     */
    public selected = (): CardDetails|null => {
        if (this.select === undefined ||
            this.isSelected === undefined ||
            this.isInSelectMode === undefined) {
            return null;
        }

        console.log('selected ', this.isSelected);
        this.select(this.isSelected);

        const cardDetails = this.getCardDetails();
        this.onSelect$.emit(cardDetails);
        return cardDetails;
    }

    /**
     * Emits when `{copied}`
     */
    @Output()
    public onCopy$ = new EventEmitter();
    /**
     * Callback firing when this card gets copied.
     */
    public copied = (): string|null => {
        if (this.link) {
            this.onCopy$.emit({
                link: this.link,
                card: this.getCardDetails()
            } as CardLinkDetails);

            return this.link;
        }

        return null;
    }

    /**
     * Emits when `{shared}`
     */
    @Output()
    public onShare$ = new EventEmitter();
    /**
     * Callback firing when this card gets shared.
     */
    public shared = (): string|null => {
        if (this.shareLink) {
            this.onShare$.emit({
                link: this.shareLink,
                card: this.getCardDetails()
            } as CardLinkDetails);
        }

        return null;
    }
}
