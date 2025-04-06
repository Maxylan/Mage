import { Component, EventEmitter, inject, Input, Output, Signal, signal, ViewChild, WritableSignal } from '@angular/core';
import { FormControl, FormGroup, NgForm, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Params } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';

export type SearchQueryParameters = Params & { search: string }; 

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
    private route: ActivatedRoute = inject(ActivatedRoute);

    @Input({ required: true })
    public formName!: string;

    @Input()
    public placeholder: string = 'Search for ' + this.formName;

    @Input()
    public initialValue: string = '';

    @Input()
    public initialSearch: boolean = true;

    @Input()
    public searchOnQueryChange: boolean = false;

    @Output()
    public onSearch$: EventEmitter<SearchQueryParameters> = new EventEmitter<SearchQueryParameters>();

    @Output()
    public onQueryChange$: Observable<Params> = this.route.queryParams;
    public readonly queryParameters$ = toSignal(this.onQueryChange$);

    public searchForm = new FormGroup({
        search: new FormControl<string>(this.queryParameters$()?.['search'] || this.initialValue)
    });

    public readonly submitHandler = (event?: SubmitEvent) => {
        if (event && 'preventDefault' in event) {
            event.preventDefault();
        }

        const params: SearchQueryParameters = {
            ...this.queryParameters$(),
            search: this.searchForm.controls.search.value || ''
        };

        this.onSearch$.emit(params);
    }

    public ngOnInit() {
        // Initial Search
        if(this.initialSearch) {
            this.submitHandler();
        }

        // Subscribe to query-parameter changes..
        this.onQueryChange$.subscribe(_ => {
            if (this.searchOnQueryChange) {
                this.submitHandler();
            }
        })
    }
}
