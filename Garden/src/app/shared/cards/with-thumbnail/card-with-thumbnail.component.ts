import { Component, computed, EventEmitter, input, Input, model, output, Output, Signal, signal } from '@angular/core';
import { MatRipple } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { NgClass } from '@angular/common';
import {
    CardDetails,
    CardLinkDetails,
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
        MatMenuModule
    ],
    templateUrl: 'card-with-thumbnail.component.html',
    styleUrl: 'card-with-thumbnail.component.css'
})
export class ThumbnailCardComponent {
    public readonly isHandset = input.required<boolean>();
    public readonly key = input.required<string>();
    public readonly title = input.required<string>();

    public readonly summary = input<string>();
    public readonly link = input<string>();
    public readonly share = input<string>();
    public readonly isSelected = input<boolean>();
    public readonly isInSelectMode = input<boolean>();
    public readonly select = model<boolean>();

    /**
     * Compute if we should show the 'select' checkbox.
     * Only determines checbox visibility, not 'checked' status.
     */
    public readonly showSelect = computed(() => (
        this.select() !== undefined &&
        this.isSelected() !== undefined &&
        this.isInSelectMode() !== undefined && (
            this.isSelected() || 
            this.isInSelectMode()
        )
    ));

    public readonly isFavorite = input<boolean>(false);
    public readonly isKebabOpen = signal<boolean>(false);

    /**
     * Get some of the internal properties of this card as an object instance.
     */
    public readonly cardDetails = computed<CardDetails>(() => ({
        key: this.key(),
        title: this.title(),
        summary: this.summary() || null,
        link: this.link() || null
    }));

    /**
     * Emits when `{clicked}`
     */
    public readonly onClick = output<CardDetails>();
    /**
     * Callback firing when this card gets clicked
     */
    public readonly clicked = (event?: Event): void => {
        if (event) {
            if ('preventDefault' in event) {
                event.preventDefault();
            }
        }

        this.onClick.emit(
            this.cardDetails()
        );
    }

    /**
     * Emits when `{selected}`
     */
    public readonly onSelect = output<CardDetails>();
    /**
     * Callback firing when this card gets selected/de-selected
     */
    public readonly selected = (): void => {
        if (this.select() === undefined ||
            this.isSelected() === undefined ||
            this.isInSelectMode() === undefined) {
            return;
        }

        this.select.update(this.isSelected);

        this.onSelect.emit(
            this.cardDetails()
        );
    }

    /**
     * Emits when `{copied}`
     */
    public readonly onCopy = output<CardLinkDetails>();
    /**
     * Callback firing when this card gets copied.
     */
    public readonly copied = (): void => {
        const link = this.link();
        if (link) {
            this.onCopy.emit({
                link: link,
                card: this.cardDetails()
            } as CardLinkDetails);
        }
    }

    /**
     * Emits when `{shared}`
     */
    public readonly onShare = output<CardLinkDetails>();
    /**
     * Callback firing when this card gets shared.
     */
    public readonly shared = (): void => {
        const share = this.share();
        if (share) {
            this.onShare.emit({
                link: share,
                card: this.cardDetails()
            } as CardLinkDetails);
        }
    }
}
