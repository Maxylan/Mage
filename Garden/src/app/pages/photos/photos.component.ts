import { Component, inject } from '@angular/core';
import { PhotosService } from '../../core/api/photos.service';
import { Photo, PhotoPage, PhotoPageStore } from '../../core/types/photos.types';
import { ThumbnailCardComponent } from '../../shared/cards/with-thumbnail/card-with-thumbnail.component';
import { PhotoThumbnailComponent } from '../../shared/cards/with-thumbnail/photo/photo-thumbnail.component';
import { SelectionObserver, SelectState } from './toolbar/selection-observer.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { PhotoToolbarComponent } from './toolbar/photos-toolbar.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatGridListModule } from '@angular/material/grid-list';
import { map, Observable, shareReplay } from 'rxjs';
import { MatDivider } from '@angular/material/divider';
import { AsyncPipe } from '@angular/common';

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
    templateUrl: 'photos.component.html',
    styleUrl: 'photos.component.css'
})
export class PhotosComponent {
    private breakpointObserver = inject(BreakpointObserver);
    private selectionObserver = inject(SelectionObserver);

    public selectPhoto = (photo: Photo) => (() => this.selectionObserver.selectItems(photo));
    public setSelectionMode = this.selectionObserver.setSelectionMode;
    public selectionState = this.selectionObserver.State;

    public page?: PhotoPage|null;
    public pageIndex?: number;
    public pageSize?: number;
    public photoCount?: number;
    public isLoading: boolean = false;

    public computePhotoStoreValues = (pageStore: PhotoPageStore): void => {
        const {
            store,
            pageSize,
            currentPage,
            isLoading
        } = pageStore;
        const pageIndex = store.findIndex(p => p.page === currentPage);
        if (pageIndex === -1) {
            console.warn(`Page not found! (${currentPage})`, store);
            return;
        }

        this.page = store[currentPage];
        this.pageIndex = currentPage;
        this.pageSize = pageSize;
        this.isLoading = isLoading;
        this.photoCount = store.reduce(
            (prev, val) => prev += val.set.size, 0
        );
    }

    public isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );
}
