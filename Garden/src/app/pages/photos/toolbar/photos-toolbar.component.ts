import { Component, effect, inject, input, output, signal } from '@angular/core';
import { NavbarControllerService } from '../../../layout/navbar/navbar-controller.service';
import { SearchBarComponent } from '../../../shared/blocks/search-bar/search-bar.component';
import { TagsInputComponent } from '../../../shared/blocks/tags/tags-input.component';
import { IPhotoQueryParameters } from '../../../core/types/photos.types';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatIconButton } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { SelectionObserver } from '../selection-observer.component';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';

@Component({
    selector: 'photos-toolbar',
    imports: [
        ReactiveFormsModule,
        SearchBarComponent,
        TagsInputComponent,
        MatToolbarModule,
        MatButtonModule,
        MatIconButton,
        MatIconModule,
        FormsModule
    ],
    templateUrl: 'photos-toolbar.component.html',
    styleUrl: 'photos-toolbar.component.scss'
})
export class PhotoToolbarComponent {
    private readonly navbarController = inject(NavbarControllerService);
    private readonly selectionObserver = inject(SelectionObserver);
    private readonly urlEncoder = inject(HttpUrlEncodingCodec);
    private readonly route = inject(ActivatedRoute);

    public readonly selectionMode = this.selectionObserver.isSelecting;

    public readonly getNavbar = this.navbarController.getNavbar;
    public readonly searchControl = signal(new FormControl<string>('') as FormControl<string>);
    public readonly tagsControl = signal(new FormControl<string>('') as FormControl<string>);
    public readonly tags = signal<string[]>([]);
    
    /**
     * Parse the `ParamMap` URL/Query Parameters observable into a supported
     * `IPhotoQueryParameters` collection.
     */
    public readonly queryParameters = toSignal<IPhotoQueryParameters>(
        this.route.queryParamMap.pipe(
            map(params => {
                let query: IPhotoQueryParameters = {
                    search: '',
                    offset: 0,
                    limit: 32
                };

                let searchValue = this.searchControl()
                    .value
                    ?.trim()
                    ?.normalize(); 

                if (searchValue) {
                    query.search = searchValue;
                }
                else if (params.get('search')) {
                    query.search = this.urlEncoder.encodeValue(params.get('search') || '');
                }

                let tags = this.tags().map(
                    tag => tag?.trim()?.normalize()
                );

                if (tags.length) {
                    query.tags = tags;
                }
                else if (params.get('search')) {
                    query.search = this.urlEncoder.encodeValue(params.get('search') || '');
                }
                else {
                    let tagValue = this.tagsControl()
                        .value
                        ?.trim()
                        ?.normalize(); 

                    if (tagValue) {
                        query.tags = [tagValue];
                    }
                }

                if (params.has('slug')) {
                    query.slug = this.urlEncoder.encodeValue(params.get('slug') || '');
                }
                if (params.has('title')) {
                    query.title = this.urlEncoder.encodeValue(params.get('title') || '');
                }
                if (params.has('summary')) {
                    query.summary = this.urlEncoder.encodeValue(params.get('summary') || '');
                }
                if (params.has('tags')) {
                    query.tags = (params.getAll('tags') || []).map(this.urlEncoder.encodeValue);
                }
                if (params.has('offset')) {
                    let offsetParam: string|number = params.get('offset') || 0;
                    if (typeof offsetParam === 'string') {
                        offsetParam = Number.parseInt(offsetParam);
                    }
                    if (Number.isNaN(offsetParam) || offsetParam < 0) {
                        throw new Error('Invalid "offset" param');
                    }
                    query.offset = offsetParam;
                }
                if (params.has('limit')) {
                    let limitParam: string|number = params.get('limit') || 0;
                    if (typeof limitParam === 'string') {
                        limitParam = Number.parseInt(limitParam);
                    }
                    if (Number.isNaN(limitParam) || limitParam < 0) {
                        throw new Error('Invalid "offset" param');
                    }
                    query.offset = limitParam;
                }

                return query;
            })
        )
    );

    /**
     * Effect invoked when search-query parameters gets read/updated.
     */
    private readonly onParametersChange = effect(
        () => {
            const parameters: IPhotoQueryParameters = this.queryParameters() || {
                search: this.searchControl().value || '',
                tags: this.tags() || [],
                offset: 0,
                limit: 32
            };

            if (Array.isArray(parameters.tags) && parameters.tags.length) {
                this.tags.set(parameters.tags);
            }

            this.searchEvent.emit(parameters);
        }
    );

    /**
     * Callback invoked when a search-query is triggered.
     */
    public readonly searchEvent = output<IPhotoQueryParameters>({ alias: 'onSearch' });
}
