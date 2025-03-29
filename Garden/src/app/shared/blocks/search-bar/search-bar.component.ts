import { Component, EventEmitter, Input, Output, signal, ViewChild, WritableSignal } from '@angular/core';
import { FormControl, FormGroup, NgForm, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Params } from '@angular/router';
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
    @Input({ required: true })
    public formName!: string;

    @Input()
    public placeholder: string = 'Search for ' + this.formName;

    @Input()
    public initialValue: string = '';

    @Input()
    public searchOnQueryChange: boolean = false;

    @Output()
    public onSearch$: EventEmitter<SearchQueryParameters> = new EventEmitter<SearchQueryParameters>();

    @Output()
    public onQueryChange$: Observable<Params>;
    public queryParameters$: WritableSignal<Params> = signal({});

    public submitHandler = (event?: SubmitEvent) => {
        if (event) {
            event.preventDefault();
        }

        if (!this.searchControl.value) {
            return;
        }

        const params: SearchQueryParameters = {
            ...this.queryParameters$(),
            search: this.searchControl.value
        };

        this.onSearch$.emit(params);
    }

    public searchControl = new FormControl<string>(this.initialValue);
    public searchForm = new FormGroup({ search: this.searchControl });

    @ViewChild('f')
    private formRef!: NgForm;

    constructor(private route: ActivatedRoute) {
        this.onQueryChange$ = this.route.queryParams;
        this.onQueryChange$.subscribe(params => {
            this.queryParameters$.set(params);

            if (this.searchOnQueryChange) {
                this.formRef.ngSubmit.emit();
            }
        })
    }
}
