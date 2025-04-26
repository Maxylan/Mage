import { Route, Routes } from '@angular/router';
import { HomePageComponent } from './pages/home/home.component';
import { PhotosPageComponent } from './pages/photos/photos.component';
import { SinglePhotoPageComponent } from './pages/photos/single-photo.component';
import { AlbumsPageComponent } from './pages/albums/albums.component';
import { TagsPageComponent } from './pages/tags/tags.component';
import { CategoriesPageComponent } from './pages/categories/categories.component';
import { AdminPageComponent } from './pages/admin/admin.component';

export const navigation: (Route & { headline: string })[] = [
    { path: 'photos', component: PhotosPageComponent, headline: 'Photos' },
    { path: 'albums', component: AlbumsPageComponent, headline: 'Albums' },
    { path: 'tags', component: TagsPageComponent, headline: 'Tags' },
    { path: 'categories', component: CategoriesPageComponent, headline: 'Categories' },
    { path: 'admin', component: AdminPageComponent, headline: 'Admin' },
];

export const routes: Routes = [
    { path: 'photos/single/:id', component: SinglePhotoPageComponent },
    ...navigation,
    { path: '', component: HomePageComponent, headline: 'Home' }
];
