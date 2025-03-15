import { Component, effect, EventEmitter, inject, Input, Output, signal, Signal } from '@angular/core';
import { SelectionObserver, SelectState } from './selection-observer.component';
import { NavbarControllerService } from '../../../layout/navbar/navbar-controller.service';
import { SearchBarComponent } from '../../../shared/blocks/search-bar/search-bar.component';
import {
    SearchCallback,
    SearchParameters
} from '../../../shared/blocks/search-bar/search-bar.types';

@Component({
    selector: 'photos-toolbar',
    imports: [
        SearchBarComponent
    ],
    styleUrl: 'photos-toolbar.component.css',
    templateUrl: 'photos-toolbar.component.html'
})
export class PhotoToolbarComponent {
    private navbarController = inject(NavbarControllerService);

    @Input()
    public selectionState?: Signal<SelectState>;

    @Input()
    public setSelectionMode?: SelectionObserver['setSelectionMode'];

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
}
