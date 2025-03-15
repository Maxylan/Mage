import { Component, effect, inject, Signal, viewChild } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AsyncPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { NavbarControllerService } from './navbar-controller.service';
import { MatToolbar } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { map, shareReplay } from 'rxjs/operators';
import { navigation } from '../../app.routes';
import { Observable } from 'rxjs';

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

    private navbarRef: Signal<MatSidenav> = viewChild.required(MatSidenav);
    private navbarEffect = effect(
        () => this.navbarController.initialize(this.navbarRef())
    );

    public navigationLinks = navigation;
    public isOpened: boolean = false;
    public isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );
}
