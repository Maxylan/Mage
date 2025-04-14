import { Component, computed, effect, inject, input, output, signal, untracked } from '@angular/core';
import { NavbarControllerService } from '../../../layout/navbar/navbar-controller.service';
import { PhotoTagsInputComponent } from './tags/photo-tags-input.component';
import { IPhotoQueryParameters } from '../../../core/types/photos.types';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatIconButton } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { SelectionObserver } from '../selection-observer.component';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { MatInput } from '@angular/material/input';
import { MatChip } from '@angular/material/chips';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { last, map } from 'rxjs';

@Component({
    selector: 'photos-toolbar',
    imports: [
        PhotoTagsInputComponent,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatToolbarModule,
        MatButtonModule,
        MatIconModule,
        MatIconButton,
        MatInput,
        MatChip
    ],
    templateUrl: 'photos-toolbar.component.html',
    styleUrl: 'photos-toolbar.component.scss'
})
export class PhotoToolbarComponent {
    private readonly navbarController = inject(NavbarControllerService);
    private readonly selectionObserver = inject(SelectionObserver);
    private readonly urlEncoder = inject(HttpUrlEncodingCodec);
    private readonly route = inject(ActivatedRoute);

    public readonly searchOffset = input<number>(0, { alias: 'offset' });
    public readonly searchLimit = input<number>(32, { alias: 'limit' });

    public readonly selectionState = this.selectionObserver.State;
    public readonly quitSelectMode = () => setTimeout(
        () => this.selectionObserver.setSelectionMode(false),
        64
    );

    public readonly getNavbar = this.navbarController.getNavbar;
    public readonly searchControl = new FormControl<string>('');
    public readonly tags = signal<string[]>([]);

    private readonly lastParameters = signal<IPhotoQueryParameters|undefined>(undefined);

    /**
     * Parse the `ParamMap` URL/Query Parameters observable into a supported
     * `IPhotoQueryParameters` collection.
     */
    public readonly queryParameters = toSignal<IPhotoQueryParameters>(
        this.route.queryParamMap.pipe(
            map(params => {
                let query: IPhotoQueryParameters = {
                    search: this.searchControl.value?.trim()?.normalize() || '',
                    offset: this.searchOffset(),
                    limit: this.searchLimit(),
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
        )
    );

    /**
     * Compute Query Parameters into a supported `IPhotoQueryParameters` collection.
     * Also computes Parameters into URL Query parameters.
     */
    private readonly computeSearchParameters = (): IPhotoQueryParameters => {
        let parameters: IPhotoQueryParameters = {
            ...this.queryParameters(),
            offset: this.searchOffset(),
            limit: this.searchLimit(),
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
            value = this.urlEncoder.encodeValue(value);
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

        if (untracked(this.lastParameters) === parameters) {
            return;
        }

        this.lastParameters.set(parameters);
        this.searchEvent.emit(parameters);
    };

    /**
     * Callback invoked when a search-query is triggered.
     */
    public readonly searchEvent = output<IPhotoQueryParameters>({ alias: 'onSearch' });

    /**
     * Initial search..
     */
    private readonly initialSearch = effect(this.triggerSearch);
}
