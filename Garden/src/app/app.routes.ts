import { Route, Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { PhotosComponent } from './pages/photos/photos.component';
import { SinglePhotoComponent } from './pages/photos/single-photo.component';

export const navigation: (Route & { headline: string })[] = [
    { path: 'photos', component: PhotosComponent, headline: 'Photos' },
    { path: 'albums', component: PhotosComponent, headline: 'Albums' },
    { path: 'tags', component: PhotosComponent, headline: 'Tags' },
    { path: 'categories', component: PhotosComponent, headline: 'Categories' },
    { path: 'admin', component: PhotosComponent, headline: 'Admin' },
];

export const routes: Routes = [
    { path: 'photos/single/:id', component: SinglePhotoComponent },
    ...navigation,
    { path: '', component: HomeComponent, headline: 'Home' }
];
