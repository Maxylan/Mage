import { computed, effect, inject, Injectable, signal, untracked } from "@angular/core";
import { AuthService } from "../../../core/api/services/auth.service";
import { TagsService } from "../../../core/api/services/tags.service";
import { PhotosService } from "../../../core/api/services/photos.service";
import { SelectionObserver } from "../../../layout/toolbar/selection-observer.service";
import { SearchPhotosParameters } from "../../../core/types/search-photos-parameters";
import { DisplayPhoto } from "../../../core/types/generated/display-photo";
import { PhotosPageComponent } from "../photos.component";
import PageBase from "../../../core/classes/page.class";
import { PageEvent } from "@angular/material/paginator";
import { Account } from "../../../core/types/generated/account";
import { ITagDTO } from "../../../core/types/generated/tag-dto";

@Injectable({
    providedIn: PhotosPageComponent,
})
export class PhotosPageService extends PageBase {
    // Dependencies
    private readonly tagsService = inject(TagsService);
    private readonly photosService = inject(PhotosService);
    private readonly authService = inject(AuthService);

    public readonly selectionObserver = inject(SelectionObserver);

    // States
    public readonly isEmpty = computed<boolean>(() => this.photos().length === 0);
    public readonly searchParameters = signal<SearchPhotosParameters>({
        offset: 99,
        limit: 0
    });

    public readonly me = signal<Account|null>(null);

    public readonly selectionState = this.selectionObserver.State;
    public readonly photos = signal<DisplayPhoto[]>([]);

    public readonly tagNames = signal<string[]>([]);
    public readonly tags = signal<ITagDTO[]>([]);

    // Photos
    public async toggleFavorite(photoId: number): Promise<void> {
        await this.photosService
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
        this.tags.set([]);

        const myAccount = await this.authService.me();
        if (!myAccount) {
            throw Error('[PhotosPageService] Failed to get account details of the current user!');
        }

        this.me.set(myAccount);

        const searchParameters = untracked(this.searchParameters);
        const searchPhotosPromise = this.photosService
            .searchDisplayPhotos(searchParameters)
            .then(
                res => this.photos.set(res),
                rej => console.warn('', rej)
            );

        const tagsPromise = this.getOrCreateTags(
            this.tagNames()
        )
            .then(
                res => this.tags.set(res),
                rej => console.warn('', rej)
            );

        await Promise.all([
            searchPhotosPromise,
            tagsPromise
        ])
            .finally(() => this.isLoading.set(false));
    }

    // Update main-page data, incl. pagination-related parameters
    private runs: number = 0;
    public searchParametersSideEffect = effect(cleanup => {
        const isLoading = untracked(this.isLoading);
        console.log('searchParametersSideEffect ' + ++this.runs, '- isLoading', isLoading);
        if (isLoading) {
            cleanup(this.refetch);
        }
        else {
            void this.refetch();
        }
    });

    public /* async */ update(params?: SearchPhotosParameters): void /* Promise<void> */ {
        if (params) {
            this.searchParameters.set(params);
        }

        // await this.refetch();
    }

    // Update pagination-related parameters
    public /* async */ pagination(event?: PageEvent): void /* Promise<void> */ {
        if (event) {
            this.searchParameters.update(params => {
                params.limit = event.pageSize;
                params.offset = event.pageIndex;
                return params;
            });

            // await this.refetch();
        }
    }

    // Selects what function to use to fetch tags, based on the user's permissions. 
    public get view() {
        return this.auth(AuthService.VIEW);
    }
    public get viewAll() {
        return this.auth(AuthService.VIEW_ALL);
    }
    public get create() {
        return this.auth(AuthService.CREATE);
    }
    public get delete() {
        return this.auth(AuthService.DELETE);
    }
    public get administrate() {
        return this.auth(AuthService.ADMIN);
    }
    public auth(privilege: number): boolean {
        const me = this.me();
        if (!me) {
            return false;
        }

        return AuthService.Can(this.me()).CheckPrivilege(privilege);
    }

    // Selects what function to use to fetch tags, based on the user's permissions. 
    public getOrCreateTags(tagNames: string[]): Promise<ITagDTO[]> {
        if (AuthService.Can(this.me()).Create()) {
            return this.tagsService.createTags(tagNames);
        }

        return this.tagsService.getTagsByNames(tagNames);
    }
}
