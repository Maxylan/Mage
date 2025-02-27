import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { MatPaginator, MatPaginatorIntl, PageEvent } from '@angular/material/paginator';
import { IPaginationConfiguration, IPaginationLabling } from './pagination.types';

@Component({
    selector: 'shared-paginator',
    imports: [
        MatPaginator
    ],
    templateUrl: 'pagination.component.html',
    styleUrl: 'pagination.component.css'
})
export class PaginationComponent {
    private paginatorService = inject(MatPaginatorIntl);
    
    public setLablingOptions(lableOptions: IPaginationLabling): void {
        // Remove undefined/null (omitted) values..
        lableOptions = {
            ...this.paginatorService,
            ...lableOptions
        };

        this.paginatorService.firstPageLabel = lableOptions.firstPageLabel!;
        this.paginatorService.itemsPerPageLabel = lableOptions.itemsPerPageLabel!;
        this.paginatorService.lastPageLabel = lableOptions.lastPageLabel!;
        this.paginatorService.nextPageLabel = lableOptions.nextPageLabel!;
        this.paginatorService.previousPageLabel = lableOptions.previousPageLabel!;

        this.paginatorService.changes.next();
    };

    public getLabels = (): IPaginationLabling => ({
        firstPageLabel: this.paginatorService.firstPageLabel,
        itemsPerPageLabel: this.paginatorService.itemsPerPageLabel,
        lastPageLabel: this.paginatorService.lastPageLabel,
        nextPageLabel: this.paginatorService.nextPageLabel,
        previousPageLabel: this.paginatorService.previousPageLabel
    });

    // Required..
    @Input({ required: true })
    length!: IPaginationConfiguration['length'];

    @Input({ required: true })
    pageSize!: IPaginationConfiguration['pageSize'];

    @Input({ required: true })
    pageIndex!: IPaginationConfiguration['pageIndex'];

    @Input()
    pageSizeOptions: IPaginationConfiguration['pageSizeOptions'] = [
        8, 16, 32, 48, 64
    ];

    @Input()
    color: IPaginationConfiguration['color'];
    @Input()
    disabled: IPaginationConfiguration['disabled'];
    @Input()
    hidePageSize: IPaginationConfiguration['hidePageSize'] = !this.pageSize;
    @Input()
    showFirstLastButtons: IPaginationConfiguration['showFirstLastButtons'] = this.pageIndex !== 0;
    
    @Output()
    page = new EventEmitter<PageEvent>();
    forwardPageEvent = (e: PageEvent) => this.page.emit(e);

    /* constructor(options?: IPaginationConfiguration, lables?: IPaginationLabling) {
        if (options) {
            this.length = options.length;
            this.pageSize = options.pageSize;
            this.pageIndex = options.pageIndex;
            this.pageSizeOptions = options.pageSizeOptions;
            this.color = options.color;
            this.disabled = options.disabled;
            this.hidePageSize = options.hidePageSize;
            this.showFirstLastButtons = options.showFirstLastButtons;
        }

        if (lables) {
            this.setLablingOptions(lables);
        }
    } */
}
