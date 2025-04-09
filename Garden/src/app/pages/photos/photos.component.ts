import { Component, inject, model, signal, WritableSignal } from '@angular/core';
import { PhotosService } from '../../core/api/photos.service';
import { defaultPhotoPageContainer, IPhotoQueryParameters, Photo, PhotoPage, PhotoPageStore } from '../../core/types/photos.types';
import { ThumbnailCardComponent } from '../../shared/cards/with-thumbnail/card-with-thumbnail.component';
import { PhotoThumbnailComponent } from '../../shared/cards/with-thumbnail/photo/photo-thumbnail.component';
import { SelectionObserver, SelectState } from './toolbar/selection-observer.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { PhotoToolbarComponent } from './toolbar/photos-toolbar.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatGridListModule } from '@angular/material/grid-list';
import { filter, first, map, Observable, shareReplay, take } from 'rxjs';
import { MatDivider } from '@angular/material/divider';
import { AsyncPipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HttpUrlEncodingCodec } from '@angular/common/http';

@Component({
    selector: 'page-list-photos',
    imports: [
        ThumbnailCardComponent,
        PhotoThumbnailComponent,
        PhotoToolbarComponent,
        PaginationComponent,
        MatGridListModule,
        AsyncPipe
    ],
    providers: [
        HttpUrlEncodingCodec,
        PhotosService
    ],
    templateUrl: 'photos.component.html',
    styleUrl: 'photos.component.css'
})
export class PhotosComponent {
    private readonly breakpointObserver = inject(BreakpointObserver);
    private readonly selectionObserver = inject(SelectionObserver);
    private readonly urlEncoder = inject(HttpUrlEncodingCodec);
    private readonly photoService = inject(PhotosService);
    private readonly route = inject(ActivatedRoute);

    public readonly selectPhoto = (photo: Photo): ((isSelected: boolean) => void) => (
        (isSelected: boolean) => isSelected
            ? this.selectionObserver.deselectItems(photo)
            : this.selectionObserver.selectItems(photo)
    );

    public readonly store: WritableSignal<PhotoPageStore> = model(defaultPhotoPageContainer);
        /* const {
            page,
            pageSize,
            currentPage,
            isLoading
        } = pageStore;

        const pageIndex = page.findIndex(p => p.page === currentPage);
        if (pageIndex === -1) {
            console.warn(`Page not found! (${currentPage})`, store);
            this.page = null;
            return;
        }

        this.page = store[currentPage];
        this.pageIndex = currentPage;
        this.pageSize = pageSize;
        this.isLoading = isLoading;
        this.photoCount = store.reduce(
            (prev, val) => prev += val.set.size, 0
        );
    } */
    
    public readonly initialQueryParameters$: Observable<IPhotoQueryParameters> =
        /** Parse incomming `ParamMap` URL/Query Parameters into a supported `IPhotoQueryParameters` collection. */
        this.route.queryParamMap.pipe(
            map(params => {
                console.debug('Query params updating..');
                let query: IPhotoQueryParameters = {
                    search: '',
                    offset: 0,
                    limit: 32
                };

                if (params.get('search')) {
                    query.search = this.urlEncoder.encodeValue(params.get('search') || '');
                }
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
                    query.tags = (params.getAll('tags') || []).map(this.urlEncoder.encodeValue);
                }
                if (params.has('offset')) {
                    let offsetParam: string|number = params.get('offset') || 0;
                    if (typeof offsetParam === 'string') {
                        offsetParam = Number.parseInt(offsetParam);
                    }
                    if (Number.isNaN(offsetParam) || offsetParam < 0) {
                        throw new Error('Invalid "offset" param');
                    }
                    query.offset = offsetParam;
                }
                if (params.has('limit')) {
                    let limitParam: string|number = params.get('limit') || 0;
                    if (typeof limitParam === 'string') {
                        limitParam = Number.parseInt(limitParam);
                    }
                    if (Number.isNaN(limitParam) || limitParam < 0) {
                        throw new Error('Invalid "offset" param');
                    }
                    query.offset = limitParam;
                }
                console.debug('Query params:', query);
                return query;
            })
        );

    public readonly isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );

    /**
     * Callback subscribed to query-parameters mutating.
     * Performs the GET-Request to search for photos.
     */
    public searchForPhotos = (searchQuery: ) => {
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
}
