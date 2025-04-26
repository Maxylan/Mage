import { Component, inject, signal } from '@angular/core';
import { BaseToolbarComponent } from '../../layout/toolbar/toolbar-base.component';

@Component({
    selector: 'page-albums',
    imports: [
        BaseToolbarComponent,
    ],
    templateUrl: 'tags.component.html',
    styleUrl: 'tags.component.css'
})
export class TagsPageComponent {
}
