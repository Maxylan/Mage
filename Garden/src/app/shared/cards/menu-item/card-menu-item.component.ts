import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'shared-card-menu-item',
    imports: [
        MatIconModule
    ],
    templateUrl: 'card-menu-item.component.html',
    styleUrl: 'card-menu-item.component.css'
})
export class CardMenuItemComponent {
    public readonly callback = output<string>({ alias: 'onClick' });
    public readonly key = input.required<string>();
    public readonly text = input.required<string>();

    public readonly icon = input<string>(); // '󰒖'
    public readonly iconSelected = input<string>(); // '󱇴'
    public readonly label = input<string>('Card Menu Item');
    public readonly labelSelected = input<string>();
    public readonly textSelected = input<string>();
    public readonly disabled = input<boolean>(false);
    public readonly isSelected = input<boolean>();
}
