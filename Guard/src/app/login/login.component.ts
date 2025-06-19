import { FormControl, FormControlOptions, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Component, inject, signal } from '@angular/core';
import { AsyncPipe, NgClass } from '@angular/common';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckbox } from '@angular/material/checkbox';
import { MatInput } from '@angular/material/input';
import {
    HashedUserDetails,
    LoginBody,
    Session,
    Account,
    Client
} from './types';

@Component({
  selector: 'app-login',
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
  ],
  templateUrl: 'login.component.html',
  styleUrl: 'login.component.css'
})
export class AppLoginComponent {
    public static readonly : string = '/reception';

    private breakpointObserver = inject(BreakpointObserver);
    private matIconsRegistry = inject(MatIconRegistry);
    private apiUrl: string = '/reception';

    public failedLoginMessage = signal<string|null>(null);
    public usernameErrorState = signal<boolean>(false);
    public passwordErrorState = signal<boolean>(false);

    public usernameControl = new FormControl<string>('', { disable: false } as FormControlOptions);
    public passwordControl = new FormControl<string>('', { disable: false } as FormControlOptions);
    public rememberMeControl = new FormControl<string>('', { disable: false } as FormControlOptions);

    public toggleFormFields(state?: boolean): void {
        if (state === undefined) {
            state = (
                this.usernameControl.disabled &&
                this.passwordControl.disabled &&
                this.rememberMeControl.disabled
            );
        }
        switch(state) {
            case true:
                this.usernameControl.enable();
                this.passwordControl.enable();
                this.rememberMeControl.enable();
                break;
            case false:
                this.usernameControl.disable();
                this.passwordControl.disable();
                this.rememberMeControl.disable();
                break;
        }
    };

    public loginForm = new FormGroup({
        username: this.usernameControl,
        password: this.passwordControl,
        rememberMe: this.rememberMeControl
    });

    private userDetails?: HashedUserDetails;

    constructor() {
        this.matIconsRegistry.registerFontClassAlias('hack', 'hack-nerd-font .hack-icons');
        this.toggleFormFields(true);

        localStorage.getItem('');
    }

    isHandset$: Observable<boolean> = this.breakpointObserver
        .observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );

    onLoginSubmit() {
        this.toggleFormFields(false);
        if (!this.userDetails) {
            this.userDetails = {
                username: '',
                hash: ''
            };
        }

        if (this.failedLoginMessage() !== null) {
            // Reset 'failed login' message, if one exists.
            this.failedLoginMessage.set(null);
        }

        setTimeout(() => {
            const sanitizedUsername: string|null = this.usernameControl.value
                ?.normalize()
                ?.trim() ?? null;

            if (!sanitizedUsername) {
                console.error('[onLoginSubmit] Username was falsy after sanitation!', sanitizedUsername);
                this.usernameErrorState.set(true); 
                this.toggleFormFields(true);
                return;
            }

            this.userDetails!.username = sanitizedUsername;

            const exceedsMaxLength: boolean = sanitizedUsername.length > 127;
            const belowMinLength: boolean = sanitizedUsername.length < 3;

            if (exceedsMaxLength || belowMinLength) {
                console.error('[onLoginSubmit] Username length invalid!', sanitizedUsername);
                this.usernameErrorState.set(true); 
                this.toggleFormFields(true);
                return;
            }

            if (!this.passwordControl.value) {
                this.passwordErrorState.set(true);
                this.toggleFormFields(true);
                console.error('[onLoginSubmit] Password value was falsy!');
                return;
            }

            const passwordExceedsMaxLength: boolean = this.passwordControl.value.length > 127;
            const passwordBelowMinLength: boolean = this.passwordControl.value.length < 4;

            if (passwordExceedsMaxLength || passwordBelowMinLength) {
                console.error('[onLoginSubmit] Password length invalid!', this.passwordControl.value.length);
                this.passwordErrorState.set(true); 
                this.toggleFormFields(true);
                return;
            }

            const encodedPassword = 
                new TextEncoder().encode(this.passwordControl.value);
            /*
            const encodedUsername = 
                new TextEncoder().encode(sanitizedUsername);
            Promise
                .all([
                    crypto.subtle.digest('SHA-256', encodedUsername),
                    crypto.subtle.digest('SHA-256', encodedPassword),
                ])
            */
            crypto.subtle.digest('SHA-256', encodedPassword)
                .then((digest) => {
                    const hashedPassword: string = 
                        // Converts to hexadecimal (i.e base 16)
                        Array.from(new Uint8Array(digest))
                            .map(byte => byte.toString(16))
                            .join('');

                    this.userDetails!.hash = hashedPassword;
                    return Promise.resolve(hashedPassword);
                })
                .then(
                    (hashedPassword) => this.sendLoginRequest(
                        sanitizedUsername,
                        hashedPassword
                    )
                )
                .then(
                    (session) => {
                        console.debug(
                            '[onLoginSubmit] Successfully retrieved session!',
                            this.rememberMeControl.value
                        );

                        localStorage.setItem('mage-stored-usr', JSON.stringify(session));

                        if (!!this.rememberMeControl.value) {
                            localStorage.setItem('mage-stored-creds', JSON.stringify(this.userDetails));
                        }

                        return Promise.resolve(session);
                    }
                )
                .then(
                    (session) => location.href = '/garden#@' + session.code
                )
                .catch(
                    err => {
                        if (err instanceof ReadableStream) {
                            const utf8Decoder = new TextDecoder('utf-8');
                            const reader = err.getReader();

                            let processErrorResponse = '';
                            reader
                                .read()
                                .then(function processText({ done, value }): Promise<any> {
                                    processErrorResponse += utf8Decoder.decode(value);

                                    if (done) {
                                        return Promise.resolve(
                                            processErrorResponse
                                                .trim()
                                                .replace(/^[\\"]{0,3}(.*)/, '$1')
                                                .replace(/[\\"]*$/, '')
                                        );
                                    }

                                    return reader
                                        .read()
                                        .then(processText);
                                })
                                .then(errorMessage => {
                                    console.error('[onLoginSubmit] Login failed!', errorMessage);
                                    this.failedLoginMessage.set(
                                        JSON.stringify(errorMessage, null, 4)
                                    );
                                });
                        }
                        else {
                            console.error('[onLoginSubmit] Login failed!', err);
                            if (!!err) {
                                this.failedLoginMessage.set(JSON.stringify(err, null, 4));
                            }
                        }
                    }
                )
                .finally(() => {
                    this.toggleFormFields(true);
                })
        }, 256); // Surface-level spam prevention
        // In practice a `setTimeout` is easily circumvented, but my backend
        // has spam-protection, so this is just a small added precaution.
    }

    sendLoginRequest(username: string, hashedPassword: string): Promise<Session> {
        const requestBody: LoginBody = {
            username: username,
            hash: hashedPassword
        };

        return fetch(this.apiUrl + '/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
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
                        !session.accountId ||
                        session.accountId < 1
                    ) {
                        console.error('[sendLoginRequest] Session Response missing / invalid IDs', session.id, session.accountId);
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
