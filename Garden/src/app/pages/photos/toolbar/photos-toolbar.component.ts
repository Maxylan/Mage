import { Component, Input, Output, Signal, WritableSignal, computed, effect, inject, signal } from '@angular/core';
import { NavbarControllerService } from '../../../layout/navbar/navbar-controller.service';
import { SelectionObserver, SelectState } from './selection-observer.component';
import { defaultPhotoPageContainer, IPhotoQueryParameters, PhotoPageStore } from '../../../core/types/photos.types';
import { PhotosService } from '../../../core/api/photos.service';
import { MatToolbarModule } from '@angular/material/toolbar';
import { toObservable } from '@angular/core/rxjs-interop';
import { MatButtonModule, MatIconButton } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { SearchBarComponent, SearchQueryParameters } from '../../../shared/blocks/search-bar/search-bar.component';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { ParamMap } from '@angular/router';
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
    providers: [
        PhotosService,
        HttpUrlEncodingCodec
    ],
    templateUrl: 'photos-toolbar.component.html',
    styleUrl: 'photos-toolbar.component.scss'
})
export class PhotoToolbarComponent {
    private readonly navbarController = inject(NavbarControllerService);
    private readonly urlEncoder = inject(HttpUrlEncodingCodec);
    private readonly photoService = inject(PhotosService);

    @Input({ required: true })
    public queryParameters: ParamMap|null = null;

    public getNavbar = this.navbarController.getNavbar;
    public isLoading: WritableSignal<boolean> = signal(false);
    public photoStore: WritableSignal<PhotoPageStore> = signal(defaultPhotoPageContainer);
    
    public tagsControl = new FormControl<string>('');

    /**
     * Photo-tags signal..
     */
    public readonly photoTags = signal<string[]>((
        // Calculates *Initial* `photoTags` state..
        // TODO - Use injected route instead of using `location.href.split(..)`?
        () => {
            const [_, baseQuery] = location.href.split('?');
            if (!baseQuery) {
                return [];
            }
            return baseQuery
                .split('&')
                .filter(param => {
                    let tag = param.trim();
                    return !!(
                        tag
                        && tag.length > 2
                        && tag.startsWith('t=')
                    );
                })
                .map(param => {
                    let tag = param.trim().substring(2)
                    return this.urlEncoder.decodeValue(tag);
                });
        }
    )());

    /**
     * Effect that triggers every time `this.photoTags()` gets updated..
     * Keeps the Query Parameters in the URL up-to-date..
     */
    public readonly onPhotoTags = effect(() => {
        const tags = this.photoTags();
        const [
            baseUrl,
            baseQuery
        ] = location.href.split('?');
        let parameters = '?';

        if (baseQuery) {
            parameters += baseQuery
                .split('&')
                .filter(_ => !_.startsWith('t='))
                .join('&');
        }

        let sanitizedTags = tags
            .map(unsanitizedTag => {
                let tag = unsanitizedTag?.normalize()?.trim();
                if (!tag) {
                    return '';
                }

                return this.urlEncoder.encodeValue(tag);
            })
            .filter(tag => !!tag);

        if (!tags.length) {
            if (parameters === '?') {
                parameters = '';
            }
            
            window.history.replaceState(null, '', new URL(parameters, baseUrl));
            return;
        }

        parameters = parameters.length > 1 ? parameters + '&' : '?';
        parameters += `t=${sanitizedTags.join('&t=')}`;

        window.history.replaceState(null, '', new URL(parameters, baseUrl));
    });
    
    /**
     * Callback triggered by pressing the (X) to remove a tag..
     */
    public readonly removeTag = (keyword: string): void => {
        this.photoTags.update(tags => {
            const index = tags.indexOf(keyword);
            if (index < 0) {
                return tags;
            }

            tags.splice(index, 1);
            return [...tags];
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
            this.photoTags.update(
                keywords => [...keywords, value]
            );
        }

        event.chipInput!.clear();
    }

    /**
     * Effect that triggers every time `this.searchForPhotos(..)` gets invoked..
     * Keeps the Query Parameters in the URL up-to-date..
     */
    public readonly updatePageParameters = (searchQuery: SearchQueryParameters): void => {
        const [
            baseUrl,
            _baseQuery
        ] = location.href.split('?');

        const sanitized = Array.from(Object.entries(searchQuery))
            .flatMap(unsanitized => {
                let key = unsanitized[0]?.normalize()?.trim();
                const skipKeyIf = [
                    'limit',
                    'offset'
                ];

                if (!key || skipKeyIf.includes(key)) {
                    return null;
                }

                let values: any[];
                if (Array.isArray(unsanitized[1])) {
                    values = unsanitized[1]
                        .filter(value => !!value);

                    if (!values.length) {
                        return null;
                    }
                }
                else {
                    values = [unsanitized[1]];

                    if (!values[0]) {
                        return null;
                    }
                }

                return values.map(value => {
                    if (typeof value === 'string') {
                        value = value.normalize().trim();
                    }
                
                    if (!value) {
                        return null;
                    }

                    return [
                        this.urlEncoder.encodeKey(key),
                        this.urlEncoder.encodeValue(value)
                    ];
                });
            })
            .filter(kvp => !!kvp)    
            .map(kvp => `${kvp[0]}=${kvp[1]}`);

        if (!sanitized.length) {
            window.history.replaceState(null, '', baseUrl);
            return;
        }

        const parameters = `?${sanitized.join('&')}`;
        // console.debug('search parameters', new URL(parameters, baseUrl));
        window.history.replaceState(null, '', new URL(parameters, baseUrl));
    }


    @Input()
    public selectionState?: Signal<SelectState>;

    @Input()
    public setSelectionMode?: SelectionObserver['setSelectionMode'];

    @Output()
    public photos$: Observable<PhotoPageStore> = toObservable(this.photoStore);

    /**
     * Callback invoked when a search-query is triggered.
     * Performs the GET-Request to search for photos.
     */
    public searchForPhotos = (searchQuery: SearchQueryParameters) => {
        this.isLoading.set(true);
        const {
            currentPage,
            pageSize
        } = this.photoStore();

        const fetchLimit = currentPage > 0 ? pageSize * 3 : pageSize * 2;
        const fetchOffset = currentPage > 1 ? fetchLimit * currentPage - pageSize : 0;

        if (!searchQuery || !Object.keys(searchQuery).length) {
            console.warn('Skipping an empty search query!', searchQuery);
            return;
        }

        searchQuery = {
            ...searchQuery,
            t: this.photoTags()
        };

        this.updatePageParameters(searchQuery);

        const queryParameters = this.photoService
            .parseSearchQueryParameters(searchQuery, {
                offset: fetchOffset,
                limit: fetchLimit
            });

        return this.photoService
            .getPhotos(queryParameters)
            .then(data => {
                console.debug('[searchForPhotos] Search Result', data);
                let newStore: PhotoPageStore = {
                    ...this.photoStore()
                };

                let iteration = 0;
                while(data.length > 0 && ++iteration < 3) {
                    const sliceLength = data.length < pageSize
                        ? data.length
                        : pageSize;

                    const slice = new Set(data.splice(0, sliceLength));

                    const pageNumber = currentPage && currentPage - iteration;
                    const pageIndex = newStore.store.findIndex(p => p.page === pageNumber);
                    if (pageIndex === -1) {
                        newStore.store.push({
                            page: pageNumber,
                            set: slice
                        });
                    }
                    else {
                        newStore.store[pageIndex] = {
                            page: pageNumber,
                            set: slice
                        };
                    }
                }
                
                console.debug(newStore);
                this.photoStore.set(newStore);
            })
            .catch(err => {
                console.error('[searchForPhotos] Error!', err);
                /* this.photoStore.set({
                    ...this.photoStore(),
                    store: []
                }); */
            })
            .finally(() => this.isLoading.set(false));
    }
}
