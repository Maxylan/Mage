import { Injectable, Signal, signal } from '@angular/core';
import { HashedUserDetails } from '../../types/auth';
import { Session } from '../../types/generated/session';
import ApiBase from '../classes/base.class';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    public static readonly HEADER = 'x-mage-token';
    public static readonly STORED_SESSION = 'mage-stored-usr';
    public static readonly STORED_CREDENTIALS = 'mage-stored-creds';

    public readonly isLoading = signal<boolean>(false);
    protected readonly sessionToken = signal<string|null>(
        localStorage.getItem(AuthService.STORED_SESSION)
    );

    protected readonly storedCredentials = signal<string|null>( // HashedUserDetails
        localStorage.getItem(AuthService.STORED_CREDENTIALS)
    );

    /**
     * Get the token.
     */
    public get getToken(): Signal<string|null> {
        // Attempt to consume one from an URL, probably won't be successfully, but doesn't hurt.
        this.consumeToken();

        const token = this.sessionToken();
        if (!token) {
            const localStorageSession = 
                localStorage.getItem(AuthService.STORED_SESSION);

            if (localStorageSession) {
                this.sessionToken.set(localStorageSession);
            }
        }

        return this.sessionToken.asReadonly();
    }

    /**
     * Authorize the user, either via stored credentials (remember-me), or via redirection to 'Guard' (login)
     */
    public async authorize(): Promise<string|null> {
        // TODO - Check to see if we have a stored session already, and if that session is valid?
        // Not strictly necessary, but would prevent some potentially unecessary re-logins.

        let localStorageCreds = this.storedCredentials();
        if (!localStorageCreds) {
            localStorageCreds = 
                localStorage.getItem(AuthService.STORED_CREDENTIALS);

            if (!localStorageCreds) {
                this.fallbackToAuth();
                return null;
            }

            this.storedCredentials.set(localStorageCreds);
        }

        const creds: HashedUserDetails = JSON.parse(localStorageCreds);

        if (!creds.username || !creds.hash) {
            console.warn('[authorize] Stored credentials are invalid!', creds);
            localStorage.removeItem(AuthService.STORED_CREDENTIALS);
            return null;
        }

        this.isLoading.set(true);

        // TODO! Remove log
        console.debug('[AuthService] Attempting auto-refresh (remember me)', creds);

        const session = await this.refreshLogin(creds.username, creds.hash)
            .catch(this.handleErrors);

        return session?.code || null;
    }

    /**
     * Fallback on a redirect to 'Guard' to have the user re-authorize (login)
     */
    public fallbackToAuth(failedResponse?: Response) {
        console.debug('[AuthService] Falling-back to \'Guard\'..');
        this.isLoading.set(true);

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
    private async refreshLogin(username: string, hashedPassword: string): Promise<Session> {
        if (this.isLoading() === false) {
            this.isLoading.set(true);
        }

        return await fetch(ApiBase.API_URL + '/auth/login', {
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
                        !session.accountId ||
                        session.accountId < 1
                    ) {
                        console.error('[AuthService] (sendLoginRequest) Session Response missing / invalid IDs', session.id, session.accountId);
                        return Promise.reject('Session Response missing / invalid IDs');
                    }

                    if (!session.code || session.code.length !== 36) {
                        console.error('[AuthService] (sendLoginRequest) Session `code` missing / invalid', session.code);
                        return Promise.reject('Session `code` missing / invalid');
                    }

                    localStorage.setItem(AuthService.STORED_SESSION, session.code!);
                    this.sessionToken.set(session.code);

                    return session;
                }
            )
            .finally(
                () => this.isLoading.set(false)
            );
    }

    private async handleErrors(error: Error): Promise<void> {
        console.error('Login / Auto-refresh failed!', error);

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
                    // TODO! Growl? Alert?
                    console.error(errorMessage)
                );
        }
        else if (!!error) {
            const message = JSON.stringify(error, null, 4);
            // TODO! Growl? Alert?
            console.error(message);
        }
    }

    /**
     * Attempt to consume a token/session-code from a URL hash-value
     */
    public consumeToken(): void {
        if (!location.hash) {
            return;
        }
        const [
            url,
            token
        ] = location.href.split('#');

        if (token) {
            this.sessionToken.set(
                token.replace(/^#?@?/, '')
            );

            window.history.replaceState(null, '', url);
        }
    }

    constructor() {
        this.consumeToken();
    }
}
