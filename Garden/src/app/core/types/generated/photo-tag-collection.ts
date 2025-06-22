import { ITagDTO } from './tag-dto';
import { IPhotoDTO } from './photo-dto';

export type PhotoTagCollection = {
    photo: IPhotoDTO,
    tags: ITagDTO[] | null,
}