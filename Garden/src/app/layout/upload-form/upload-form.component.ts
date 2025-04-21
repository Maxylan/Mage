import { HttpClient } from "@angular/common/http";
import { Component } from "@angular/core";

@Component({
	selector: "upload-form",
	templateUrl: 'upload-form.component.html',
	styleUrl: 'upload-form.component.css'
})
export class UploadFormComponent {
	constructor(private http: HttpClient) { }

	onSubmit(ev: Event): any {
		if (!ev) {
			return;
		}

		ev.preventDefault();

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
