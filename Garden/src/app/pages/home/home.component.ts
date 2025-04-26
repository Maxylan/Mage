import { Component, inject, signal } from '@angular/core';
import { BaseToolbarComponent } from '../../layout/toolbar/toolbar-base.component';
import { defaultPhotoPageContainer } from '../../core/types/photos.types';
import { PhotosService } from '../../core/api/photos.service';

@Component({
    selector: 'page-home',
    imports: [
        BaseToolbarComponent,
    ],
    templateUrl: 'home.component.html',
    styleUrl: 'home.component.css'
})
export class HomePageComponent {
    // private readonly photoService = inject(PhotosService);

    // public readonly photoStore = signal(defaultPhotoPageContainer);
}
