import { computed, inject, Injectable, signal } from "@angular/core";
import { PhotosService } from "../../../core/api/services/photos.service";
import { SelectionObserver } from "../../../layout/toolbar/selection-observer.service";
import { SearchPhotosParameters } from "../../../core/types/search-photos-parameters";
import { DisplayPhoto } from "../../../core/types/generated/display-photo";
import { PhotosPageComponent } from "../photos.component";
import PageBase from "../../../core/classes/page.class";
import { PageEvent } from "@angular/material/paginator";

@Injectable({
    providedIn: PhotosPageComponent,
})
export class PhotosPageService extends PageBase {
    // Dependencies
    private readonly photoService = inject(PhotosService);
    private readonly selectionObserver = inject(SelectionObserver);

    // States
    public readonly isEmpty = computed<boolean>(() => this.photos().length === 0);
    public readonly searchParameters = signal<SearchPhotosParameters>({
        offset: 99,
        limit: 0
    });

    public readonly selectionState = this.selectionObserver.State;
    public readonly photos = signal<DisplayPhoto[]>([]);

    // Photos
    public async toggleFavorite(photoId: number): Promise<void> {
        await this.photoService
            .toggleFavorite(photoId)
            .then(this.refetch);
    }

    // Selection
    public readonly select = this.selectionObserver.selectItems;
    public readonly deselect = this.selectionObserver.deselectItems;

    // Refetch main-page data
    public async refetch(): Promise<void> {
        this.isLoading.set(true);
        this.photos.set([]);

        await this.photoService
            .searchDisplayPhotos(this.searchParameters())
            .then(
                res => this.photos.set(res),
                rej => console.warn('', rej)
            )
            .finally(() => this.isLoading.set(true));
    }

    // Update main-page data, incl. pagination-related parameters
    public async update(event?: PageEvent): Promise<void> {
        if (event) {
            this.searchParameters.update(params => {
                params.limit = event.pageSize;
                params.offset = event.pageIndex;
                return params;
            });
        }

        await this.refetch();
    }
}
