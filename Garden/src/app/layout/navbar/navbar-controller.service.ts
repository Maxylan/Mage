import { Injectable, Signal, viewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';

@Injectable({
    providedIn: 'root'
})
export class NavbarControllerService {
    private matSidenavRef?: MatSidenav; 
    public initialize = (ref?: MatSidenav|undefined): void => {
        this.matSidenavRef = ref;
    } 

    public getNavbar = () => this.matSidenavRef;

    constructor() { } 
}
