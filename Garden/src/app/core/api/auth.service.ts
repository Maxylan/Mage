import { Injectable, signal } from '@angular/core';
import { RefreshCredentials, Session } from '../types/generic.types';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private apiUrl: string = '/reception';
    private isAuthLoading = signal<boolean>(true);
    private storedCredentials: RefreshCredentials|null = null;
    private storedSession: Session|null = null;
    private code: string|null = null;

    /**
     * Get the hardcoded (..or configured?) API Url
     */
    public getApiUrl = (): string => this.apiUrl;
    /**
     * Get the token.
     */
    public getToken = (): string|null => {
        // Attempt to consume one from an URL, probably won't be successfully, but doesn't hurt.
        this.consumeToken();

        if (this.code === null || this.storedSession === null) {
            const localStorageSession = localStorage.getItem('mage-stored-usr');
            if (localStorageSession) {
                this.storedSession = JSON.parse(localStorageSession);
            }
        }

        return (this.code || this.storedSession?.code) ?? null;
    };

    /**
     * Attempt to consume a token/session-code from a URL hash-value
     */
    public consumeToken = (): void => {
        if (!location.hash) {
            return;
        }

        if (location.hash.startsWith('#@') && location.hash.length === 38) {
            this.code = location.hash.substring(2);
            // location.hash = '';
            window.history.replaceState(
                {},
                document.title,
                location.href.split('#')[0]
            );
        }
    }

    /**
     * Authorize the user, either via stored credentials (remember-me), or via redirection to 'Guard' (login)
     */
    public authorize = (): Promise<string|null> => {
        this.isAuthLoading.set(true);

        if (!this.storedCredentials) {
            const storedCredentials = localStorage.getItem('mage-stored-creds');
            if (storedCredentials) {
                this.storedCredentials = JSON.parse(storedCredentials);
            }
        }

        if (!this.storedCredentials) {
            this.fallbackToAuth();
            return Promise.resolve(
                this.getToken()
            );
        }

        console.debug('[AuthService] Attempting auto-refresh (remember me)');
        return this.refreshLogin(this.storedCredentials.username, this.storedCredentials.hash)
            .then(session => {
                this.storedSession = session;
                this.code = session.code;
                return Promise.resolve(
                    this.getToken()
                );
            })
            .catch(err => {
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
                            console.error('[AuthService] Login Refresh failed!', errorMessage);
                        });
                }
                else {
                    console.error('[AuthService] Login Refresh failed!', err);
                }

                this.fallbackToAuth(err);
                return Promise.resolve(err);
            });
    }

    /**
     * Fallback on a redirect to 'Guard' to have the user re-authorize (login)
     */
    public fallbackToAuth = (failedResponse?: Response|any) => {
        console.debug('[AuthService] Falling-back to \'Guard\'..');
        this.isAuthLoading.set(true);

        if (failedResponse && failedResponse instanceof Response) {
            console.warn(`[AuthService] Failed attempt to re-authorize (${failedResponse.status}, ${failedResponse.statusText})`);
        }
        else if (failedResponse !== undefined) {
            console.warn('[AuthService] Failed to authorize user!', failedResponse);
        }

        location.href = '/guard';
        return;
    }

    /**
     * Refresh logins for people who have their credentials stored in their browser (remember-me)
     */
    private refreshLogin(username: string, hashedPassword: string): Promise<Session> {
        if (this.isAuthLoading() === false) {
            this.isAuthLoading.set(true);
        }

        return fetch(this.getApiUrl() + '/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                hash: hashedPassword
            })
        })
            .then(
                res => {
                    if (!res || res.status != 200) {
                        console.error('[AuthService] Falsy / Unsuccessfull fetch `Response`', res?.status);
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
                        console.error('[AuthService] (sendLoginRequest) Session Response missing / invalid IDs', session.id, session.userId);
                        return Promise.reject('Session Response missing / invalid IDs');
                    }

                    if (!session.code || session.code.length !== 36) {
                        console.error('[AuthService] (sendLoginRequest) Session `code` missing / invalid', session.code);
                        return Promise.reject('Session `code` missing / invalid');
                    }

                    return Promise.resolve(session);
                }
            )
            .finally(
                () => this.isAuthLoading.set(false)
            );
    }

    constructor() {
        this.consumeToken();
    }

    ngInit = () => {
        this.consumeToken();
    }
}
