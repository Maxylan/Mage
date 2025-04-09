import { Component, model, input } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';

@Component({
    selector: 'shared-tags-input',
    templateUrl: 'tags-input.component.html',
    /* host: {
        style: "display: inline-block; margin: 0px auto;"
    } */
    imports: [
        ReactiveFormsModule,
        MatFormFieldModule,
        MatButtonModule,
        MatChipsModule,
        MatIconModule,
        FormsModule,
        MatInput
    ],
})
export class TagsInputComponent {
    public readonly control = model.required<FormControl<string>>();
    public readonly tags = model<string[]>([]);

    /**
     * Callback triggered by pressing the (X) to remove a tag..
     */
    public readonly removeTag = (keyword: string): void => {
        this.tags.update(tags => {
            if (!Array.isArray(tags) || !tags.length) {
                return [];
            }

            const index = tags.indexOf(keyword);
            if (index > -1) {
                tags.splice(index, 1);
            }

            return tags;
        });
    }
    
    /**
     * Callback triggered when finished typing/creating a tag..
     */
    public readonly completeTag = (event: MatChipInputEvent): void => {
        if (!event.value) {
            event.chipInput?.clear();
            return; 
        }

        const value = event.value
            .normalize()
            .trim();

        if (value) {
            this.tags.update(tags => {
                if (!Array.isArray(tags)) {
                    tags = [];
                }

                const index = tags.indexOf(value);
                if (index === -1) {
                    tags.push(value);
                }

                return tags;
            });
        }

        event.chipInput!.clear();
    }
}
