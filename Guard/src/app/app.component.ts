import { Component } from '@angular/core';
import { AppLoginComponent } from "./login/login.component";

@Component({
    selector: 'app-root',
    imports: [AppLoginComponent],
    template: `
        <app-login/>
    `,
    styles: `
        app-login {
            display: flex;
            height: 100%;
        }
    `
})
export class AppComponent {
    title = 'Guard';
}
