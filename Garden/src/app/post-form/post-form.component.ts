import { HttpClient } from "@angular/common/http";
import { Component } from "@angular/core";

@Component({
	selector: "post-form-component",
	template: `
		<p>post-form works!</p>
		<form
			id="uploadForm"
			(submit)="onSubmit($event)"
		>
			<dl>
				<dt>
					<label for="note">Note</label>
				</dt>
				<dd>
					<input id="note" type="text" name="note" />
				</dd>
				<dt>
					<label for="file">File</label>
				</dt>
				<dd>
					<input id="file" type="file" name="file" />
				</dd>
			</dl>

			<input class="btn" type="submit" value="Upload" />

			<div style="margin-top:15px">
				<output form="uploadForm" name="result"></output>
			</div>
		</form>
	`,
	styles: ``
})
export class PostFormComponent {
	constructor(private http: HttpClient) { }

	onSubmit(ev: Event): any {
		console.log('onSubmit fired.', ev);
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
				.post<FormData>('/reception/photos/stream', formData, {
					headers: {
						"x-mage-token": "820be47b-0def-42d7-be2a-92919b8f371e"
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
