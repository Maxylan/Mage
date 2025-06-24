import { Component, inject, model } from '@angular/core';
import { NavbarControllerService } from '../../layout/nav/nav-controller.service';
import { MatButtonModule, MatIconButton } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { SelectionObserver } from './selection-observer.service';
import { MatChip } from '@angular/material/chips';

@Component({
    selector: 'toolbar-base',
    imports: [
        MatToolbarModule,
        MatButtonModule,
        MatIconModule,
        MatIconButton,
        MatChip
    ],
    templateUrl: 'toolbar-base.component.html',
    styleUrl: 'toolbar-base.component.scss'
})
export class BaseToolbarComponent {
    private readonly navbarController = inject(NavbarControllerService);
    private readonly selectionObserver = inject(SelectionObserver);

    public readonly getNavbar = this.navbarController.getNavbar;
    public readonly selectionState = this.selectionObserver.State;
    public readonly quitSelectMode = () => setTimeout(
        () => this.selectionObserver.setSelectionMode(false),
        64
    );

    /**
     * Output invoked when the navbar's open state changes.
     */
    public readonly navbarEvent = model<boolean>(false, { alias: 'onNavbarToggle' });

    public readonly toggleNavbar = async (clickEvent: Event|null = null) => {
        this.navbarEvent.set(!this.getNavbar()?.opened);
        return this.getNavbar()?.toggle()
    }

    public ngOnInit() {
        this.navbarEvent.set(!!this.getNavbar()?.opened);
    }
}
