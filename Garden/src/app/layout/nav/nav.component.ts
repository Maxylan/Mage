import { Component, effect, inject, input, Signal, viewChild } from '@angular/core';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { NavbarControllerService } from './nav-controller.service';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { navigation } from '../../app.routes';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'layout-nav',
    templateUrl: 'nav.component.html',
    styleUrl: 'nav.component.css',
    standalone: true,
    imports: [
        MatSidenavModule,
        MatToolbarModule,
        MatButtonModule,
        MatListModule,
        RouterLink
    ]
})
export class LayoutNavComponent {
    private navController = inject(NavbarControllerService);
    public readonly navigationLinks = navigation;

    private readonly navRef: Signal<MatSidenav> = viewChild.required(MatSidenav);
    private readonly navEffect = effect(
        () => this.navController.initialize(this.navRef())
    );

    public readonly isHandset = input.required<boolean>();
}
