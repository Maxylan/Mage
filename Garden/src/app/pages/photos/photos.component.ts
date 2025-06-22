import { Component, computed, inject, signal } from '@angular/core';
import { SelectionObserver, SelectionState } from '../../layout/toolbar/selection-observer.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { PhotoToolbarComponent } from './toolbar/photos-toolbar.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { PhotosService } from '../../core/api/services/photos.service';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, shareReplay } from 'rxjs';
import { PhotoCardComponent } from './photo-card/photo-card.component';
import { NgClass } from '@angular/common';
import { FavoritePhotoRelation } from '../../core/types/generated/favorite-photo-relation';
import { Photo } from '../../core/types/generated/photo';
import { SearchPhotosParameters } from '../../core/types/search-photos-parameters';

@Component({
    selector: 'page-list-photos',
    imports: [
        PhotoToolbarComponent,
        PaginationComponent,
        PhotoCardComponent,
        NgClass
    ],
    providers: [
        SelectionObserver,
        HttpUrlEncodingCodec,
        PhotosService
    ],
    templateUrl: 'photos.component.html',
    styleUrl: 'photos.component.css'
})
export class PhotosPageComponent {
    private readonly breakpointObserver = inject(BreakpointObserver);
    private readonly selectionObserver = inject(SelectionObserver);
    private readonly photoService = inject(PhotosService);

    public readonly navbarOpen = signal<boolean>(false);

    public readonly photoStore = signal<{ currentPage: number, pageSize: number }>({ currentPage: 0, pageSize: 24 });
    public readonly selectionState = this.selectionObserver.State;
    public readonly select = this.selectionObserver.selectItems;
    public readonly deselect = this.selectionObserver.deselectItems;
    public readonly isLoadingPhotos = this.photoService.isLoading;

    public readonly favoriteStore = signal<string|null>(window.localStorage.getItem(`favourite-photos`));
    public readonly favorites = computed<FavoritePhotoRelation[]>(() => {
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
        return (photoId: Photo['id']) => 
            (): boolean => {
                return this.favorites().some(relation => relation.photoId === photoId);
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
    public readonly search = (query: SearchPhotosParameters) => {
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
        const photoQuery: SearchPhotosParameters = {
            ...query,
            offset: offset,
            limit: limit
        }

        return this.photoService
            .searchPhotos(photoQuery)
            .then(data => {
                // console.debug('[search] Result', { ...data });
                /*this.photoStore.update(store => {
                    let iteration = 0;
                    while(data.length > 0 && ++iteration < 3) {
                        const sliceLength = data.length < pageSize
                            ? data.length
                            : pageSize;

                        const slice = new Set(data.splice(0, sliceLength));

                        const pageNumber = currentPage && currentPage - iteration;
                        const pageIndex = store.page.findIndex(p => p.page === pageNumber);

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
                })*/
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
