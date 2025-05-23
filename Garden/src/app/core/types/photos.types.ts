import { Params } from "@angular/router";

export type PhotoCollection = {
    source: Photo,
    medium?: Photo,
    thumbnail?: Photo
};

export type Photo = Filepath & {
    photoId: number,
    slug: string,
    title: string,
    summary?: string,
    description?: string,
    uploadedBy?: number,
    uploadedAt: Date,
    createdAt?: Date,
    updatedAt: Date,
    tags?: string[],
    links?: string[]
};

export type Filepath = {
    filepathId: number,
    dimension: Dimension,
    filesize?: number,
    height?: number,
    width?: number,
    filename: string,
    path: string,
}

export type PhotoDTO = MutatePhoto & {
    uploadedBy?: number,
    uploadedAt: Date,
    createdAt?: Date,
    updatedAt: Date
};

export type MutatePhoto = {
    id?: number,
    slug: string,
    title: string,
    summary?: string,
    description?: string,
    tags?: string[]
};

/* Photo {
    photoId: number,
    filepathId: number,
    slug: string,
    title: string,
    summary?: string,
    description?: string,
    uploadedBy?: number,
    uploadedAt: Date,
    createdAt?: Date,
    updatedAt: Date,
    dimension: Dimension,
    filesize?: number,
    height?: number,
    width?: number,
    filename: string,
    path: string,
    tags?: string[],
    links?: string[]
} */

/* PhotoDTO = {
    id?: number,
    slug: string,
    title: string,
    summary?: string,
    description?: string,
    tags?: string[],
    uploadedBy?: number,
    uploadedAt: Date,
    createdAt?: Date,
    updatedAt: Date
} */

export enum Dimension {
    SOURCE = 'SOURCE',
    MEDIUM = 'MEDIUM',
    THUMBNAIL = 'THUMBNAIL'
};

export type FavoritePhotos = Set<Photo['photoId']>;
export type PhotoPage = {
    page: number,
    set: Set<PhotoCollection>
};
export interface PhotoPageStore {
    isLoading: boolean,
    currentPage: number,
    pageSize: number,
    pages: PhotoPage[]
    page: () => PhotoPage|null
};
export const defaultPhotoPageContainer: PhotoPageStore = {
    isLoading: false,
    currentPage: 0,
    pageSize: 32,
    pages: [],
    page: function() {
        return this.pages.at(
            this.currentPage
        ) || null;
    }
};

export interface IPhotoSearchParameters {
    slug?: string,
    title?: string,
    summary?: string,
};

export interface IPhotoQueryParameters {
    limit: number,
    offset: number,
    dimension?: Dimension,
    search?: string,
    tags?: string[],
    slug?: string,
    title?: string,
    summary?: string,
    uploadedBy?: number,
    uploadedBefore?: Date,
    uploadedAfter?: Date,
    createdBefore?: Date,
    createdAfter?: Date
};

export type SearchQueryParameters = Omit<IPhotoQueryParameters, 'offset'|'limit'> & Params; 
