import { AsyncPipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormControl, FormControlOptions, FormGroup, FormSubmittedEvent, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormField, MatInput, MatLabel } from '@angular/material/input';
import { SearchCallback, SearchParameters } from './search-bar.types';

@Component({
    selector: 'shared-search-bar',
    imports: [
        ReactiveFormsModule,
        MatFormField,
        MatButtonModule,
        MatIconModule,
        MatInput,
        /* AsyncPipe, */
        /* MatLabel, */
    ],
    templateUrl: 'search-bar.component.html',
    styleUrl: 'search-bar.component.scss',
    host: {
        style: "display: inline-block; margin: 0px auto;"
    }
})
export class SearchBarComponent {
    @Input({ required: true })
    public searchFormName!: string;

    @Input()
    public placeholder?: string = 'Search for ' + this.searchFormName;

    @Output()
    public onSearch$: EventEmitter<SearchParameters> = new EventEmitter<SearchParameters>();

    public searchControl = new FormControl<string>('');
    public searchForm = new FormGroup({
        keyword: this.searchControl,
    });

    public parseSearchForm = () => {
        // TODO: Feels like there's loads missing here?
        let parameters: SearchParameters = {
            query: this.searchControl.value ?? undefined
        };

        this.onSearch$.emit(parameters);
    };
}
