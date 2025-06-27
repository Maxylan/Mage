import { Component, inject } from '@angular/core';
import { UploadFormContainerComponent } from './layout/upload-form/upload-images.component';
import { BaseToolbarComponent } from './layout/toolbar/toolbar-base.component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { LayoutNavComponent } from './layout/nav/nav.component';
import { MatIconRegistry } from '@angular/material/icon';
import { RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, shareReplay } from 'rxjs';

@Component({
    selector: 'app-root',
    imports: [
        UploadFormContainerComponent,
        BaseToolbarComponent,
        LayoutNavComponent,
        RouterOutlet
    ],
    templateUrl: 'app.component.html',
    styles: [],
})

export class AppComponent {
    private readonly matIconsRegistry = inject(MatIconRegistry);
    private readonly breakpointObserver = inject(BreakpointObserver);

    private readonly isHandsetObservable$ =
        this.breakpointObserver
            .observe(Breakpoints.Handset)
            .pipe(
                map(result => result.matches),
                shareReplay()
            );

    public readonly isHandset = toSignal(
        this.isHandsetObservable$, { initialValue: true }
    );

    constructor() {
        this.matIconsRegistry.registerFontClassAlias('hack', 'hack-icons mat-ligature-font');
    }
}
