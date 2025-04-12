import { Component, effect, inject, output, signal } from '@angular/core';
import { NavbarControllerService } from '../../../layout/navbar/navbar-controller.service';
import { PhotoTagsInputComponent } from './tags/photo-tags-input.component';
import { IPhotoQueryParameters } from '../../../core/types/photos.types';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatIconButton } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { SelectionObserver } from '../selection-observer.component';
import { HttpUrlEncodingCodec } from '@angular/common/http';
import { MatInput } from '@angular/material/input';
import { MatChip } from '@angular/material/chips';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

@Component({
    selector: 'photos-toolbar',
    imports: [
        PhotoTagsInputComponent,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatToolbarModule,
        MatButtonModule,
        MatIconModule,
        MatIconButton,
        MatInput,
        MatChip
    ],
    templateUrl: 'photos-toolbar.component.html',
    styleUrl: 'photos-toolbar.component.scss'
})
export class PhotoToolbarComponent {
    private readonly navbarController = inject(NavbarControllerService);
    private readonly selectionObserver = inject(SelectionObserver);
    private readonly urlEncoder = inject(HttpUrlEncodingCodec);
    private readonly route = inject(ActivatedRoute);

    public readonly selectionState = this.selectionObserver.State;
    public readonly quitSelectMode = () => setTimeout(
        () => this.selectionObserver.setSelectionMode(false),
        64
    );

    public readonly getNavbar = this.navbarController.getNavbar;
    public readonly searchControl = new FormControl<string>('');
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

                let searchValue = this.searchControl
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
                    tag => this.urlEncoder.encodeValue(
                        tag?.trim()?.normalize()
                    )
                );

                if (tags.length) {
                    query.tags = tags;
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
                    let offsetParam: string | number = params.get('offset') || 0;
                    if (typeof offsetParam === 'string') {
                        offsetParam = Number.parseInt(offsetParam);
                    }
                    if (Number.isNaN(offsetParam) || offsetParam < 0) {
                        throw new Error('Invalid "offset" param');
                    }
                    query.offset = offsetParam;
                }
                if (params.has('limit')) {
                    let limitParam: string | number = params.get('limit') || 0;
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
                search: this.searchControl.value || '',
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
