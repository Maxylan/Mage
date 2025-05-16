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
    username VARCHAR(127) NOT NULL UNIQUE,
    password VARCHAR(127) NOT NULL,
    full_name VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_login TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    privilege SMALLINT NOT NULL DEFAULT 0 CHECK (privilege >= 0 AND privilege <= 15),
    avatar_id INT,
    PRIMARY KEY(id)
);

-- Track known clients.
CREATE TABLE IF NOT EXISTS clients (
    id SERIAL NOT NULL,
    trusted BOOLEAN NOT NULL DEFAULT false,
    address VARCHAR(255) NOT NULL,
    user_agent VARCHAR(1023),
    logins INT NOT NULL DEFAULT 0,
    failed_logins INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_visit TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY(id)
);

-- Bad / Banned clients.
CREATE TABLE IF NOT EXISTS banned_clients (
    id SERIAL NOT NULL,
    client_id INT NOT NULL,
    expires_at TIMESTAMPTZ,
    reason TEXT,
    PRIMARY KEY(id),
    CONSTRAINT fk_client
        FOREIGN KEY(client_id)
        REFERENCES clients(id)
        ON DELETE CASCADE
);

-- Table tracking many-2-many (N:N) relationships between the `clients` & `accounts` tables.
CREATE TABLE IF NOT EXISTS account_clients (
    account_id INT NOT NULL,
    client_id INT NOT NULL,
    PRIMARY KEY(account_id, client_id),
    CONSTRAINT fk_account
        FOREIGN KEY(account_id)
        REFERENCES accounts(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_client
        FOREIGN KEY(client_id)
        REFERENCES clients(id)
        ON DELETE CASCADE
);

-- The `sessions` table keeps track of active sessions.
CREATE TABLE IF NOT EXISTS sessions (
    id SERIAL NOT NULL,
    account_id INT NOT NULL,
    client_id INT NOT NULL,
    code CHAR(36) UNIQUE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ NOT NULL,
    PRIMARY KEY(id),
    CONSTRAINT fk_account
        FOREIGN KEY(account_id)
        REFERENCES accounts(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_client
        FOREIGN KEY(client_id)
        REFERENCES clients(id)
        ON DELETE CASCADE
);

-- The `photos` table keeps track of uploaded photos.
CREATE TABLE IF NOT EXISTS photos (
    id SERIAL NOT NULL,
    slug VARCHAR(127) UNIQUE NOT NULL,
    title VARCHAR(255) NOT NULL,
    summary VARCHAR(255),
    description TEXT,
    uploaded_by INT,
    uploaded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by INT,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at TIMESTAMP NOT NULL,
    is_analyzed BOOLEAN NOT NULL DEFAULT false,
    analyzed_at TIMESTAMP,
    required_privilege SMALLINT NOT NULL DEFAULT 0 CHECK (required_privilege >= 0 AND required_privilege <= 15),
    PRIMARY KEY(id),
    CONSTRAINT fk_uploaded_by_user
        FOREIGN KEY(uploaded_by)
        REFERENCES accounts(id)
        ON DELETE SET NULL,
    CONSTRAINT fk_updated_by_user
        FOREIGN KEY(updated_by)
        REFERENCES accounts(id)
        ON DELETE SET NULL
);

-- The `links` table keeps track of publically shared links to photos.
CREATE TABLE IF NOT EXISTS links (
    id SERIAL NOT NULL,
    photo_id INT NOT NULL,
    code CHAR(32) UNIQUE NOT NULL,
    created_by INT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ NOT NULL,
    access_limit INT CHECK (access_limit >= 0 OR access_limit IS null),
    accessed INT NOT NULL DEFAULT 0 CHECK (accessed >= 0),
    PRIMARY KEY(id),
    CONSTRAINT fk_photo
        FOREIGN KEY(photo_id)
        REFERENCES photos(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_user
        FOREIGN KEY(created_by)
        REFERENCES accounts(id)
        ON DELETE CASCADE
);

-- The `tags` table keeps track of tags.
CREATE TABLE IF NOT EXISTS tags (
    id SERIAL NOT NULL,
    name VARCHAR(127) UNIQUE NOT NULL CHECK (length(name) > 0),
    description TEXT,
    required_privilege SMALLINT NOT NULL DEFAULT 0 CHECK (required_privilege >= 0 AND required_privilege <= 15),
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
    updated_by INT,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    required_privilege SMALLINT NOT NULL DEFAULT 0 CHECK (required_privilege >= 0 AND required_privilege <= 15),
    PRIMARY KEY(id),
    CONSTRAINT fk_created_by_user
        FOREIGN KEY(created_by)
        REFERENCES accounts(id)
        ON DELETE SET NULL,
    CONSTRAINT fk_updated_by_user
        FOREIGN KEY(updated_by)
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
    updated_by INT,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    required_privilege SMALLINT NOT NULL DEFAULT 0 CHECK (required_privilege >= 0 AND required_privilege <= 15),
    PRIMARY KEY(id),
    CONSTRAINT fk_created_by_user
        FOREIGN KEY(created_by)
        REFERENCES accounts(id)
        ON DELETE SET NULL,
    CONSTRAINT fk_updated_by_user
        FOREIGN KEY(updated_by)
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
    filename VARCHAR(127) NOT NULL,
    path VARCHAR(255) NOT NULL,
    dimension DIMENSION NOT NULL DEFAULT 'SOURCE',
    filesize INT CHECK (filesize >= 0),
    width INT CHECK (width >= 0),
    height INT CHECK (height >= 0),
    PRIMARY KEY(id),
    CONSTRAINT fk_photo
        FOREIGN KEY(photo_id)
        REFERENCES photos(id)
        ON DELETE CASCADE
);

-- Table tracking many-2-many (N:N) relationships between the `photos` & `tags` tables.
CREATE TABLE IF NOT EXISTS photo_tags (
    photo_id INT NOT NULL,
    tag_id INT NOT NULL,
    added TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
    added TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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
    added TIMESTAMPTZ NOT NULL DEFAULT NOW(),
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

-- Table tracking many-2-many (N:N) relationships between the `accounts` & `photos` tables.
CREATE TABLE IF NOT EXISTS favorite_photos (
    account_id INT NOT NULL,
    photo_id INT NOT NULL,
    added TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY(account_id, photo_id),
    CONSTRAINT fk_account
        FOREIGN KEY(account_id)
        REFERENCES accounts(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_photo
        FOREIGN KEY(photo_id)
        REFERENCES photos(id)
        ON DELETE CASCADE
);

-- Table tracking many-2-many (N:N) relationships between the `accounts` & `albums` tables.
CREATE TABLE IF NOT EXISTS favorite_albums (
    account_id INT NOT NULL,
    album_id INT NOT NULL,
    added TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY(account_id, album_id),
    CONSTRAINT fk_account
        FOREIGN KEY(account_id)
        REFERENCES accounts(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_album
        FOREIGN KEY(album_id)
        REFERENCES albums(id)
        ON DELETE CASCADE
);

CREATE TYPE severity AS ENUM('TRACE','DEBUG','INFORMATION','SUSPICIOUS','WARNING','ERROR','CRITICAL');
CREATE TYPE method AS ENUM('UNKNOWN','HEAD','GET','POST','PUT','PATCH','DELETE');
CREATE TYPE source AS ENUM('INTERNAL','EXTERNAL');

CREATE TABLE IF NOT EXISTS logs (
    id SERIAL NOT NULL,
    user_id INT,
    user_email VARCHAR(255),
    user_username VARCHAR(127),
    user_full_name VARCHAR(255),
    request_address VARCHAR(255),
    request_user_agent VARCHAR(1023),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    log_level SEVERITY NOT NULL DEFAULT 'INFORMATION',
    source SOURCE NOT NULL DEFAULT 'INTERNAL',
    method METHOD DEFAULT 'UNKNOWN',
    action VARCHAR(255) NOT NULL,
    log TEXT,
    PRIMARY KEY(id)
);

-- Last FK-Constraint, allowing for avatars on users.
ALTER TABLE accounts ADD CONSTRAINT fk_user_avatar FOREIGN KEY(avatar_id) REFERENCES photos(id) ON DELETE SET NULL;

-- Indicies for session lookups by user_id
CREATE INDEX idx_sessions_account_id ON sessions (account_id);
CREATE INDEX idx_sessions_client_id ON sessions (client_id);

CREATE INDEX idx_banned_clients_client_id ON banned_clients (client_id);

-- Indicies for lookups by unique titles/names
CREATE INDEX idx_filepaths_filename ON filepaths (filename);

CREATE INDEX idx_links_code ON links (code);

CREATE INDEX idx_photos_slug ON photos (slug);
CREATE INDEX idx_tags_name ON tags (name);

CREATE INDEX idx_albums_title ON albums (title);
CREATE INDEX idx_categories_title ON categories (title);

-- Index for filepath lookups by photo_id
CREATE INDEX idx_filepaths_photo_id ON filepaths (photo_id);

-- Index for link lookups by photo_id
CREATE INDEX idx_links_photo_id ON links (photo_id);

-- Indicies for many-to-many relationship tables..
CREATE INDEX idx_account_clients_account_id ON account_clients (account_id);
CREATE INDEX idx_account_clients_photo_id ON account_clients (client_id);

CREATE INDEX idx_photo_tags_photo_id ON photo_tags (photo_id);
CREATE INDEX idx_photo_tags_tag_id ON photo_tags (tag_id);

CREATE INDEX idx_photo_albums_photo_id ON photo_albums (photo_id);
CREATE INDEX idx_photo_albums_album_id ON photo_albums (album_id);

CREATE INDEX idx_album_tags_album_id ON album_tags (album_id);
CREATE INDEX idx_album_tags_tag_id ON album_tags (tag_id);

CREATE INDEX idx_favorite_photos_account_id ON favorite_photos (account_id);
CREATE INDEX idx_favorite_photos_photo_id ON favorite_photos (photo_id);

CREATE INDEX idx_favorite_albums_account_id ON favorite_albums (account_id);
CREATE INDEX idx_favorite_albums_album_id ON favorite_albums (album_id);

-- Indicies for commonly filtered columns
CREATE UNIQUE INDEX idx_accounts_email ON accounts (email);
CREATE UNIQUE INDEX idx_accounts_username ON accounts (username);

CREATE UNIQUE INDEX idx_path_filename ON filepaths (path, filename);

-- Indicies for time-based filtering
CREATE INDEX idx_logs_created_at ON logs (created_at);
CREATE INDEX idx_accounts_last_login ON accounts (last_login);
CREATE INDEX idx_clients_last_visit ON clients (last_visit);
CREATE INDEX idx_photos_updated_at ON photos (updated_at);
CREATE INDEX idx_albums_updated_at ON albums (updated_at);
CREATE INDEX idx_categories_updated_at ON categories (updated_at);
