import { Component, inject, input, signal } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { PhotosService } from '../../core/api/services/photos.service';

export type MenuEvent = [string, number];

@Component({
    selector: 'app-card-image',
    standalone: true,
    imports: [ MatProgressBarModule ],
    styleUrl: 'image.component.scss',
    templateUrl: 'image.component.html'
})
export class CardImageComponent {
    private readonly photoService = inject(PhotosService);

    public readonly photoId = input.required<number>();
    public readonly alt = input<string>();

    public readonly source = signal<string|null>(null);
}
