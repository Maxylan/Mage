import { Injectable } from '@angular/core';
import ApiBase from '../../classes/base.class';
import { Account } from '../../types/generated/account';
import { Session } from 'inspector';
import { Login } from '../../types/generated/login';

export interface PrivilegeChecks {
    /** ..view any? */
    View(): boolean,
    /** ..view all? */
    ViewAll(): boolean,
    /** ..create? */
    Create(): boolean,
    /** ..delete? */
    Delete(): boolean,
    /** ..administrate? */
    Administrate(): boolean,
    /** ..match the given `privilege`? */
    CheckPrivilege(privilege: number): boolean
}

@Injectable({
    providedIn: 'root'
})
export class AuthService extends ApiBase {
    public static readonly VIEW = 0b00001;
    public static readonly VIEW_ALL = 0b00010;
    public static readonly CREATE = 0b00100;
    public static readonly DELETE = 0b01000;
    public static readonly ADMIN = 0b10000;

    /**
     * Is the given user privileged enough to..
     */
    public static Can(user: Account|null): PrivilegeChecks {
        return {
            /** ..view any? */
            View(): boolean {
                if (!user?.privilege || user.privilege < 0) {
                    return false;
                }
                return (AuthService.VIEW & user.privilege) === AuthService.VIEW;
            },
            /** ..view all? */
            ViewAll(): boolean {
                if (!user?.privilege || user.privilege < 0) {
                    return false;
                }
                return (AuthService.VIEW_ALL & user.privilege) === AuthService.VIEW_ALL;
            },
            /** ..create? */
            Create(): boolean {
                if (!user?.privilege || user.privilege < 0) {
                    return false;
                }
                return (AuthService.CREATE & user.privilege) === AuthService.CREATE;
            },
            /** ..delete? */
            Delete(): boolean {
                if (!user?.privilege || user.privilege < 0) {
                    return false;
                }
                return (AuthService.DELETE & user.privilege) === AuthService.DELETE;
            },
            /** ..administrate? */
            Administrate(): boolean {
                if (!user?.privilege || user.privilege < 0) {
                    return false;
                }
                return (AuthService.ADMIN & user.privilege) === AuthService.ADMIN;
            },
            /** ..match the given `privilege`? */
            CheckPrivilege(privilege: number): boolean {
                if (!user?.privilege || user.privilege < 0) {
                    return false;
                }
                return (privilege & user.privilege) === privilege;
            }
        }
    }

    /**
     * Validates that a session (..inferred from `<see cref="HttpContext"/>`) ..exists and is valid.
     * In other words this endpoint tests my Authentication Pipeline.
     *
     * [Authorize]
     * [HttpHead("validate")]
     */
    public async validateSession(): Promise<void> {
        return await this.head('/auth/')
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[validateSession] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Returns the `<see cref="Account"/>` tied to the requesting client's session (i.e, in our `<see cref="HttpContext"/>` pipeline).
     *
     * [Authorize]
     * [HttpGet("me")]
     */
    public async me(): Promise<Account> {
        return await this.get('/auth/me')
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[me] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Attempt to grab a full `<see cref="Session"/>` instance, identified by PK (uint) <paramref name="id"/>.
     * 
     * [Authorize]
     * [HttpGet("session/{id:int}")]
     */
    public async getSessionDetails(id: number): Promise<Session> {
        return await this.get('/auth/session/' + id)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getSessionDetails] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Attempt to grab a full `<see cref="Session"/>` instance, identified by unique <paramref name="session"/> code (string).
     * 
     * [Authorize]
     * [HttpGet("session/code/{session}")]
     */
    public async getSessionDetailsByCode(session: string): Promise<Session> {
        return await this.get('/auth/session/code/' + session)
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[getSessionDetailsByCode] Error!', err);
                    return err;
                }
            );
    }

    /**
     * Attempt to login a user, creating a new `<see cref="Session"/>` instance.
     * 
     * [HttpPost("login")]
     */
    public async login(credentials: Login): Promise<Session> {
        const body = JSON.stringify(credentials);

        return await this.post('/auth/login', { body })
            .then(res => res.json())
            .catch(
                err => {
                    console.error('[login] Error!', err);
                    return err;
                }
            );
    }
}
