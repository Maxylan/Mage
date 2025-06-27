import { Component, input, output } from '@angular/core';
import { ITag } from '../../core/types/generated/i-tag';
import { CardImageComponent } from './image.component';
import { MatIconButton } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatIcon } from '@angular/material/icon';

export type MenuEvent = [string, number];

@Component({
    selector: 'app-card',
    standalone: true,
    imports: [
        CardImageComponent,
        MatIconButton,
        MatMenuModule,
        MatIcon
    ],
    styleUrl: 'card.component.scss',
    templateUrl: 'card.component.html'
})
export class CardComponent {
    public readonly id = input.required<number>();
    public readonly title = input.required<string>();
    public readonly isHandset = input.required<boolean>();
    public readonly summary = input<string|null>(null);
    public readonly tags = input<ITag[]|null>(null);
    public readonly thumbnail = input<number|null>(null);

    public readonly menu = output<MenuEvent>();
    public readonly selectMode = input<boolean>();
    public readonly deselect = output<number>();
    public readonly select = output<number>();

    public readonly image = input<string>();
}
