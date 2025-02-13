import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class AuthService
{
    public fallbackToAuth = function(failedResponse?: Response|any) {
        if (failedResponse && failedResponse instanceof Response) {
            console.warn(`[AuthService] Call to fallback to auth made (${failedResponse.status}, ${failedResponse.statusText}), redirecting..`);
        }
        else {
            console.warn('[AuthService] Call to fallback on auth made, redirecting..', failedResponse);
        }

        location.href = '/guard';
    }

    constructor() { }
}
