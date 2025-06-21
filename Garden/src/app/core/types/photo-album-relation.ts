import { Photo } from './photo';
import { Album } from './album';
import { IPhotoAlbumRelationDTO } from './photo-album-relation-dto';

export type PhotoAlbumRelation = IPhotoAlbumRelationDTO & {
    photoId: number,
    albumId: number,
    added: Date,
    album: Album,
    photo: Photo,
}