import { Component, inject, signal } from '@angular/core';
import { BaseToolbarComponent } from '../../layout/toolbar/toolbar-base.component';

@Component({
    selector: 'page-albums',
    imports: [
        BaseToolbarComponent,
    ],
    templateUrl: 'albums.component.html',
    styleUrl: 'albums.component.css'
})
export class AlbumsPageComponent {
}
