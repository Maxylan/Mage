import { Component, inject, output } from '@angular/core';
import { UploadDialogComponent, UploadDialogRef } from './upload-dialog.component';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogRef, MatDialogState } from '@angular/material/dialog';

@Component({
    selector: '[uploadFormContainer]',
    imports: [
        MatButtonModule,
        MatChipsModule
    ],
    styles: '',
    template: '<ng-content />',
    /*
    styleUrl: 'upload-form.component.css',
    templateUrl: 'upload-form.component.html',
    */
    host: {
        '(dragenter)': 'dragEnter($event)',
        '(drop)': 'drop($event)'
    }
})
export class UploadFormContainerComponent {
    private readonly dialog = inject(MatDialog);
    private dialogRef: UploadDialogRef|null = null;

    /**
     * Emits on `{dragEnter}`
     */
    public readonly onDragEnter = output<DragEvent>();
    /**
     * Callback firing on `dragEnter`.
     */
    public readonly dragEnter = (e: DragEvent): void => {
        if (!e || !('relatedTarget' in e)) {
            return;
        }

        if (this.dialogRef !== null &&
            this.dialogRef.getState() === MatDialogState.OPEN
        ) {
            return;
        }

        if ('preventDefault' in e) {
            e.preventDefault();
        }
        if ('stopPropagation' in e) {
            e.stopPropagation();
        }

        this.onDragEnter.emit(e);
        console.log('dragEnter', e);

        this.dialogRef = this.dialog.open(UploadDialogComponent, {
            data: e
        });
    }

    /**
     * Emits on `{drop}`
     */
    public readonly onDrop = output<DragEvent>();
    /**
     * Callback firing on `drop`.
     */
    public readonly drop = (e: DragEvent): void => {
        if (!e || !('relatedTarget' in e)) {
            return;
        }

        if ('preventDefault' in e) {
            e.preventDefault();
        }
        if ('stopPropagation' in e) {
            e.stopPropagation();
        }

        console.log('drop', e);
        this.onDrop.emit(e);
    }
}
