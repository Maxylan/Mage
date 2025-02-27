import { Component, inject, signal } from '@angular/core';
import { PhotosService } from '../../core/api/photos.service';
import { IPhotoQueryParameters, IPhotoSearchParameters, PhotoCollection } from '../../core/types/photos.types';
import { PhotoCardComponent } from '../../shared/cards/photos/photo-card.component';
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
        PhotoCardComponent,
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
    private navbarController = inject(NavbarControllerService);
    private breakpointObserver = inject(BreakpointObserver);
    private photoService = inject(PhotosService);

    public getNavbar = this.navbarController.getNavbar;

    photos: PhotoCollection[] = [];

    public page = 0;
    public pageSize = 32;
    
    isLoading = signal(false);
    public searchForPhotos: SearchCallback<IPhotoSearchParameters, PhotoCollection> = (params) => {
        this.isLoading.set(true);

        const fetchLimit = this.pageSize * 3;
        const offsetStart = this.page < 1 ? 0 : this.page - 1;
        const searchQuery: IPhotoQueryParameters = {
            limit: fetchLimit,
            offset: offsetStart,
            slug: params?.query,
            title: params?.query,
            summary: params?.query
        };

        return this.photoService
            .getPhotos(searchQuery)
            .then(data => {
                console.debug('[searchForPhotos] Search Result', data);
                this.photos = data;
            })
            .catch(err => {
                console.error('[searchForPhotos] Error!', err);
                this.photos = [];
            })
            .then(() => this.photos)
            .finally(() => this.isLoading.set(false));
    }

    constructor() {
        this.searchForPhotos();
    }

    isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );
}
