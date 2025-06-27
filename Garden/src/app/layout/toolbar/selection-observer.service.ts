import { computed, Injectable, WritableSignal, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';

export type CardSelectionState = {
    isSelected: boolean,
    isInSelectMode: boolean,
    select: () => void
};
export type SelectionState = {
    selectModeActive: boolean,
    selection: (string|number)[]
};

@Injectable({
    providedIn: 'root'
})
export class SelectionObserver {
    private selection: WritableSignal<(string|number)[]> = signal<(string|number)[]>([]);
    private selectMode: WritableSignal<boolean> = signal(false);
    private observable?: Observable<SelectionState>;

    public State = computed(() => ({
        selectModeActive: this.selectMode(),
        selection: this.selection()
    }));

    public ngOnInit() {
        if (!this.observable) {
            this.observable = toObservable(this.State);
        }
    }

    public observe = (): Observable<SelectionState> => {
        if (!this.observable) {
            this.observable = toObservable(this.State);
        }

        return this.observable;
    }

    public subscribe: Observable<SelectionState>['subscribe'] = this.observe().subscribe;
    public forEach: Observable<SelectionState>['forEach'] = this.observe().forEach;
    public pipe: Observable<SelectionState>['pipe'] = this.observe().pipe;
    public lift: Observable<SelectionState>['lift'] = this.observe().lift;

    public isSelecting = (): boolean =>
        this.selectMode();

    public setSelectionMode = (active: boolean): void => {
        if (!active) {
            this.selection.set([]);
        }

        this.selectMode.set(active);
    }

    public toggleSelectionMode = (): void =>
        this.selectMode.update(prev => {
            if (prev) {
                this.selection.set([]);
            }

            return !prev;
        });

    public getSelectedItems = (): (string|number)[] =>
        this.selection();

    public setSelectedItems = (items: (string|number)[]): void => {
        if ((!items || items.length === 0) && this.selectMode()) {
            this.setSelectionMode(false);
            return;
        }

        this.selection.set(items);
    }

    public deselectItems = (...items: (string|number)[]): void => {
        if (!items || items.length === 0) {
            return;
        }

        this.selection.update(selectedItems => {
            let newSelectedItems = selectedItems.filter(s => !items.includes(s))
            if ((!newSelectedItems || newSelectedItems.length === 0) && this.selectMode()) {
                this.setSelectionMode(false);
                return [];
            }

            return newSelectedItems;
        });
    }

    public selectItems = (...items: (string|number)[]): void => {
        if (!items || items.length === 0) {
            return;
        }

        this.selection.update(
            selectedItems => {
                let newItems = selectedItems
                    .concat(items)
                    .filter(
                        (item, index, arr) => index === arr.indexOf(item)
                    );

                if (!this.selectMode() && newItems.length) {
                    this.setSelectionMode(true);
                }

                return newItems;
            }
        );
    }

    public isSelected = (item: string|number): boolean => {
        if (!item) {
            return false;
        }

        return this.selection().includes(item);
    }
}
