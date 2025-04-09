import { Component, inject, signal } from '@angular/core';
import { PhotosService } from '../../core/api/photos.service';
import { defaultPhotoPageContainer, IPhotoQueryParameters, Photo } from '../../core/types/photos.types';
import { ThumbnailCardComponent } from '../../shared/cards/with-thumbnail/card-with-thumbnail.component';
import { PhotoThumbnailComponent } from '../../shared/cards/with-thumbnail/photo/photo-thumbnail.component';
import { SelectionObserver, SelectState } from './selection-observer.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { PhotoToolbarComponent } from './toolbar/photos-toolbar.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatGridListModule } from '@angular/material/grid-list';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { AsyncPipe } from '@angular/common';
import { map, shareReplay } from 'rxjs';

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
    private readonly photoService = inject(PhotosService);

    public readonly photoStore = signal(defaultPhotoPageContainer);
    public readonly selectionState = this.selectionObserver.State;
    public readonly isLoadingPhotos = this.photoService.isLoading;

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
     * Select or deselect a photo, based on if its already selected.
     */
    public readonly selectPhoto = (photo: Photo): ((isSelected: boolean) => void) => (
        (isSelected: boolean) => isSelected
            ? this.selectionObserver.deselectItems(photo)
            : this.selectionObserver.selectItems(photo)
    );

    /**
     * Callback subscribed to query-parameters mutating.
     * Performs the GET-Request to search for photos.
     */
    public search = (query: Partial<IPhotoQueryParameters>) => {
        // console.debug('search query', { ...query });
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
                        const pageIndex = store.page.findIndex(p => p.page === pageNumber);
                        if (pageIndex === -1) {
                            store.page.push({
                                page: pageNumber,
                                set: slice
                            });
                        }
                        else {
                            store.page[pageIndex] = {
                                page: pageNumber,
                                set: slice
                            };
                        }
                    }
                    
                    console.debug(store);
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
