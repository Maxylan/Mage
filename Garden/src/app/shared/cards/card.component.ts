import { Component, computed, effect, inject, input, output, signal, viewChild } from '@angular/core';
import { SelectionObserver, SelectionState } from '../../layout/toolbar/selection-observer.component';
import { CardSummaryDialogComponent } from './summary/card-summary.component';
import { CardMenuItemComponent } from './menu-item/card-menu-item.component';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule, MatMenuTrigger } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { NgClass } from '@angular/common';
import { CardDetails } from './card.types';

@Component({
    selector: 'shared-card',
    imports: [
        CardMenuItemComponent,
        MatCheckboxModule,
        MatTooltipModule,
        MatRippleModule,
        MatButtonModule,
        MatIconModule,
        MatMenuModule,
        NgClass
    ],
    templateUrl: 'card.component.html',
    styleUrl: 'card.component.css'
})
export class CardComponent {
    private readonly dialog = inject(MatDialog);

    public readonly key = input.required<string>();
    public readonly title = input.required<string>();
    public readonly isHandset = input<boolean|undefined>(true);
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
    public readonly summary = input<string|null>(null);
    public readonly description = input<string|null>(null);
    public readonly tags = input<string[]|null>(null);

    public readonly isHolding = signal<boolean>(false);

    private readonly menuTrigger = viewChild.required(MatMenuTrigger);
    private openKebabMenu() {
        this.menuTrigger().openMenu();
    }

    private readonly touchTimeout = signal<NodeJS.Timeout|null>(null);
    private readonly ensureIsHoldingFlips = effect(() => {
        if (this.touchTimeout() === null) {
            this.isHolding.set(false);
        }
    });

    /**
     * Get some of the internal properties of this card as an object instance.
     */
    public readonly cardDetails = computed<CardDetails>(() => {
        let tags = this.tags();
        if (!Array.isArray(tags) || tags.length <= 0) {
            tags = null;
        }

        return {
            key: this.key(),
            title: this.title(),
            summary: this.summary() || null,
            description: this.description() || null,
            tags: tags
        };
    });
    public readonly hasDetails = computed<boolean>(() => {
        const details = this.cardDetails();
        return !!(
            details.summary || 
            details.description || (
                Array.isArray(details.tags) && details.tags.length >= 0
            )
        );
    });

    /**
     * Emits when `{touchStart}`
     */
    public readonly onTouchStart = output<CardDetails>();
    /**
     * Callback firing when this card starts to get touched
     */
    public readonly touchStart = (event?: Event): void => {
        /* if (event) {
            if ('preventDefault' in event) {
                event.preventDefault();
            }
        } */

        if (this.touchTimeout() === null) {
            this.touchTimeout.set(
                setTimeout(this.held, 500)
            );
        }

        this.onTouchStart.emit(
            this.cardDetails()
        );
    }

    /**
     * Emits when `{touchEnd}`
     */
    public readonly onTouchEnd = output<CardDetails>();
    /**
     * Callback firing when this card siezes to be touched
     */
    public readonly touchEnd = (event?: Event): void => {
        /* if (event) {
            if ('preventDefault' in event) {
                event.preventDefault();
            }
        } */

        const timeout = this.touchTimeout();
        if (timeout !== null) {
            clearTimeout(timeout);
            this.touchTimeout.set(null);
        }

        this.onTouchEnd.emit(
            this.cardDetails()
        );
    }

    /**
     * Emits when `{held}`
     */
    public readonly onHeld = output<CardDetails>();
    /**
     * Callback firing when this card gets held
     */
    public readonly held = (event?: Event): void => {
        /* if (event) {
            if ('preventDefault' in event) {
                event.preventDefault();
            }
        } */
        
        this.isHolding.set(true);

        if ('vibrate' in navigator) {
            navigator.vibrate(30);
        }

        if (!this.isSelected()) {
            this.selected();
        }
        else {
            this.openKebabMenu();
            this.touchEnd();
        }

        this.onHeld.emit(
            this.cardDetails()
        );
    }

    /**
     * Emits when `{clicked}`
     */
    public readonly onClick = output<CardDetails>();
    /**
     * Callback firing when this card gets clicked
     */
    public readonly clicked = (event?: Event): void => {
        if (this.isHolding() === true) {
            return;
        }

        let specialClicked = false;
        if (event) {
            /* if ('preventDefault' in event) {
                event.preventDefault();
            } */

            specialClicked = (
                ('ctrlKey' in event && event.ctrlKey === true) ||
                ('shiftKey' in event && event.shiftKey === true) ||
                ('controlKey' in event && event.controlKey === true)
            );
        }

        if (specialClicked && this.isSelected()) {
            this.openKebabMenu();
        }
        else if (this.selectionState().selectModeActive || specialClicked) {
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
     * Emits when `{itemSummary}` is triggered
     */
    public readonly showItemSummary = output<CardDetails>();
    /**
     * Callback firing when this card gets copied.
     */
    public readonly itemSummary = (): void => {
        const state = this.selectionState();
        if (!state) {
            return;
        }

        const cardDetails = this.cardDetails();
        this.showItemSummary.emit(cardDetails);

        this.dialog.open(CardSummaryDialogComponent, {
            data: cardDetails
        });
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
