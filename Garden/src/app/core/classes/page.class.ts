import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { computed, inject, Signal, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, Observable, shareReplay } from 'rxjs';

export default abstract class PageBase {
    private readonly breakpointObserver = inject(BreakpointObserver);

    public readonly navbarOpen = signal<boolean>(false);
    public readonly isLoading = signal<boolean>(false);
    public abstract readonly isEmpty: Signal<boolean>;

    /**
     * Signal/Breakpoint for a mobile/handset-viewport.
     */
    public readonly isHandset = toSignal(
        this.breakpointObserver
            .observe(Breakpoints.Handset)
            .pipe(
                map(result => result.matches),
                shareReplay()
            ),
        { initialValue: true }
    );

    public abstract refetch(): Promise<void>;
}
