-- Create the `magedb` database schema.
CREATE SCHEMA IF NOT EXISTS magedb;
SET search_path TO 'magedb';

-- Timezone of the `magedb` database schema.
SET timezone TO 'Europe/Stockholm';
SET datestyle TO 'Euro';

-- The `accounts` table keeps track of valid accounts.
CREATE TABLE IF NOT EXISTS accounts (
    id SERIAL NOT NULL,
    email VARCHAR(255) UNIQUE,
    username VARCHAR(63) UNIQUE,
    password VARCHAR(127) NOT NULL,
    full_name VARCHAR(127),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_visit TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    permissions SMALLINT NOT NULL DEFAULT 0 CHECK (permissions >= 0 AND permissions <= 15),
    PRIMARY KEY(id)
);

-- The `sessions` table keeps track of active sessions.
CREATE TABLE IF NOT EXISTS sessions (
    id SERIAL NOT NULL,
    user_id INT NOT NULL,
    code CHAR(36) UNIQUE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ NOT NULL,
    PRIMARY KEY(id),
    CONSTRAINT fk_user
        FOREIGN KEY(user_id)
        REFERENCES accounts(id)
        ON DELETE CASCADE
);

-- The `photos` table keeps track of uploaded photos.
CREATE TABLE IF NOT EXISTS photos (
    id SERIAL NOT NULL,
    name VARCHAR(127) UNIQUE NOT NULL,
    title VARCHAR(255),
    summary VARCHAR(255),
    description TEXT,
    created_by INT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY(id),
    CONSTRAINT fk_user
        FOREIGN KEY(created_by)
        REFERENCES accounts(id)
        ON DELETE SET NULL
);

-- The `tags` table keeps track of tags.
CREATE TABLE IF NOT EXISTS tags (
    id SERIAL NOT NULL,
    name VARCHAR(127) UNIQUE NOT NULL,
    description TEXT,
    PRIMARY KEY(id)
);

-- The `categories` table keeps track of album categories.
CREATE TABLE IF NOT EXISTS categories (
    id SERIAL NOT NULL,
    title VARCHAR(255) UNIQUE NOT NULL,
    summary VARCHAR(255),
    description TEXT,
    created_by INT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY(id),
    CONSTRAINT fk_user
        FOREIGN KEY(created_by)
        REFERENCES accounts(id)
        ON DELETE SET NULL
);

-- The `albums` table keeps track of photo albums.
CREATE TABLE IF NOT EXISTS albums (
    id SERIAL NOT NULL,
    category_id INT,
    thumbnail_id INT,
    title VARCHAR(255) UNIQUE NOT NULL,
    summary VARCHAR(255),
    description TEXT,
    created_by INT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY(id),
    CONSTRAINT fk_user
        FOREIGN KEY(created_by)
        REFERENCES accounts(id)
        ON DELETE SET NULL,
    CONSTRAINT fk_category
        FOREIGN KEY(category_id)
        REFERENCES categories(id)
        ON DELETE SET NULL,
    CONSTRAINT fk_thumbnail
        FOREIGN KEY(thumbnail_id)
        REFERENCES photos(id)
        ON DELETE SET NULL
);

-- Different `dimension` images (Image Sizes) ensures good load-times when loading photos.
CREATE TYPE dimension AS ENUM('THUMBNAIL','MEDIUM','SOURCE');

-- The `filepaths` table keeps track of where photos are located on-disk.
CREATE TABLE IF NOT EXISTS filepaths (
    id SERIAL NOT NULL,
    photo_id INT NOT NULL,
    filename VARCHAR(127) UNIQUE NOT NULL,
    path VARCHAR(255) NOT NULL,
    dimension DIMENSION NOT NULL,
    filesize INT NOT NULL CHECK (filesize >= 0),
    PRIMARY KEY(id),
    CONSTRAINT fk_photo
        FOREIGN KEY(photo_id)
        REFERENCES photos(id)
        ON DELETE RESTRICT
);

-- Table tracking many-2-many (N:N) relationships between the `photos` & `tags` tables.
CREATE TABLE IF NOT EXISTS photo_tags (
    photo_id INT NOT NULL,
    tag_id INT NOT NULL,
    PRIMARY KEY(photo_id, tag_id),
    CONSTRAINT fk_photo
        FOREIGN KEY(photo_id)
        REFERENCES photos(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_tag
        FOREIGN KEY(tag_id)
        REFERENCES tags(id)
        ON DELETE CASCADE
);

-- Table tracking many-2-many (N:N) relationships between the `photos` & `albums` tables.
CREATE TABLE IF NOT EXISTS photo_albums (
    photo_id INT NOT NULL,
    album_id INT NOT NULL,
    PRIMARY KEY(photo_id, album_id),
    CONSTRAINT fk_photo 
        FOREIGN KEY(photo_id)
        REFERENCES photos(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_album
        FOREIGN KEY(album_id)
        REFERENCES albums(id)
        ON DELETE CASCADE
);

-- Table tracking many-2-many (N:N) relationships between the `albums` & `tags` tables.
CREATE TABLE IF NOT EXISTS album_tags (
    album_id INT NOT NULL,
    tag_id INT NOT NULL,
    PRIMARY KEY(album_id, tag_id),
    CONSTRAINT fk_album
        FOREIGN KEY(album_id)
        REFERENCES albums(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_tag
        FOREIGN KEY(tag_id)
        REFERENCES tags(id)
        ON DELETE CASCADE
);

CREATE TYPE severity AS ENUM('TRACE','DEBUG','INFORMATION','SUSPICIOUS','WARNING','ERROR','CRITICAL');
CREATE TYPE method AS ENUM('HEAD','GET','POST','PUT','PATCH','DELETE');
CREATE TYPE source AS ENUM('INTERNAL','EXTERNAL');

CREATE TABLE IF NOT EXISTS logs (
    id SERIAL NOT NULL,
    user_id INT,
    user_email VARCHAR(255),
    user_username VARCHAR(63),
    user_full_name VARCHAR(127),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    log_level SEVERITY NOT NULL DEFAULT 'INFORMATION',
    source SOURCE NOT NULL DEFAULT 'INTERNAL',
    method METHOD,
    action VARCHAR(255),
    log TEXT,
    PRIMARY KEY(id)
);

-- Indicies for session lookups by user_id
CREATE INDEX idx_sessions_user_id ON sessions (user_id);

-- Indicies for lookups by unique titles/names
CREATE INDEX idx_filepaths_filename ON filepaths (filename);

CREATE INDEX idx_photos_name ON photos (name);
CREATE INDEX idx_tags_name ON tags (name);

CREATE INDEX idx_albums_title ON albums (title);
CREATE INDEX idx_categories_title ON categories (title);

-- Index for filepath lookups by photo_id
CREATE INDEX idx_filepaths_photo_id ON filepaths (photo_id);

-- Indicies for many-to-many relationships (photo-tags, photo-albums, album-categories)
CREATE INDEX idx_photo_tags_photo_id ON photo_tags (photo_id);
CREATE INDEX idx_photo_tags_tag_id ON photo_tags (tag_id);

CREATE INDEX idx_photo_albums_photo_id ON photo_albums (photo_id);
CREATE INDEX idx_photo_albums_album_id ON photo_albums (album_id);

CREATE INDEX idx_album_tags_album_id ON album_tags (album_id);
CREATE INDEX idx_album_tags_tag_id ON album_tags (tag_id);

-- Indicies for commonly filtered columns
CREATE UNIQUE INDEX idx_accounts_email ON accounts (email);
CREATE UNIQUE INDEX idx_accounts_username ON accounts (username);

-- Indicies for time-based filtering
CREATE INDEX idx_logs_created_at ON logs (created_at);
CREATE INDEX idx_accounts_last_visit ON accounts (last_visit);
CREATE INDEX idx_photos_updated_at ON photos (updated_at);
CREATE INDEX idx_albums_updated_at ON albums (updated_at);
CREATE INDEX idx_categories_updated_at ON categories (updated_at);