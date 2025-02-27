import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UploadFormComponent } from './layout/upload-form/upload-form.component';
import { LayoutNavbarComponent } from './layout/navbar/navbar.component';

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
    </layout-navbar>
    <upload-form />
  `,
  styles: [],
})

export class AppComponent {}
