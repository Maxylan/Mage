import { Component, effect, inject, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { AuthService } from '../../core/api/services/auth.service';
import { TagsService } from '../../core/api/services/tags.service';
import { PhotosService } from '../../core/api/services/photos.service';
import { PhotosPageService } from './services/photos-page.service';
import { SelectionObserver } from '../../layout/toolbar/selection-observer.service';
import { PhotoSearchbarComponent } from './searchbar/photos-searchbar.component';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CardComponent } from '../../layout/card/card.component';
import { MatIcon } from '@angular/material/icon';

@Component({
    selector: 'page-list-photos',
    imports: [
        MatPaginatorModule,
        MatProgressSpinnerModule,
        PhotoSearchbarComponent,
        CardComponent,
        MatIcon,
        NgClass
    ],
    providers: [
        AuthService,
        TagsService,
        PhotosService,
        PhotosPageService,
        SelectionObserver
    ],
    templateUrl: 'photos.component.html',
    styleUrl: 'photos.component.css'
})
export class PhotosPageComponent {
    public static readonly LS_PAGE_KEY = 'photos-page-component';
    public readonly page = inject(PhotosPageService);
    /* - Inherited / Abstract methods:
    pageService.navbarOpen
    pageService.isLoading
    pageService.isEmpty
    pageService.isHandset
    pageService.refetch
    */

    public pageIndex = signal<number>(
        Number.parseInt(localStorage.getItem(PhotosPageComponent.LS_PAGE_KEY) || '0') || 0
    );

    public pageIndexSideEffect = effect(() => {
        localStorage.setItem(
            PhotosPageComponent.LS_PAGE_KEY,
            String(this.pageIndex())
        );
    });

    updatePagination(event: PageEvent) {
        this.pageIndex.set(event.pageIndex);
        this.page.pagination(event);
    }
}
