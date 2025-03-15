import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UploadFormComponent } from './layout/upload-form/upload-form.component';
import { LayoutNavbarComponent } from './layout/navbar/navbar.component';
import { MatIconRegistry } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  imports: [
      RouterOutlet,
      UploadFormComponent,
      LayoutNavbarComponent
  ],
  template: `
    <layout-navbar>
        <router-outlet />
        <upload-form />
    </layout-navbar>
  `,
  styles: [],
})

export class AppComponent {
    private matIconsRegistry = inject(MatIconRegistry);

    constructor() {
        this.matIconsRegistry.registerFontClassAlias('hack', 'hack-icons mat-ligature-font');
    }
}
