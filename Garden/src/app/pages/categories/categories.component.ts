import { Component, inject, signal } from '@angular/core';
import { BaseToolbarComponent } from '../../layout/toolbar/toolbar-base.component';

@Component({
    selector: 'page-categories',
    imports: [
        BaseToolbarComponent,
    ],
    templateUrl: 'categories.component.html',
    styleUrl: 'categories.component.css'
})
export class CategoriesPageComponent {
}
