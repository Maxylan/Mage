import { Component, input, computed, effect, model } from '@angular/core';
import { MatProgressBar } from '@angular/material/progress-bar';
import { MatCardSmImage } from '@angular/material/card';

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
    public readonly file = input<File|null>(null);
    public readonly base64 = model<string|null>(null);

    public readonly imageIsLoading = computed<boolean>(
        () => !!this.base64()?.length
    );

    private readonly blob2baseEffect = effect(onCleanup => {
        const file = this.file();
        if (!file) {
            return;
        }

        if (!file.type.includes('image')) {
            console.warn('Invalid `file.type` (Content-Type)', file.type);
            return;
        }

        const transformPromise = file
            .arrayBuffer()
            .then(buf => {
                this.base64.set(
                    `data:${file.type};base64,` + // ..i wonder, at what point is @ts-ignore acceptable :p
                    (new Uint8Array(buf) as Uint8Array & { toBase64: () => string|null}).toBase64()
                );
            });

        onCleanup(
            async () => await transformPromise
        );
    });

    public alt = input<string>();
}
