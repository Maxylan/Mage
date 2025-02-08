import { Component, inject, signal } from '@angular/core';
import { PhotosService } from '../../core/api/photos.service';
import { PhotoCollection } from '../../core/types/photos.types';
import { PhotoCardComponent } from '../../shared/cards/photos/photo-card.component';
import { PaginationComponent } from '../../shared/pagination/pagination.component';

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
    private photoService = inject(PhotosService);

    photos: PhotoCollection[] = [];

    page = 0;
    pageSize = 16;
    
    isLoading = signal(false);
    getPhotos = (): void => {
        // try {
            this.isLoading.set(true);
            this.photoService
                .getPhotos({
                    offset: this.page,
                    limit: this.pageSize
                })
                .then(data => this.photos = data)
                .finally(() => this.isLoading.set(false));
        /* }
        catch (err) {
            console.error('[getPhotos] Error!', err);
            this.isLoading.set(false);
            this.photos = [];
        } */
    }

    constructor() {
        this.getPhotos();

    }
}
