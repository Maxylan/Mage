-- Select the `magedb` database schema.
SET search_path TO 'magedb';

-- Truncate tables (in case they already existed..)
TRUNCATE TABLE accounts CASCADE;
TRUNCATE TABLE sessions CASCADE;
TRUNCATE TABLE categories CASCADE;
TRUNCATE TABLE albums CASCADE;
TRUNCATE TABLE tags CASCADE;
TRUNCATE TABLE photos CASCADE;
TRUNCATE TABLE filepaths CASCADE;
TRUNCATE TABLE photo_tags CASCADE;
TRUNCATE TABLE photo_albums CASCADE;
TRUNCATE TABLE album_tags CASCADE;
TRUNCATE TABLE logs CASCADE;

INSERT INTO accounts (email, username, password, full_name, permissions) VALUES(
    'webmaster@torpssons.se', 'root', 'password', 'Administrator', 15
);