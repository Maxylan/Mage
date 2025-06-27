import { Component, inject, model } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { ReactiveFormsModule } from '@angular/forms';
import { SelectionObserver } from '../toolbar/selection-observer.service';
import { UploadCardThumbnailComponent } from './thumbnail.component';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';

@Component({
    selector: 'upload-form',
    imports: [
        UploadCardThumbnailComponent,
        ReactiveFormsModule,
        MatButtonModule,
        MatChipsModule
    ],
    providers: [
        SelectionObserver
    ],
    templateUrl: 'upload-form.component.html',
    styleUrl: 'upload-form.component.css'
})
export class UploadFormComponent {
    private readonly http = inject(HttpClient);
    private readonly selectionObserver = inject(SelectionObserver);

    public readonly selectionState = this.selectionObserver.State;
    public readonly deselect = this.selectionObserver.deselectItems;
    public readonly select = this.selectionObserver.selectItems;

    public readonly files = model<FileList|null>(null);

	public onSubmit(ev: Event): void {
		if (!ev) {
			return;
		}
        else if ('preventDefault' in ev) {
		    ev.preventDefault();
        }

		/* method="post"
		enctype="multipart/form-data" */

		const formElement = /* el */ ev.target as HTMLFormElement;
		const formData = new FormData(formElement);
		console.debug('onSubmit', formElement, ev, formData);

		try {
			this.http
				.post<FormData>('/reception/photos/upload', formData, {
					headers: {
						"x-mage-token": "2c1b1a9f-9c02-4a73-bf37-d61899f8a36b"
					}
				})
				.subscribe((value) => {
					(formElement.elements.namedItem("result") as HTMLElement).innerHTML = "Result: " + JSON.stringify(value, null, 4);
				});
		} catch (error) {
			console.error("Error:", error);
		}
	}
}
