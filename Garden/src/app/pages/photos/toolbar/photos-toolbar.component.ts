import { Component, Input, Output, Signal, WritableSignal, computed, inject, signal } from '@angular/core';
import { NavbarControllerService } from '../../../layout/navbar/navbar-controller.service';
import { SelectionObserver, SelectState } from './selection-observer.component';
import { defaultPhotoPageContainer, IPhotoQueryParameters, PhotoPageStore } from '../../../core/types/photos.types';
import { SearchCallback, SearchParameters } from '../../../shared/blocks/search-bar/search-bar.types';
import { SearchBarComponent } from '../../../shared/blocks/search-bar/search-bar.component';
import { PhotosService } from '../../../core/api/photos.service';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Observable } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'photos-toolbar',
    imports: [
        SearchBarComponent,
        MatToolbarModule,
        MatButtonModule,
        MatIconModule
    ],
    providers: [
        PhotosService,
    ],
    templateUrl: 'photos-toolbar.component.html',
    styleUrl: 'photos-toolbar.component.scss'
})
export class PhotoToolbarComponent {
    private navbarController = inject(NavbarControllerService);
    private photoService = inject(PhotosService);

    public getNavbar = this.navbarController.getNavbar;
    public isLoading: WritableSignal<boolean> = signal(false);
    public photoStore: WritableSignal<PhotoPageStore> = signal(defaultPhotoPageContainer);

    @Input()
    public selectionState?: Signal<SelectState>;

    @Input()
    public setSelectionMode?: SelectionObserver['setSelectionMode'];

    @Output()
    public onPhotosChange$: Observable<PhotoPageStore> = toObservable(this.photoStore);

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
}
