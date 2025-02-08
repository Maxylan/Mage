<pre>
## Components
│
├─ Blocks (Very modular, easily reusable components, like `@angular/material`)
│   ╰─ #TBD ...
│
├─ Navbar
│
╰─ Upload Form *(available on all pages)*

## Services
│
├─ API (Reception) Controllers - Gets data to display.
│   ├─ `/auth`
│   ├─ `/accounts`
│   ├─ `/categories`
│   ├─ `/tags`
│   ├─ `/albums`
│   ├─ `/photos` (+ `../blob`)
│   ├─ `/links` (+ `view/..`)
│   ╰─ `/logs`
│
╰─ Resolvers - Transform IDs into useful data.
    ├─ AccountResolver
    │   - Resolves `User IDs` into Accounts.
    │   - Resolves `Avatar IDs` into Photos.
    ├─ CategoryResolver
    │   - Resolves `Category IDs` into Categories.
    ├─ AlbumResolver
    │   - Resolves `Album IDs` into Categories.
    │   - Resolves `Thumbnail IDs` into Photos.
    ├─ TagResolver
    │   - Resolves `Tag Names` into Tags.
    ╰─ PhotoResolver
        - Resolves `Photo IDs` into Photos.

## Pages
- Each page supports uploading photos, for convenience.
│
├─ Home (`#`)
│   - Greeting
│   - Recently visited photos *(localStorage)*
│   - Recently visited albums *(localStorage)*
│
├─ Photos (`#/photos`)
│   - Searchable
│   - Paginated view of most recently created *(..or uploaded)* photos.
│       - Photo Cards have shortcuts to delete, rename and share *(link)* photos.
│   - Shows available tags, to aid filtering / searching.
│   ╰─ Inspect Single Photo (`photos/{photo_id}`)
│       - View / Edit photo details
│       - Add / Remove tags
│       - A convenient share button *(..create link button)*
│       - Display references to existing links
│       - Display tags
│       - *(Desktop)* Right-click tags to quickly remove them from the photo
│         (API: `(PATCH)` `/photos/{photo_id}/remove/tag/{tag_name}`)
│
├─ List Tags (`#/tags`)
│   - Non-paginated view of all tags, ordered by their item counts.
│   - Filterable / Searchable
│   ╰─ Inspect Single Tag (`tags/{tag_name}`)
│       - View / Edit tag description
│       - See albums tagged with this tag (API: `/tags/{tag_name}/albums`)
│       - See photos tagged with this tag (API: `/tags/{tag_name}/photos`)
│       - *(Desktop)* Right-click albums/photos to quickly remove them from the tag
│         (API: `(PATCH)` `/albums/{album_id}/remove/tag/{tag_name}`)
│         (API: `(PATCH)` `/photos/{photo_id}/remove/tag/{tag_name}`)
│
├─ Albums (`#/albums`)
│   - Searchable
│   - Paginated view of most recently updated albums
│       - Album Cards have shortcuts to delete and rename.
│   - Shows available tags, to aid filtering / searching.
│   ╰─ Inspect Single Album (`albums/{album_id}`)
│       - View / Edit album details
│       - See photos (API: `/albums/{album_id}/photos`)
│       - See tags (API: `/albums/{album_id}/tags`)
│       - *(Desktop)* Right-click tags to quickly remove them from the album
│         (API: `(PATCH)` `/albums/{album_id}/remove/tag/{tag_name}`)
│
├─ Categories (`#/categories`)
│   - Non-paginated view of all categories, ordered by their item counts.
│   - Filterable / Searchable
│   ╰─ Inspect Single Category (`categories/{category_id}`)
│       - View / Edit category details
│       - See albums in this category (API: `/categories/{category_id}/albums`)
│          - Album Cards have shortcuts to delete, rename and remove from category.
│
╰─ Admin (`#/admin`)
    ├─ Accounts (`admin/accounts`)
    ╰─ Logs (`admin/logs`)
</pre>

Example folder structure:
<pre>
app/
│── core/                 # Core services, interceptors, resolvers
│   ├── api/              # API controllers
│   │   ├── auth.service.ts
│   │   ├── accounts.service.ts
│   │   ├── categories.service.ts
│   │   ├── tags.service.ts
│   │   ├── albums.service.ts
│   │   ├── photos.service.ts
│   │   ├── links.service.ts
│   │   ├── logs.service.ts
│   │   └── api.module.ts
│   ├── resolvers/        # Route data resolvers
│   │   ├── account.resolver.ts
│   │   ├── category.resolver.ts
│   │   ├── album.resolver.ts
│   │   ├── tag.resolver.ts
│   │   ├── photo.resolver.ts
│   │   └── resolvers.module.ts
│   ├── guards/           # Route guards (if needed)
│   │   ├── auth.guard.ts
│   │   ├── admin.guard.ts
│   │   └── guards.module.ts
│   ├── interceptors/     # Global HTTP interceptors
│   │   ├── auth.interceptor.ts
│   │   └── logging.interceptor.ts
│   └── core.module.ts    # Global providers (import in `AppModule`)
│
│── shared/               # Reusable components, pipes, directives
│   ├── blocks/           # UI component library (like Angular Material)
│   │   ├── button/
│   │   ├── card/
│   │   ├── modal/
│   │   ├── dropdown/
│   │   ├── input-field/
│   │   └── blocks.module.ts
│   ├── directives/       # Shared directives
│   │   ├── click-outside.directive.ts
│   │   ├── lazy-load.directive.ts
│   │   └── directives.module.ts
│   ├── pipes/            # Reusable pipes
│   │   ├── format-date.pipe.ts
│   │   ├── truncate.pipe.ts
│   │   └── pipes.module.ts
│   └── shared.module.ts  # Imports all shared modules
│
│── layout/               # Structural UI (nav, footer, layout)
│   ├── navbar/
│   │   ├── navbar.component.ts
│   │   ├── navbar.component.html
│   │   ├── navbar.component.scss
│   │   └── navbar.module.ts
│   ├── upload-form/
│   │   ├── upload-form.component.ts
│   │   ├── upload-form.component.html
│   │   ├── upload-form.component.scss
│   │   └── upload-form.module.ts
│   └── layout.module.ts
│
│── pages/                # Feature modules for each page
│   ├── home/             # Homepage
│   │   ├── home.component.ts
│   │   ├── home.component.html
│   │   ├── home.component.scss
│   │   └── home.module.ts
│   ├── photos/           # Photos page
│   │   ├── list-photos/
│   │   │   ├── list-photos.component.ts
│   │   │   ├── list-photos.component.html
│   │   │   ├── list-photos.component.scss
│   │   │   ├── list-photos.module.ts
│   │   │   └── photo-card/  # Nested subcomponent
│   │   │       ├── photo-card.component.ts
│   │   │       ├── photo-card.component.html
│   │   │       ├── photo-card.component.scss
│   │   ├── inspect-photo/
│   │   │   ├── inspect-photo.component.ts
│   │   │   ├── inspect-photo.component.html
│   │   │   ├── inspect-photo.component.scss
│   │   │   └── inspect-photo.module.ts
│   │   └── photos.module.ts
│   ├── albums/           # Albums page
│   │   ├── list-albums/
│   │   ├── inspect-album/
│   │   └── albums.module.ts
│   ├── tags/             # Tags page
│   │   ├── list-tags/
│   │   ├── inspect-tag/
│   │   └── tags.module.ts
│   ├── categories/       # Categories page
│   │   ├── list-categories/
│   │   ├── inspect-category/
│   │   └── categories.module.ts
│   ├── admin/            # Admin panel
│   │   ├── accounts/
│   │   ├── logs/
│   │   └── admin.module.ts
│   └── pages.module.ts
│
│── app.component.ts      # Root app component
│── app.module.ts         # Root app module
│── app-routing.module.ts # Global app routing
│── main.ts               # Entry point
│── styles.scss           # Global styles
│── environments/         # Environment configs
</pre>
