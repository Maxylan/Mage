import { Component, Input, Output, Signal, WritableSignal, computed, effect, inject, signal } from '@angular/core';
import { NavbarControllerService } from '../../../layout/navbar/navbar-controller.service';
import { SelectionObserver, SelectState } from './selection-observer.component';
import { defaultPhotoPageContainer, IPhotoQueryParameters, PhotoPageStore } from '../../../core/types/photos.types';
import { PhotosService } from '../../../core/api/photos.service';
import { MatToolbarModule } from '@angular/material/toolbar';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule, MatIconButton } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { SearchBarComponent, SearchQueryParameters } from '../../../shared/blocks/search-bar/search-bar.component';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { last, map, Observable } from 'rxjs';
import { AsyncPipe } from '@angular/common';

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
        AsyncPipe,
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
    public selectionState?: Signal<SelectState>;

    @Input()
    public setSelectionMode?: SelectionObserver['setSelectionMode'];

    public readonly tagsControl = new FormControl<string>('');
    public readonly getNavbar = this.navbarController.getNavbar;
    public readonly isLoading: WritableSignal<boolean> = signal(true);
    public readonly photoStore: WritableSignal<PhotoPageStore> = signal(defaultPhotoPageContainer);
    
    public readonly photoQuery$: Observable<Omit<IPhotoQueryParameters, 'offset'|'limit'>> =
        /** Parse incomming `ParamMap` URL/Query Parameters into a supported `IPhotoQueryParameters` collection. */
        this.route.queryParamMap.pipe(
            map(params => {
                console.debug('Query params updating..');
                const query = {
                    search: params.get('search') || undefined,
                    summary: params.get('summary') || undefined,
                    title: params.get('title') || undefined,
                    slug: params.get('slug') || undefined,
                    tags: params.getAll('t') || undefined,
                };
                console.debug('Query params:', query);
                return query;
            })
        );

    public readonly photoQuery = toSignal(this.photoQuery$);
    public readonly query = effect(() => {
        const _q = this.photoQuery();
        if (!_q) {
            return;
        }

        this.searchForPhotos(_q);
    });

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
     */
    public onSearch = (searchQuery: SearchQueryParameters) => {
    }

    /**
     * Callback subscribed to query-parameters mutating.
     * Performs the GET-Request to search for photos.
     */
    public searchForPhotos = (searchQuery: Omit<IPhotoQueryParameters, 'offset'|'limit'>) => {
        console.debug('searchForPhotos searchQuery', { ...searchQuery });

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
        const photoQuery: IPhotoQueryParameters = {
            ...searchQuery,
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
