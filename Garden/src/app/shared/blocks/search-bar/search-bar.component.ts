import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { SearchParameters } from './search-bar.types';

@Component({
    selector: 'shared-search-bar',
    imports: [
        ReactiveFormsModule,
        MatFormFieldModule,
        MatButtonModule,
        MatIconModule,
        MatInput
    ],
    templateUrl: 'search-bar.component.html',
    styleUrl: 'search-bar.component.scss',
    /* host: {
        style: "display: inline-block; margin: 0px auto;"
    } */
})
export class SearchBarComponent {
    @Input({ required: true })
    public formName!: string;

    @Input()
    public initialSearch: string|false = false;

    @Input()
    public placeholder?: string = 'Search for ' + this.formName;

    @Output()
    public onSearch$: EventEmitter<SearchParameters> = new EventEmitter<SearchParameters>();

    public searchControl = new FormControl<string>(this.initialSearch || '');
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
    
    public ngOnInit() {
        if (this.initialSearch) {
            this.parseSearchForm();
        }
    }
}
