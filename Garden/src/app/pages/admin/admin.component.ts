import { Component, inject, signal } from '@angular/core';
import { BaseToolbarComponent } from '../../layout/toolbar/toolbar-base.component';

@Component({
    selector: 'page-admin',
    imports: [
        BaseToolbarComponent,
    ],
    templateUrl: 'admin.component.html',
    styleUrl: 'admin.component.css'
})
export class AdminPageComponent {
}
