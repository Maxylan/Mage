import { Component, effect, EventEmitter, Input, Output, Signal, signal, WritableSignal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'shared-search-bar',
    imports: [
        ReactiveFormsModule,
        MatFormFieldModule,
        MatButtonModule,
        MatIconModule,
        MatInput
    ],
    templateUrl: 'search-bar.component.html',
    styleUrl: 'search-bar.component.scss',
    /* host: {
        style: "display: inline-block; margin: 0px auto;"
    } */
})
export class SearchBarComponent {
    @Input({ required: true })
    public formName!: string;

    @Input()
    public placeholder: string = 'Search for ' + this.formName;

    @Input()
    public value: string = '';

    @Output()
    public onSearch$: EventEmitter<SubmitEvent> = new EventEmitter<SubmitEvent>();

    public searchControl = new FormControl<string>(this.value);
    public searchForm = new FormGroup({
        keyword: this.searchControl,
    });
}
