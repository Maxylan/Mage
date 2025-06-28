import { Component, inject, input, signal } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { PhotosService } from '../../core/api/services/photos.service';

export type MenuEvent = [string, number];

@Component({
    selector: 'app-upload-card-thumbnail',
    standalone: true,
    imports: [ MatProgressBarModule ],
    styleUrl: 'thumbnail.component.scss',
    templateUrl: 'thumbnail.component.html'
})
export class UploadCardThumbnailComponent {
    private readonly photoService = inject(PhotosService);

    public readonly file = input.required<File>();
    public readonly alt = input<string>();

    public readonly source = signal<string|null>(null);
}
