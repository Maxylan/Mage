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
TRUNCATE TABLE clients CASCADE;
TRUNCATE TABLE banned_clients CASCADE;
TRUNCATE TABLE account_clients CASCADE;
TRUNCATE TABLE favorite_photos CASCADE;
TRUNCATE TABLE favorite_albums CASCADE;

INSERT INTO accounts (email, username, password, full_name, privilege) VALUES
    ('webmaster@torpssons.se', 'root', 'b392112656161e99cc7a9dff542ef3b2d27a7f66d1fe1f7fee846780e82b49fc', 'Administrator', 15),
    ('maxylan@torpssons.se', 'maxylan', 'b392112656161e99cc7a9dff542ef3b2d27a7f66d1fe1f7fee846780e82b49fc', 'Maxylan', 15),
    ('skais@torpssons.se', 'skais', 'b392112656161e99cc7a9dff542ef3b2d27a7f66d1fe1f7fee846780e82b49fc', 'Skais', 15),
    ('user@torpssons.se', 'user', 'b392112656161e99cc7a9dff542ef3b2d27a7f66d1fe1f7fee846780e82b49fc', 'Anonymous User', 3),
    ('guest@torpssons.se', 'guest', 'b392112656161e99cc7a9dff542ef3b2d27a7f66d1fe1f7fee846780e82b49fc', 'Anonymous Guest', 1);

INSERT INTO tags (name, description) VALUES
    ('Generated', 'Automagically Generated Content'),
    ('2025', 'Images taken/created during 2025');

INSERT INTO logs (user_id, user_email, user_username, user_full_name, created_at, log_level, source, method, action, log) VALUES
    (0, 'webmaster@torpssons.se', 'root', 'Administrator', NOW(), 'INFORMATION', 'INTERNAL', 'UNKNOWN', 'seed', 'Hello World!');
