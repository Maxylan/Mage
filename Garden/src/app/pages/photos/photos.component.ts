import { Component, computed, inject, signal } from '@angular/core';
import { defaultPhotoPageContainer, FavoritePhotos, IPhotoQueryParameters, Photo } from '../../core/types/photos.types';
import { SelectionObserver, SelectionState } from './selection-observer.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { PhotoToolbarComponent } from './toolbar/photos-toolbar.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatGridListModule } from '@angular/material/grid-list';
import { PhotosService } from '../../core/api/photos.service';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, shareReplay } from 'rxjs';
import { PhotoCardComponent } from './photo-card/photo-card.component';

@Component({
    selector: 'page-list-photos',
    imports: [
        MatGridListModule,
        PhotoToolbarComponent,
        PaginationComponent,
        PhotoCardComponent
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
    private readonly photoService = inject(PhotosService);

    public readonly photoStore = signal(defaultPhotoPageContainer);
    public readonly selectionState = this.selectionObserver.State;
    public readonly select = this.selectionObserver.selectItems;
    public readonly deselect = this.selectionObserver.deselectItems;
    public readonly isLoadingPhotos = this.photoService.isLoading;

    public readonly favoriteStore = signal<string|null>(window.localStorage.getItem(`favourite-photos`));
    public readonly favorites = computed<FavoritePhotos>(() => {
        const encodedStore = this.favoriteStore();
        if (!encodedStore) {
            return [];
        }

        try {
            const store = JSON.parse(encodedStore);
            return store;
        }
        catch(err) {
            console.error('Failed to parse Favorite Store!', err);
            return [];
        }
    });
    public readonly isFavorite = computed(() => {
        return (photoId: Photo['photoId']) => 
            (): boolean => {
                return this.favorites().has(photoId);
            }
    });

    /**
     * Signal/Breakpoint for a mobile/handset-viewport.
     */
    public readonly isHandset = toSignal(
        this.breakpointObserver
            .observe(Breakpoints.Handset)
            .pipe(
                map(result => result.matches),
                shareReplay()
            )
    );

    /**
     * Callback subscribed to query-parameters mutating.
     * Performs the GET-Request to search for photos.
     */
    public readonly search = (query: IPhotoQueryParameters) => {
        if (!query || !Object.keys(query).length) {
            console.warn('Skipping an empty search query!', query);
            return;
        }

        const {
            currentPage,
            pageSize
        } = this.photoStore();

        const limit = currentPage > 0 ? pageSize * 3 : pageSize * 2;
        const offset = currentPage > 1 ? limit * currentPage - pageSize : 0;
        const photoQuery: IPhotoQueryParameters = {
            ...query,
            offset: offset,
            limit: limit
        }

        return this.photoService
            .getPhotos(photoQuery)
            .then(data => {
                // console.debug('[search] Result', { ...data });
                this.photoStore.update(store => {
                    let iteration = 0;
                    while(data.length > 0 && ++iteration < 3) {
                        const sliceLength = data.length < pageSize
                            ? data.length
                            : pageSize;

                        const slice = new Set(data.splice(0, sliceLength));

                        const pageNumber = currentPage && currentPage - iteration;
                        const pageIndex = store.pages.findIndex(p => p.page === pageNumber);
                        if (pageIndex === -1) {
                            store.pages.push({
                                page: pageNumber,
                                set: slice
                            });
                        }
                        else {
                            store.pages[pageIndex] = {
                                page: pageNumber,
                                set: slice
                            };
                        }
                    }
                    
                    return store;
                })
            })
            .catch(err => {
                console.error('[searchForPhotos] Error!', err);
                /* this.photoStore.set({
                    ...this.photoStore(),
                    store: []
                }); */
            });
    }
}
