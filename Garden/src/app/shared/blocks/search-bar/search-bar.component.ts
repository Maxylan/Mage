import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatInput } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Params } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';

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
    private readonly urlEncoder = inject(HttpUrlEncodingCodec);
    private readonly route = inject(ActivatedRoute);

    @Input({ required: true })
    public formName!: string;

    @Input()
    public placeholder: string = 'Search for ' + this.formName;

    @Input()
    public initialValue: string = '';

    @Input()
    public initialSearch: boolean = true;

    @Output()
    public onSearch$: EventEmitter<Params> = new EventEmitter<Params>();

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

        const url = new URL(location.href);
        let queryParameters = url.search.replace('?', '').split('&');
        let search = this.searchForm.controls.search.value;
        if (search && search.length < 1024) {
            search = 'search=' + this.urlEncoder.encodeValue(
                search.normalize().trim()
            );

            let index = queryParameters.indexOf(search);
            if (index === -1) {
                queryParameters.push(search);
            }
            else {
                queryParameters[index] = search;
            }
        }
        else {
            let index = queryParameters.findIndex(p => p.startsWith('search='));
            if (index !== -1) {
                queryParameters.splice(index, 1);
            }
        }
        
        window.history.replaceState(null, '', new URL(`?${queryParameters.join('&')}`, url));
    }

    public ngOnInit() {
        // Subscribe to query-parameter changes..
        this.onQueryChange$.subscribe(_ => {
            this.onSearch$.emit({
                ..._, search: this.searchForm.controls.search.value || ''
            })
        });
    }
}
