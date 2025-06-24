import { Component, inject } from '@angular/core';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { SelectionObserver } from '../../layout/toolbar/selection-observer.service';
import { PaginationComponent } from '../../shared/pagination/pagination.component';
import { PhotoToolbarComponent } from './toolbar/photos-toolbar.component';
import { PhotosService } from '../../core/api/services/photos.service';
import { PhotoCardComponent } from './photo-card/photo-card.component';
import { NgClass } from '@angular/common';
import { PhotosPageService } from './services/photos-page.service';

@Component({
    selector: 'page-list-photos',
    imports: [
        PhotoToolbarComponent,
        PaginationComponent,
        PhotoCardComponent,
        NgClass
    ],
    providers: [
        PhotosService,
        PhotosPageService,
        HttpUrlEncodingCodec,
        SelectionObserver
    ],
    templateUrl: 'photos.component.html',
    styleUrl: 'photos.component.css'
})
export class PhotosPageComponent {
    private readonly pageService = inject(PhotosPageService);
}
