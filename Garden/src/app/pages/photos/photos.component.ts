import { Component, computed, inject, Signal, signal } from '@angular/core';
import { PhotosService } from '../../core/api/photos.service';
import { defaultPhotoPageContainer, IPhotoQueryParameters, IPhotoSearchParameters, PhotoCollection, PhotoPage, PhotoPageStore } from '../../core/types/photos.types';
import { ThumbnailCardComponent } from '../../shared/cards/with-thumbnail/card-with-thumbnail.component';
import { PhotoThumbnailComponent } from '../../shared/cards/with-thumbnail/photo/photo-thumbnail.component';
import { SearchBarComponent } from '../../shared/blocks/search-bar/search-bar.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { map, Observable, shareReplay } from 'rxjs';
import { MatDivider } from '@angular/material/divider';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { SearchCallback, SearchParameters } from '../../shared/blocks/search-bar/search-bar.types';
import { NavbarControllerService } from '../../layout/navbar/navbar-controller.service';

@Component({
    selector: 'page-list-photos',
    imports: [
        SearchBarComponent,
        ThumbnailCardComponent,
        PhotoThumbnailComponent,
        PaginationComponent,
        MatToolbarModule,
        MatButtonModule,
        MatIconModule,
    ],
    providers: [
        PhotosService,
    ],
    templateUrl: 'photos.component.html',
    styleUrl: 'photos.component.css'
})
export class PhotosComponent {
    private photoService = inject(PhotosService);
    private breakpointObserver = inject(BreakpointObserver);
    private navbarController = inject(NavbarControllerService);

    private photoStore = signal(defaultPhotoPageContainer);
    public photos = computed<number>(() => {
        const pageStore = this.photoStore();
        return pageStore.store.reduce(
            (prev, val) => prev += val.set.size,
            0
        );
    });

    public pageIndex = computed<number>(() => this.photoStore().currentPage);
    public pageSize = computed<number>(() => this.photoStore().pageSize);
    public page = computed<PhotoPage|null>(() => {
        const { store, currentPage } = this.photoStore();
        const pageIndex = store.findIndex(p => p.page === currentPage);
        if (pageIndex === -1) {
            console.warn(`Page not found! (${currentPage})`, store);
            return null;
        }

        return store[currentPage];
    });

    public getNavbar = this.navbarController.getNavbar;
    public isLoading = signal(false);

    public searchForPhotos: SearchCallback = (params) => {
        this.isLoading.set(true);
        const {
            currentPage,
            pageSize
        } = this.photoStore();

        const fetchLimit = currentPage > 0 ? pageSize * 3 : pageSize * 2;
        const fetchOffset = currentPage > 1 ? fetchLimit * currentPage - pageSize : 0;

        const searchQuery: IPhotoQueryParameters = {
            limit: fetchLimit,
            offset: fetchOffset,
            slug: params?.query,
            title: params?.query,
            summary: params?.query
        };

        return this.photoService
            .getPhotos(searchQuery)
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

    isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );

    constructor() {
        this.searchForPhotos();
    }
}
