import { HttpClient } from "@angular/common/http";
import { Component } from "@angular/core";

@Component({
	selector: "post-form-component",
	template: `
		<p>post-form works!</p>
		<form
			id="uploadForm"
			action="Streaming/UploadDatabase"
			method="post"
			enctype="multipart/form-data"
			(submit)="onSubmit(ev)"
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
	//@ts-ignore
	public ev: SubmitEvent;

	onSubmit(ev: SubmitEvent): any {
		console.log('onSubmit fired.', ev);
		if (!ev) {
			return;
		}

		const formElement = /* el */ ev.target as HTMLFormElement;
		const formData = new FormData(formElement);
		console.debug('onSubmit', formElement, ev, formData);

		try {
			this.http
				.post<FormData>(formElement.action, formData, {
					headers: {
						"x-mage-token": "4db17ad8-35fd-4b93-a622-1aad1fdb73bc"
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
