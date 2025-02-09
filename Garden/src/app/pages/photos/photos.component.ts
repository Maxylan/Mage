import { Component, inject, signal } from '@angular/core';
import { PhotosService } from '../../core/api/photos.service';
import { PhotoCollection } from '../../core/types/photos.types';
import { PhotoCardComponent } from '../../shared/cards/photos/photo-card.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { map, Observable, shareReplay } from 'rxjs';

@Component({
    selector: 'page-list-photos',
    imports: [
        PhotoCardComponent,
        PaginationComponent
    ],
    providers: [
        PhotosService,
    ],
    templateUrl: 'photos.component.html',
    styleUrl: 'photos.component.css'
})
export class PhotosComponent {
    private breakpointObserver = inject(BreakpointObserver);
    private photoService = inject(PhotosService);

    photos: PhotoCollection[] = [];

    page = 0;
    pageSize = 16;
    
    isLoading = signal(false);
    getPhotos = (): void => {
        this.isLoading.set(true);
        this.photoService
            .getPhotos({
                offset: this.page,
                limit: this.pageSize
            })
            .then(data => this.photos = data)
            .catch(err => {
                console.error('[getPhotos] Error!', err);
                this.photos = [];
            })
            .finally(() => this.isLoading.set(false));
    }

    constructor() {
        this.getPhotos();
    }

    isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );
}
