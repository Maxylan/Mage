import { Component, input, computed } from '@angular/core';
import { MatProgressBar } from '@angular/material/progress-bar';

@Component({
    selector: 'shared-card-thumbnail',
    imports: [
        MatProgressBar
    ],
    templateUrl: 'card-thumbnail.component.html',
    styleUrl: 'card-thumbnail.component.css'
})
export class CardThumbnailComponent {
    public readonly base64 = input.required<string|null>();
    public readonly imageIsLoading = computed<boolean>(
        () => !!this.base64()?.length
    );

    public alt = input<string>();
}
