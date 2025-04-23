import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UploadFormComponent } from './layout/upload-form/upload-form.component';
import { LayoutNavComponent } from './layout/nav/nav.component';
import { MatIconRegistry } from '@angular/material/icon';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, shareReplay } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [
      RouterOutlet,
      UploadFormComponent,
      LayoutNavComponent
  ],
  template: `
    <layout-nav [isHandset]="isHandset()">
        <router-outlet/>
        <upload-form />
    </layout-nav>
  `,
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
