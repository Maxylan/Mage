import { ThemePalette } from "@angular/material/core"

export interface IPaginationLabling {
    firstPageLabel?: string,
    itemsPerPageLabel?: string
    lastPageLabel?: string,
    nextPageLabel?: string,
    previousPageLabel?: string
}

export type IPaginationConfiguration = {
    length: number,
    pageIndex: number,
    pageSize: number,
    pageSizeOptions: number[],
    color?: ThemePalette,
    disabled?: boolean,
    hidePageSize?: boolean,
    showFirstLastButtons?: boolean,
    labels?: IPaginationLabling,
}
