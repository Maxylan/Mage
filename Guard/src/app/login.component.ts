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
import { HashedUserDetails, LoginBody, Session } from './login.types';
import { MatCheckbox } from '@angular/material/checkbox';

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
    MatCheckbox,
    MatFormField,
    ReactiveFormsModule,
    AsyncPipe,
    NgClass
  ]
})
export class AppLoginComponent {
    private breakpointObserver = inject(BreakpointObserver);
    private matIconsRegistry = inject(MatIconRegistry);
    private apiUrl: string = '/reception';

    public usernameErrorState = signal<boolean>(false);
    public passwordErrorState = signal<boolean>(false);

    public usernameControl = new FormControl('');
    public passwordControl = new FormControl('');
    public rememberMeControl = new FormControl('');

    public disableFormFields: boolean = true;
    public loginForm = new FormGroup({
        username: this.usernameControl,
        password: this.passwordControl,
        rememberMe: this.rememberMeControl
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
            const sanitizedUsername: string|null = this.usernameControl.value
                ?.normalize()
                ?.trim() ?? null;

            if (!sanitizedUsername) {
                console.error('[onLoginSubmit] Username was falsy after sanitation!', sanitizedUsername);
                this.usernameErrorState.set(true); 
                this.disableFormFields = false;
                return;
            }

            const usernameMaxLength: boolean = sanitizedUsername.length > 127;
            const usernameMinLength: boolean = sanitizedUsername.length < 3;

            if (!usernameMaxLength || !usernameMinLength) {
                console.error('[onLoginSubmit] Username length invalid!', sanitizedUsername);
                this.usernameErrorState.set(true); 
                this.disableFormFields = false;
                return;
            }

            if (!this.passwordControl.value) {
                this.passwordErrorState.set(true);
                this.disableFormFields = false;
                console.error('[onLoginSubmit] Password value was falsy!');
                return;
            }

            const passwordMaxLength: boolean = this.passwordControl.value.length > 127;
            const passwordMinLength: boolean = this.passwordControl.value.length < 4;

            if (!passwordMaxLength || !passwordMinLength) {
                console.error('[onLoginSubmit] Password length invalid!', this.passwordControl.value.length);
                this.passwordErrorState.set(true); 
                this.disableFormFields = false;
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
                .then(
                    (hashedCredentials) => this.sendLoginRequest(
                        hashedCredentials[0],
                        hashedCredentials[1]
                    )
                )
                .then(
                    (session) => {
                        console.debug(
                            '[onLoginSubmit] Successfully retrieved session!',
                            this.rememberMeControl.value
                        );

                        if (!!this.rememberMeControl.value) {
                            localStorage.setItem('mage-stored-usr', JSON.stringify(session));
                        }

                        return Promise.resolve(session);
                    }
                )
                .catch()
                .finally(() => {
                    this.disableFormFields = false;
                })
        }, 333); // Surface-level spam prevention
        // In practice a `setTimeout` is easily circumvented, but my backend
        // has spam-protection so this is just an added precaution.
    }

    sendLoginRequest(hashedUsername: string, hashedPassword: string): Promise<Session> {
        const requestBody: LoginBody = {
            username: hashedUsername,
            password: hashedPassword
        };

        return fetch(this.apiUrl, {
            method: 'POST',
            body: JSON.stringify(requestBody)
        })
            .then(
                res => {
                    if (!res || res.status != 200) {
                        this.usernameErrorState.set(true); 
                        this.passwordErrorState.set(true);
                        console.error('[sendLoginRequest] Falsy / Unsuccessfull fetch `Response`', res?.status);
                        return Promise.reject(res.body);
                    }

                    return res.json();
                }
            )
            .then(
                parsed => {
                    const session: Session = parsed;
                    if (!session.id ||
                        session.id < 1 ||
                        !session.userId ||
                        session.userId < 1
                    ) {
                        console.error('[sendLoginRequest] Session Response missing / invalid IDs', session.id, session.userId);
                        return Promise.reject('Session Response missing / invalid IDs');
                    }

                    if (!session.code || session.code.length !== 36) {
                        console.error('[sendLoginRequest] Session `code` missing / invalid', session.code);
                        return Promise.reject('Session `code` missing / invalid');
                    }

                    return Promise.resolve(session);
                }
            )
            .catch(
                err => Promise.reject(err)
            );
    }
}
