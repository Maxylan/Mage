import { Component, effect, inject, input, model, output, signal } from '@angular/core';
import { PhotoTagsInputComponent } from './tags/photo-tags-input.component';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatIconButton } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { MatInput } from '@angular/material/input';
import { NgClass } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { last, map } from 'rxjs';
import { SearchPhotosParameters } from '../../../core/types/search-photos-parameters';

@Component({
    selector: 'photos-searchbar',
    imports: [
        PhotoTagsInputComponent,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatButtonModule,
        MatIconModule,
        MatIconButton,
        MatInput,
        NgClass
    ],
    providers: [
        HttpUrlEncodingCodec
    ],
    templateUrl: 'photos-searchbar.component.html',
    styleUrl: 'photos-searchbar.component.scss'
})
export class PhotoSearchbarComponent {
    private readonly urlEncoder = inject(HttpUrlEncodingCodec);
    private readonly route = inject(ActivatedRoute);

    public readonly initialSearch = input<boolean>(true);
    public readonly searchParameters =
        model.required<SearchPhotosParameters>({ alias: 'parameters' });

    public readonly searchControl = new FormControl<string>('');
    public readonly tags = signal<string[]>([]);

    public readonly expandSearchForm = signal<boolean>(false);

    /**
     * Parse the `ParamMap` URL/Query Parameters observable into a supported
     * `IPhotoQueryParameters` collection.
     */
    public readonly queryParameters$ = 
        this.route.queryParamMap.pipe(
            map(params => {
                let query: SearchPhotosParameters = {
                    ...this.searchParameters(),
                    search: this.searchControl.value?.trim()?.normalize() || '',
                    tags: this.tags()
                };

                if (params.get('search')) {
                    let searchParameter = this.urlEncoder.encodeValue(
                        params.get('search')?.trim()?.normalize() || ''
                    );

                    if (query.search !== searchParameter) {
                        this.searchControl.setValue(searchParameter);
                    }
                }

                /* let tags = this.tags().map(
                    tag => this.urlEncoder.encodeValue(
                        tag?.trim()?.normalize()
                    )
                );

                if (tags.length) {
                    query.tags = tags;
                } */

                if (params.has('slug')) {
                    query.slug = this.urlEncoder.encodeValue(params.get('slug') || '');
                }
                if (params.has('title')) {
                    query.title = this.urlEncoder.encodeValue(params.get('title') || '');
                }
                if (params.has('summary')) {
                    query.summary = this.urlEncoder.encodeValue(params.get('summary') || '');
                }
                if (params.has('tags')) {
                    let tagParameters = this.urlEncoder.decodeValue(params.get('tags') || '').split(',');
                    console.log('tags', tagParameters);

                    if (query.tags !== tagParameters) {
                        this.tags.set(tagParameters);
                    }
                }
                if (params.has('offset')) {
                    let offsetParam: string | number = params.get('offset') || 0;
                    if (typeof offsetParam === 'string') {
                        offsetParam = Number.parseInt(offsetParam);
                    }
                    if (Number.isNaN(offsetParam) || offsetParam < 0) {
                        throw new Error('Invalid "offset" param');
                    }
                    query.offset = offsetParam;
                }
                if (params.has('limit')) {
                    let limitParam: string | number = params.get('limit') || 0;
                    if (typeof limitParam === 'string') {
                        limitParam = Number.parseInt(limitParam);
                    }
                    if (Number.isNaN(limitParam) || limitParam < 0) {
                        throw new Error('Invalid "offset" param');
                    }
                    query.offset = limitParam;
                }

                return query;
            }),
            last()
        );

    /**
     * Compute Query Parameters into a supported `IPhotoQueryParameters` collection.
     * Also computes Parameters into URL Query parameters.
     */
    private readonly computeSearchParameters = (): SearchPhotosParameters => {
        let parameters: SearchPhotosParameters = {
            ...this.searchParameters(),
            tags: this.tags()
        };

        if (this.searchControl.value) {
            parameters.search = this.searchControl.value;
        }

        const queryParameters = new URL('?' + Object.entries(parameters).map(kvp => {
            if (!Array.isArray(kvp) || kvp.length < 2) {
                return null;
            }

            let [ key, value ] = kvp;
            key = this.urlEncoder.encodeKey(key);
            value = this.urlEncoder.encodeValue(value as string);
            if (!key || !value) {
                return null;
            }

            return `${key}=${value}`;
        }).filter(kvp => !!kvp).join('&'), window.location.href);
        
        window.history.replaceState(null, '', queryParameters);
        return parameters;
    }

    /**
     * Computes state into search-query parameters and emits `this.searchEvent`.
     */
    public readonly triggerSearch = () => {
        const parameters = this.computeSearchParameters();

        /* if (untracked(this.lastParameters) === parameters) {
            return;
        } */

        // this.lastParameters.set(parameters);
        this.searchEvent.emit(parameters);
    };

    private readonly triggerSearchOnEffect = effect(this.triggerSearch);

    /**
     * Output invoked when a search-query is triggered.
     */
    public readonly searchEvent = output<SearchPhotosParameters>({ alias: 'onSearch' });

    /**
     * Toggle `this.expandSearchForm`.
     */
    public readonly toggleExpand = () => 
        this.expandSearchForm.update(status => !status);
}
