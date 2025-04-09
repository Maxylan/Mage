import { Component, inject, input, model, output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatInput } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { HttpUrlEncodingCodec } from '@angular/common/http';

export type SearchEvent = {
    value: string,
    event?: SubmitEvent 
}

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

    public readonly formName = input.required<string>();
    public readonly control = model.required<FormControl<string>>();
    public readonly placeholder = input<string>('Search for ' + this.formName);
    public readonly initialValue = input<string>('');
    public readonly initialSearch = input<boolean>(true);
    public readonly onSearch = output<SearchEvent>();

    public readonly submitHandler = (event?: SubmitEvent): void => {
        if (event && 'preventDefault' in event) {
            event.preventDefault();
        }

        const url = new URL(location.href);
        let queryParameters = url.search.replace('?', '').split('&');
        let search = this.control().value;
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
        this.onSearch.emit({
            value: search,
            event
        });
    }
}
