import { Component, input, computed, effect, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
    MatDialogContent,
    MatDialogActions,
    MatDialogTitle,
    MatDialogRef,
    MAT_DIALOG_DATA
} from '@angular/material/dialog';
import { MatProgressBar } from '@angular/material/progress-bar';
import { CardDetails } from '../card.types';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';

@Component({
    selector: 'shared-card-summary',
    imports: [
        MatDividerModule,
        MatDialogContent,
        MatDialogActions,
        MatButtonModule,
        MatDialogTitle,
        MatChipsModule,
        MatProgressBar
    ],
    templateUrl: 'card-summary.component.html',
    styleUrl: 'card-summary.component.css'
})
export class CardSummaryDialogComponent {
    private readonly dialogRef = inject(MatDialogRef<CardSummaryDialogComponent>);
    private readonly details = inject<CardDetails>(MAT_DIALOG_DATA);

    public readonly base64 = input<string|null>(null);
    public alt = input<string>();

    public readonly imageIsLoading = computed<boolean>(
        () => !!this.base64()?.length
    );

    public readonly cardDetails = computed<CardDetails>(
        () => this.details
    );

    public onNoClick(): void {
        this.dialogRef.close();
    }
}
