import { Component, computed, inject, signal } from '@angular/core';
import { PhotosService } from '../../core/api/photos.service';
import { defaultPhotoPageContainer, IPhotoQueryParameters, IPhotoSearchParameters, Photo, PhotoCollection, PhotoPage, PhotoPageStore } from '../../core/types/photos.types';
import { ThumbnailCardComponent } from '../../shared/cards/with-thumbnail/card-with-thumbnail.component';
import { PhotoThumbnailComponent } from '../../shared/cards/with-thumbnail/photo/photo-thumbnail.component';
import { NavbarControllerService } from '../../layout/navbar/navbar-controller.service';
import { SelectionObserver, SelectState } from './toolbar/selection-observer.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { PhotoToolbarComponent } from './toolbar/photos-toolbar.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatGridListModule } from '@angular/material/grid-list';
import { map, Observable, shareReplay } from 'rxjs';
import { MatDivider } from '@angular/material/divider';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AsyncPipe } from '@angular/common';

@Component({
    selector: 'page-list-photos',
    imports: [
        ThumbnailCardComponent,
        PhotoThumbnailComponent,
        PhotoToolbarComponent,
        PaginationComponent,
        MatGridListModule,
        MatToolbarModule,
        MatButtonModule,
        MatIconModule,
        AsyncPipe
    ],
    providers: [
        PhotosService,
    ],
    templateUrl: 'photos.component.html',
    styleUrl: 'photos.component.css'
})
export class PhotosComponent {
    private breakpointObserver = inject(BreakpointObserver);
    private selectionObserver = inject(SelectionObserver);
    private photoService = inject(PhotosService);

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


    public isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );

    public selectPhoto = (photo: Photo) => (() => this.selectionObserver.selectItems(photo));
    public setSelectionMode = this.selectionObserver.setSelectionMode;
    public selectionState = this.selectionObserver.State;

    constructor() {
        this.searchForPhotos();
    }
}
