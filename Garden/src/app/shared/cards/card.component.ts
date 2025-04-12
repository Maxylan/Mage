import { Component, computed, input, output, signal } from '@angular/core';
import {
    SelectionObserver,
    CardSelectionState,
    SelectionState
} from '../../pages/photos/selection-observer.component';
import { CardMenuItemComponent } from './menu-item/card-menu-item.component';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatRipple } from '@angular/material/core';
import { NgClass } from '@angular/common';
import { CardDetails } from './card.types';

@Component({
    selector: 'shared-card',
    imports: [
        CardMenuItemComponent,
        MatCheckboxModule,
        MatButtonModule,
        MatIconModule,
        MatMenuModule,
        MatRipple,
        NgClass
    ],
    templateUrl: 'card.component.html',
    styleUrl: 'card.component.css'
})
export class CardComponent {
    public readonly key = input.required<string>();
    public readonly title = input.required<string>();
    public readonly isHandset = input.required<boolean|undefined>();
    public readonly deselect = input.required<SelectionObserver['deselectItems']>();
    public readonly select = input.required<SelectionObserver['selectItems']>();
    public readonly selectionState = input.required<SelectionState>();
    public readonly isSelected = computed<boolean>(() => {
        const
            state = this.selectionState(),
            key = this.key();
        if (!state || !key) {
            return false;
        }
        return state.selection.includes(key);
    });

    public readonly shareUrl = input<string>();
    public readonly linkUrl = input<string>();
    public readonly summary = input<string>();

    public readonly isKebabOpen = signal<boolean>(false);

    /**
     * Get some of the internal properties of this card as an object instance.
     */
    public readonly cardDetails = computed<CardDetails>(() => ({
        key: this.key(),
        title: this.title(),
        summary: this.summary() || null
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

        if (this.selectionState().selectModeActive) {
            this.selected();
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
        const state = this.selectionState();
        if (!state) {
            return;
        }

        if (this.isSelected()) {
            this.deselect()(this.key());
        }
        else {
            this.select()(this.key());
        }

        this.onSelect.emit(
            this.cardDetails()
        );
    }

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
