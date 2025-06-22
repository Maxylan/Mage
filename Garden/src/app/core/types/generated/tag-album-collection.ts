import { IAlbumDTO } from './album-dto';
import { ITagDTO } from './tag-dto';

export type TagAlbumCollection = {
    tag: ITagDTO,
    albums: IAlbumDTO[] | null,
}