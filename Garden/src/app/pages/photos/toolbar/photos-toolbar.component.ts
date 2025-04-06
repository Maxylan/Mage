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
import { ActivatedRoute, ParamMap } from '@angular/router';
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
    private readonly route = inject(ActivatedRoute);

    @Input()
    public readonly offset: IPhotoQueryParameters['offset'] = 0;

    @Input()
    public readonly limit: IPhotoQueryParameters['limit'] = 32;

    @Input()
    public selectionState?: Signal<SelectState>;

    @Input()
    public setSelectionMode?: SelectionObserver['setSelectionMode'];

    public readonly getNavbar = this.navbarController.getNavbar;
    public readonly isLoading: WritableSignal<boolean> = signal(true);
    public readonly photoStore: WritableSignal<PhotoPageStore> = signal(defaultPhotoPageContainer);
    
    public readonly queryParameters$ = this.route.queryParamMap;
    public readonly photoQuery: WritableSignal<IPhotoQueryParameters> = signal({
        offset: this.offset,
        limit: this.limit
    });

    public readonly tagsControl = new FormControl<string>('');

    /**
     * Parse incomming `ParamMap` URL/Query Parameters into a supported `IPhotoQueryParameters` collection.
     */
    public ngOnInit() {
        this.queryParameters$.subscribe(
            params => {
                console.debug('Query params updating..');
                const query = {
                    search: params.get('search') || undefined,
                    summary: params.get('summary') || undefined,
                    title: params.get('title') || undefined,
                    slug: params.get('slug') || undefined,
                    tags: params.getAll('t') || undefined,
                    offset: this.offset,
                    limit: this.limit
                };
                console.debug('Query params:', query);
                this.photoQuery.set(query);
                /* this.photoQuery.set({
                    search: params.get('search') || undefined,
                    summary: params.get('summary') || undefined,
                    title: params.get('title') || undefined,
                    slug: params.get('slug') || undefined,
                    tags: params.getAll('t') || undefined,
                    offset: this.offset,
                    limit: this.limit
                }); */
            }
        );
    }

    /**
     * Effect that triggers every time tags are mutated..
     * Keeps the Query Parameters in the URL up-to-date..
     */
    public readonly updateTagParameters = (tags: string[]) => {
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
    }
    
    /**
     * Callback triggered by pressing the (X) to remove a tag..
     */
    public readonly removeTag = (keyword: string): void => {
        let tags = this.photoQuery().tags;
        if (!Array.isArray(tags) || !tags.length) {
            return;
        }

        const index = tags.indexOf(keyword);
        if (index < 0) {
            return;
        }

        tags.splice(index, 1);
        this.updateTagParameters(tags);
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
            let tags = this.photoQuery().tags;
            if (!Array.isArray(tags)) {
                tags = [];
            }

            tags.push(value);
        }

        event.chipInput!.clear();
    }

    /**
     * Callback invoked when a search-query is triggered.
     * Performs the GET-Request to search for photos.
     */
    public searchForPhotos = (searchQuery: SearchQueryParameters) => {
        console.debug('searchForPhotos searchQuery', {...searchQuery}, {...this.photoQuery()});
        if (!searchQuery || !Object.keys(searchQuery).length) {
            console.warn('Skipping an empty search query!', searchQuery);

            if (this.isLoading()) {
                this.isLoading.set(false);
            }

            return;
        }

        if (this.isLoading() === false) {
            this.isLoading.set(true);
        }

        const {
            currentPage,
            pageSize
        } = this.photoStore();

        const limit = currentPage > 0 ? pageSize * 3 : pageSize * 2;
        const offset = currentPage > 1 ? limit * currentPage - pageSize : 0;
        const photoQuery = {
            ...this.photoQuery(),
            offset: offset,
            limit: limit
        }

        return this.photoService
            .getPhotos(photoQuery)
            .then(data => {
                console.debug('[searchForPhotos] Search Result', { ...data });
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

    @Output()
    public photos$: Observable<PhotoPageStore> = toObservable(this.photoStore);
}
