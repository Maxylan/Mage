import { Component, input, computed, effect } from '@angular/core';
import { MatCardSmImage } from '@angular/material/card';
import { MatProgressBar } from '@angular/material/progress-bar';

@Component({
    selector: 'shared-card-thumbnail',
    imports: [
        MatProgressBar,
        MatCardSmImage
    ],
    templateUrl: 'card-thumbnail.component.html',
    styleUrl: 'card-thumbnail.component.css',
    host: {
        'style': 'display: block; height: 72%; padding: 0.32rem;'
    }
})
export class CardThumbnailComponent {
    public readonly base64 = input.required<string|null>();
    public readonly imageIsLoading = computed<boolean>(
        () => !!this.base64()?.length
    );

    public alt = input<string>();
}
