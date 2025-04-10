import { Component, computed, EventEmitter, input, Input, model, output, Output, Signal, signal } from '@angular/core';
import { MatRipple } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { NgClass } from '@angular/common';
import {
    CardDetails,
    CardLinkDetails,
} from './card.types';
import { MatButtonModule } from '@angular/material/button';

@Component({
    selector: 'shared-card',
    imports: [
        NgClass,
        MatRipple,
        MatCheckboxModule,
        MatButtonModule,
        MatIconModule,
        MatMenuModule
    ],
    templateUrl: 'card.component.html',
    styleUrl: 'card.component.css'
})
export class CardComponent {
    public readonly key = input.required<string>();
    public readonly title = input.required<string>();
    public readonly isHandset = input.required<boolean|undefined>();

    public readonly summary = input<string>();
    public readonly isSelected = input<boolean>();
    public readonly isInSelectMode = input<boolean>();
    public readonly select = input<(isSelected: boolean) => void>();

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

        this.select()!(this.isSelected()!);

        this.onSelect.emit(
            this.cardDetails()
        );
    }
}
