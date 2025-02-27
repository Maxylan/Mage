import { Component, effect, inject, Signal, viewChild } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AsyncPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconRegistry } from '@angular/material/icon';
import { map, shareReplay } from 'rxjs/operators';
import { navigation } from '../../app.routes';
import { Observable } from 'rxjs';
import { NavbarControllerService } from './navbar-controller.service';
import { MatToolbar } from '@angular/material/toolbar';

@Component({
  selector: 'layout-navbar',
  templateUrl: 'navbar.html',
  styleUrl: 'navbar.css',
  standalone: true,
  imports: [
    MatButtonModule,
    MatSidenavModule,
    MatListModule,
    MatToolbar,
    AsyncPipe,
  ]
})
export class LayoutNavbarComponent {
    private navbarController = inject(NavbarControllerService);
    private breakpointObserver = inject(BreakpointObserver);
    private matIconsRegistry = inject(MatIconRegistry);

    private navbarRef: Signal<MatSidenav> = viewChild.required(MatSidenav);
    private navbarEffect = effect(
        () => this.navbarController.initialize(this.navbarRef())
    );

    navigationLinks = navigation;

    constructor() {
        this.matIconsRegistry.registerFontClassAlias('hack', 'hack-nerd-font .hack-icons');
    }

    isOpened: boolean = false;

    isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );
}
