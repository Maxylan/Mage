import { IPhotoDTO } from './photo-dto';
import { ITagDTO } from './tag-dto';

export type TagPhotoCollection = {
    tag: ITagDTO,
    photos: IPhotoDTO[] | null,
}