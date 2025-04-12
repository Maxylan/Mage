import { Component, model } from '@angular/core';
import { COMMA, ENTER, SPACE } from '@angular/cdk/keycodes';
import {
    MAT_CHIPS_DEFAULT_OPTIONS,
    MatChipInputEvent,
    MatChipsModule
} from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'photo-tags-input',
    templateUrl: 'photo-tags-input.component.html',
    styleUrl: 'photo-tags-input.component.scss',
    imports: [
        MatFormFieldModule,
        MatButtonModule,
        MatChipsModule,
        MatIconModule
    ],
    providers: [
        {
            provide: MAT_CHIPS_DEFAULT_OPTIONS,
            useValue: {
                separatorKeyCodes: [COMMA, SPACE, ENTER]
            }
        }
    ],
})
export class PhotoTagsInputComponent {
    public readonly tags = model.required<string[]>();
    
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
