import { Photo } from './photo';
import { Dimension } from './dimension';

export interface IFilepathDTO {
    id?: number | null,
    photoId: number,
    /**
     * Max Length: 127
     * Min Length: 0
     */
    filename?: string | null,
    /**
     * Max Length: 255
     * Min Length: 0
     */
    path?: string | null,
    dimension: Dimension,
    filesize?: number | null,
    width?: number | null,
    height?: number | null,
    photo: Photo,
    readonly isSource: boolean,
    readonly isMedium: boolean,
    readonly isThumbnail: boolean,
}