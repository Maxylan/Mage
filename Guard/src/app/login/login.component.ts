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
    public static readonly API_URL: string = '/reception';

    private readonly breakpointObserver = inject(BreakpointObserver);
    private readonly matIconsRegistry = inject(MatIconRegistry);

    public failedLoginMessage = signal<string|null>(null);
    public usernameErrorState = signal<boolean>(false);
    public passwordErrorState = signal<boolean>(false);

    public usernameControl = new FormControl<string>('', { disable: false } as FormControlOptions);
    public passwordControl = new FormControl<string>('', { disable: false } as FormControlOptions);
    public rememberMeControl = new FormControl<boolean>(false, { disable: false } as FormControlOptions);


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


    public readonly loginForm = new FormGroup({
        username: this.usernameControl,
        password: this.passwordControl,
        rememberMe: this.rememberMeControl
    });


    constructor() {
        this.matIconsRegistry.registerFontClassAlias('hack', 'hack-nerd-font .hack-icons');
        this.toggleFormFields(true);

        const storedUserCredentials: string|null =
            localStorage.getItem('mage-stored-creds');

        if (storedUserCredentials) {
            const creds: HashedUserDetails =
                JSON.parse(storedUserCredentials);

            this.loginForm.patchValue({
                username: creds.username,
                rememberMe: true
            });

            this.sendLoginRequest(creds)
                .then(session => {
                    if (!session || typeof session !== 'object') {
                        console.debug('Failed to retrieve session!', session);
                        this.usernameErrorState.set(true); 
                        this.passwordErrorState.set(true);
                        return;
                    }

                    location.href = '/garden#@' + session.code;
                })
                .catch(this.handleErrors);
        }
    }

    public readonly isHandset$: Observable<boolean> =
        this.breakpointObserver
            .observe(Breakpoints.Handset)
            .pipe(
                map(result => result.matches),
                shareReplay()
            );


    public onLoginSubmit() {
        this.toggleFormFields(false);

        if (this.failedLoginMessage() !== null) {
            // Reset 'failed login' message, if one exists.
            this.failedLoginMessage.set(null);
        }

        setTimeout(
            () => this.login().finally(() => this.toggleFormFields(true)),
            256 // Surface-level spam prevention
        );
    }

    private async login(): Promise<void> {
        const sanitizedUsername: string|null = this.usernameControl.value
            ?.normalize()
            ?.trim() ?? null;

        if (!sanitizedUsername) {
            console.error('Username was falsy after sanitation!', sanitizedUsername);
            this.usernameErrorState.set(true); 
            this.toggleFormFields(true);
            return;
        }

        this.loginForm.patchValue({
            username: sanitizedUsername
        });

        const exceedsMaxLength: boolean = sanitizedUsername.length > 127;
        const belowMinLength: boolean = sanitizedUsername.length < 3;

        if (exceedsMaxLength || belowMinLength) {
            console.error('Username length invalid!', sanitizedUsername);
            this.usernameErrorState.set(true); 
            this.toggleFormFields(true);
            return;
        }

        if (!this.passwordControl.value) {
            this.passwordErrorState.set(true);
            this.toggleFormFields(true);
            console.error('Password value was falsy!');
            return;
        }

        const passwordExceedsMaxLength: boolean = this.passwordControl.value.length > 127;
        const passwordBelowMinLength: boolean = this.passwordControl.value.length < 4;

        if (passwordExceedsMaxLength || passwordBelowMinLength) {
            console.error('Password length invalid!', this.passwordControl.value.length);
            this.passwordErrorState.set(true); 
            this.toggleFormFields(true);
            return;
        }

        const encodedPassword = 
            new TextEncoder().encode(this.passwordControl.value);

        const hashedPassword =
            await crypto.subtle.digest('SHA-256', encodedPassword)
                .then((digest) => {
                    const hashedPassword: string = 
                        // Converts to hexadecimal (i.e base 16)
                        Array.from(new Uint8Array(digest))
                            .map(byte => byte.toString(16))
                            .join('');

                    return hashedPassword;
                });

        const userCreds: HashedUserDetails = {
            username: sanitizedUsername,
            hash: hashedPassword
        };

        const session =
            await this.sendLoginRequest(userCreds)
            .catch(this.handleErrors);

        if (!session || typeof session !== 'object') {
            console.debug('Failed to retrieve session!', session);
            return;
        }

        localStorage.setItem('mage-stored-usr', JSON.stringify(session));

        if (!!this.rememberMeControl.value) {
            console.debug('Successfully retrieved session! Storing credentials..', userCreds);
            localStorage.setItem('mage-stored-creds', JSON.stringify(userCreds));
        }
        else {
            console.debug('Successfully retrieved session!');
            localStorage.removeItem('mage-stored-creds');
        }

        location.href = '/garden#@' + session.code;
    }

    private async sendLoginRequest(creds: HashedUserDetails): Promise<Session> {
        const body = JSON.stringify(creds);

        return await fetch(AppLoginComponent.API_URL + '/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body 
        })
            .then(res => {
                if (!res || res.status != 200) {
                    this.usernameErrorState.set(true); 
                    this.passwordErrorState.set(true);
                    console.error('[sendLoginRequest] Falsy / Unsuccessfull fetch `Response`', res?.status);
                    return Promise.reject(res.body);
                }

                return res.json();
            })
            .then(parsed => {
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

                return session;
            });
    }

    private async handleErrors(error: Error): Promise<void> {
        console.error('Login failed!', error);

        if (error instanceof ReadableStream) {
            let processErrorResponse: string = '';
            const utf8Decoder = new TextDecoder('utf-8');
            const reader = error.getReader();

            function processText<T extends AllowSharedBufferSource>(
                { done, value }: ReadableStreamReadResult<T>
            ): Promise<string> {
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
            };

            await reader
                .read()
                .then(processText)
                .then(errorMessage => 
                    this.failedLoginMessage.set(errorMessage)
                );
        }
        else if (!!error) {
            const message = JSON.stringify(error, null, 4);
            this.failedLoginMessage.set(message);
        }
    }
}
