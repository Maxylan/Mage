import { Component, ElementRef, inject, output, signal, viewChild } from '@angular/core';
import { UploadFormComponent } from "./upload-form.component";
import { NgClass } from '@angular/common';
import {
    MatDialogContent,
    MatDialogActions,
    MatDialogTitle,
    MatDialogRef,
    MAT_DIALOG_DATA,
    MatDialogState
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';

export type UploadDialogRef = MatDialogRef<UploadDialogComponent, DragEvent>|null;

@Component({
    selector: 'upload-dialog',
    imports: [
        UploadFormComponent,
        MatDividerModule,
        MatDialogContent,
        MatDialogActions,
        MatButtonModule,
        MatDialogTitle,
        MatChipsModule,
        MatIconModule,
        NgClass
    ],
    templateUrl: 'upload-dialog.component.html',
    styleUrl: 'upload-dialog.component.css'
})
export class UploadDialogComponent {
    private readonly dialogRef = inject(MatDialogRef<UploadDialogComponent, DragEvent>);
    private readonly eventData = inject<DragEvent>(MAT_DIALOG_DATA);

    public readonly hovering = signal<boolean>(false);
    public readonly uploadedFiles = signal<FileList|null>(null);

    public readonly inputRef = viewChild<ElementRef<HTMLInputElement>>('fileInput');

    /**
     * Emits on `{change}` (hidden file input)
     */
    public readonly onFileInputChange = output<Event>({ alias: 'filesChange' });
    /**
     * Callback firing on `dragEnter`. (hidden file input)
     */
    public readonly fileInputChange = (e: Event): void => {
        if (!e) {
            return;
        }

        if ('preventDefault' in e) {
            e.preventDefault();
        }
        if ('stopPropagation' in e) {
            e.stopPropagation();
        }

        console.log('UploadDialogComponent fileInputChange', e);
        const inputElement = e.target as HTMLInputElement|null;
        if (inputElement &&
            inputElement.files &&
            inputElement.files.length > 0
        ) {
            console.log('inputElement.files', inputElement.files);
            this.uploadedFiles.set(inputElement.files);
            this.onFileInputChange.emit(e);
        }
        else {
            this.uploadedFiles.set(null);
        }
    }

    /**
     * Callback firing on `click`.
     */
    public readonly onClick = (e: MouseEvent): void => {
        if (!e || this.uploadedFiles()) {
            return;
        }

        /* if (this.dialogRef !== null &&
            this.dialogRef.getState() === MatDialogState.OPEN
        ) {
            return;
        } */

        if ('preventDefault' in e) {
            e.preventDefault();
        }
        if ('stopPropagation' in e) {
            e.stopPropagation();
        }

        console.log('UploadDialogComponent onClick', e);
        const ref = this.inputRef();
        if (ref && ref.nativeElement) {
            ref.nativeElement.click();
        }
    }

    /**
     * Emits on `{dragOver}`
     */
    public readonly onDragOver = output<DragEvent>();
    /**
     * Callback firing on `dragOver`.
     */
    public readonly dragOver = (e: DragEvent): void => {
        if (!e) {
            return;
        }

        if ('preventDefault' in e) {
            console.log('over preventDefault FIRED');
            e.preventDefault();
        }
        else {
            console.log('over preventDefault NOT FIRED');
        }
        if ('stopPropagation' in e) {
            console.log('over stopPropagation FIRED');
            e.stopPropagation();
        }
        else {
            console.log('over stopPropagation NOT FIRED');
        }

        console.log('UploadDialogComponent dragOver', e);
        this.onDragOver.emit(e);
    }

    /**
     * Emits on `{dragEnter}`
     */
    public readonly onDragEnter = output<DragEvent>();
    /**
     * Callback firing on `dragEnter`.
     */
    public readonly dragEnter = (e: DragEvent): void => {
        if (!e || !('relatedTarget' in e) || this.hovering()) {
            return;
        }

        /* if (this.dialogRef !== null &&
            this.dialogRef.getState() === MatDialogState.OPEN
        ) {
            return;
        } */

        if ('preventDefault' in e) {
            console.log('enter preventDefault FIRED');
            e.preventDefault();
        }
        else {
            console.log('enter preventDefault NOT FIRED');
        }
        if ('stopPropagation' in e) {
            console.log('enter stopPropagation FIRED');
            e.stopPropagation();
        }
        else {
            console.log('enter stopPropagation NOT FIRED');
        }

        console.log('UploadDialogComponent dragEnter', e);
        this.onDragEnter.emit(e);
        this.hovering.set(true);
    }

    /**
     * Emits on `{dragEnd}`
     */
    public readonly onDragEnd = output<DragEvent>();
    /**
     * Callback firing on `dragEnd`.
     */
    public readonly dragEnd = (e: DragEvent): void => {
        if (!e || !('relatedTarget' in e)) {
            return;
        }

        if ('preventDefault' in e) {
            console.log('end preventDefault FIRED');
            e.preventDefault();
        }
        else {
            console.log('end preventDefault NOT FIRED');
        }
        if ('stopPropagation' in e) {
            console.log('end stopPropagation FIRED');
            e.stopPropagation();
        }
        else {
            console.log('end stopPropagation NOT FIRED');
        }

        console.log('UploadDialogComponent dragEnd', e);
        this.onDragEnd.emit(e);
        this.hovering.set(false);
    }

    /**
     * Emits on `{dragLeave}`
     */
    public readonly onDragLeave = output<DragEvent>();
    /**
     * Callback firing on `dragEnter`.
     */
    public readonly dragLeave = (e: DragEvent): void => {
        if (!e || !('relatedTarget' in e)) {
            return;
        }

        if ('preventDefault' in e) {
            console.log('leave preventDefault FIRED');
            e.preventDefault();
        }
        else {
            console.log('leave preventDefault NOT FIRED');
        }
        if ('stopPropagation' in e) {
            console.log('leave stopPropagation FIRED');
            e.stopPropagation();
        }
        else {
            console.log('leave stopPropagation NOT FIRED');
        }

        console.log('UploadDialogComponent dragLeave', e);
        this.onDragLeave.emit(e);
        this.hovering.set(false);
    }

    /**
     * Emits on `{drop}`
     */
    public readonly onDrop = output<DragEvent>();
    /**
     * Callback firing on `drop`.
     */
    public readonly drop = (e: DragEvent): void => {
        if (!e) {
            return;
        }

        if ('preventDefault' in e) {
            console.log('preventDefault FIRED');
            e.preventDefault();
        }
        else {
            console.log('preventDefault NOT FIRED');
        }
        if ('stopPropagation' in e) {
            console.log('stopPropagation FIRED');
            e.stopPropagation();
        }
        else {
            console.log('stopPropagation NOT FIRED');
        }

        console.log('UploadDialogComponent drop', e);
        if ('dataTransfer' in e && e.dataTransfer) {
            this.uploadedFiles.set(e.dataTransfer.files);
        }
        else {
            this.uploadedFiles.set(null);
        }

        this.onDrop.emit(e);
        this.hovering.set(false);
    }
}
