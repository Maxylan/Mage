import { Component, inject, signal } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { AsyncPipe, NgClass } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { HashedUserDetails } from './login.types';

@Component({
  selector: 'app-login',
  templateUrl: 'login.html',
  styleUrl: 'login.css',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    MatLabel,
    MatInput,
    MatFormField,
    ReactiveFormsModule,
    AsyncPipe,
    NgClass
  ]
})
export class AppLoginComponent {
    private breakpointObserver = inject(BreakpointObserver);
    private matIconsRegistry = inject(MatIconRegistry);

    public usernameErrorState = signal<boolean>(false);
    public passwordErrorState = signal<boolean>(false);

    public usernameControl = new FormControl('');
    public passwordControl = new FormControl('');

    public disableFormFields: boolean = true;
    public loginForm = new FormGroup({
        username: this.usernameControl,
        password: this.passwordControl
    });

    private userDetails?: HashedUserDetails;

    constructor() {
        this.matIconsRegistry.registerFontClassAlias('hack', 'hack-nerd-font .hack-icons');
        this.disableFormFields = false;
    }

    isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );

    onLoginSubmit() {
        this.disableFormFields = true;
        if (!this.userDetails) {
            this.userDetails = {
                username: '',
                password: ''
            };
        }

        setTimeout(() => {
            const sanitizedUsername = this.usernameControl.value
                ?.normalize()
                ?.trim();

            if (!sanitizedUsername)
            {
                this.usernameErrorState.set(true); 
                this.disableFormFields = false;
                console.error('[onLoginSubmit] Username sanitation/validation error! Value was falsy after sanitation!', sanitizedUsername);
                return;
            }

            if (!this.passwordControl.value) {
                this.passwordErrorState.set(true);
                this.disableFormFields = false;
                console.error('[onLoginSubmit] Password validation error! Value was falsy!');
                return;
            }

            const encodedUsername = 
                new TextEncoder().encode(sanitizedUsername);
            const encodedPassword = 
                new TextEncoder().encode(this.passwordControl.value);

            Promise
                .all([
                    crypto.subtle.digest('SHA-256', encodedUsername),
                    crypto.subtle.digest('SHA-256', encodedPassword),
                ])
                .then((credentials) => {
                    const hashedCredentials: string[] = credentials
                        .map((buffer: ArrayBuffer) => {
                            return Array.from(new Uint8Array(buffer))
                            // Converts to hexadecimal (i.e base 16)
                            .map(byte => byte.toString(16))
                            .join('');
                        });

                    return Promise.resolve(hashedCredentials);
                })
                .catch()
                .finally(() => {
                    this.disableFormFields = false;
                })
        }, 666); // Surface-level spam prevention
        // In practice a `setTimeout` is easily circumvented, but my backend
        // has spam-protection so this is just an added precaution.
    }

    sendLoginRequest() {
    }
}
