import { DisplayAlbum } from './display-album';

export type DisplayCategory = {
    readonly albums: DisplayAlbum[] | null,
    readonly count: number,
}