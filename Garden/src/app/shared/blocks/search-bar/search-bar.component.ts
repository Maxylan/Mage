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
export class SearchBarComponent<TSupported extends object, TResult extends object> {
    @Input({required: true})
    public searchFormName!: string;

    @Input({required: true})
    // public callback!: SearchCallback<TSupported, TResult>;
    public callback!: SearchCallback;

    public searchControl = new FormControl<string>('');
    public searchForm = new FormGroup({
        keyword: this.searchControl,
    });

    // private searchResults: TResult[] = [];
    // public getSearchResults: TResult[] = this.searchResults;

    public parseSearchForm = () => {
        let parameters: SearchParameters<{[key: string]: string|number|undefined}> = {
            query: this.searchControl.value ?? undefined
        };

        this.callback(parameters as SearchParameters<TSupported>);
            /* .then(result => {
                this.searchResults = result;
                return this.results.emit(result);
            }); */
    };

    /* @Output()
    public results: EventEmitter<TResult[]> = new EventEmitter<TResult[]>(); */
}
