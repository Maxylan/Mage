import { Tag } from './tag';
import { Photo } from './photo';

export type PhotoTagRelation = {
    photoId: number,
    tagId: number,
    added: Date,
    photo: Photo,
    tag: Tag,
}