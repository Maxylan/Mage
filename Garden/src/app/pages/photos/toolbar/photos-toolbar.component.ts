import { Component, WritableSignal, computed, effect, inject, input, model, signal } from '@angular/core';
import { NavbarControllerService } from '../../../layout/navbar/navbar-controller.service';
import { SearchBarComponent } from '../../../shared/blocks/search-bar/search-bar.component';
import { defaultPhotoPageContainer, IPhotoQueryParameters, PhotoPageStore, SearchQueryParameters } from '../../../core/types/photos.types';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatIconButton } from '@angular/material/button';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatToolbarModule } from '@angular/material/toolbar';
import { toObservable } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { Observable } from 'rxjs';

@Component({
    selector: 'photos-toolbar',
    imports: [
        ReactiveFormsModule,
        SearchBarComponent,
        MatFormFieldModule,
        MatFormFieldModule,
        MatToolbarModule,
        MatButtonModule,
        MatChipsModule,
        MatIconButton,
        MatIconModule,
        FormsModule,
        MatInput
    ],
    templateUrl: 'photos-toolbar.component.html',
    styleUrl: 'photos-toolbar.component.scss'
})
export class PhotoToolbarComponent {
    private readonly navbarController = inject(NavbarControllerService);
    public readonly getNavbar = this.navbarController.getNavbar;

    public readonly initial = input.required<IPhotoQueryParameters>();
    public readonly photos = model<PhotoPageStore>(defaultPhotoPageContainer);

    public readonly tags: WritableSignal<string[]> = signal(this.initial().tags ?? []);
    public readonly tagsControl = new FormControl<string>('');

    public readonly isLoading = signal<boolean>(true);
    
    public ngOnInit() {
        this.onSearch(this.initial);
    }

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

    /**
     * Callback invoked when a search-query is triggered.
     */
    public onSearch = (query: SearchQueryParameters) => {
    }
}
