import { computed, Injectable, WritableSignal, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';

export type SelectState<T extends object = {}> = {
    selectModeActive: boolean,
    selection: object[]
};

@Injectable({
    providedIn: 'root'
})
export class SelectionObserver {
    private selection: WritableSignal<object[]> = signal<object[]>([]);
    private selectMode: WritableSignal<boolean> = signal(false);
    private observable?: Observable<SelectState>;

    public State = computed(() => ({
        selectModeActive: this.selectMode(),
        selection: this.selection()
    }));

    public ngOnInit() {
        if (!this.observable) {
            this.observable = toObservable(this.State);
        }
    }

    public observe = (): Observable<SelectState<object>> => {
        if (!this.observable) {
            this.observable = toObservable(this.State);
        }

        return this.observable;
    }

    public subscribe: Observable<SelectState<object>>['subscribe'] = this.observe().subscribe;
    public forEach: Observable<SelectState<object>>['forEach'] = this.observe().forEach;
    public pipe: Observable<SelectState<object>>['pipe'] = this.observe().pipe;

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

    public getSelectedItems = (): object[] =>
        this.selection();

    public setSelectedItems = (items: object[]): void => {
        if ((!items || items.length === 0) && this.selectMode()) {
            this.setSelectionMode(false);
            return;
        }

        this.selection.set(items);
    }

    public deselectItems = (...items: object[]): void => {
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

    public selectItems = (...items: object[]): void => {
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
}
