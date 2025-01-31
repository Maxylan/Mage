import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PostFormComponent } from './post-form/post-form.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, PostFormComponent],
  template: `
    <h1>Welcome to {{title}}!</h1>
    <router-outlet />
    <post-form-component />
  `,
  styles: [],
})

export class AppComponent {
  title = 'Garden';
}
