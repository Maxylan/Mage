import { Component, inject, signal, WritableSignal } from '@angular/core';
import { PhotosService } from '../../core/api/photos.service';
import { Photo, PhotoPage, PhotoPageStore } from '../../core/types/photos.types';
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
    private readonly breakpointObserver = inject(BreakpointObserver);
    private readonly selectionObserver = inject(SelectionObserver);

    public readonly setSelectionMode = this.selectionObserver.setSelectionMode;
    public readonly selectionState = this.selectionObserver.State;
    public readonly selectPhoto = (photo: Photo): ((isSelected: boolean) => void) => (
        (isSelected: boolean) => isSelected
            ? this.selectionObserver.deselectItems(photo)
            : this.selectionObserver.selectItems(photo)
    );

    public pageIndex?: number;
    public pageSize?: number;
    public photoCount?: number;
    public isLoading: boolean = false;
    public page: PhotoPage|null = null;
    public readonly computePhotoStoreValues = (pageStore: PhotoPageStore): void => {
        const {
            store,
            pageSize,
            currentPage,
            isLoading
        } = pageStore;
        const pageIndex = store.findIndex(p => p.page === currentPage);
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
    }

    public readonly isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );
}
