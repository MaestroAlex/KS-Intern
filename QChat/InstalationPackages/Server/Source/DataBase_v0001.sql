DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS groups;
DROP TABLE IF EXISTS rooms;
DROP TABLE IF EXISTS room_members;
DROP TABLE IF EXISTS group_members;
DROP TABLE IF EXISTS user_chats;
DROP TABLE IF EXISTS contents;

                                   
CREATE TABLE users(                
id SERIAL PRIMARY KEY,             
login TEXT NOT NULL,               
password INTEGER NOT NULL          
);                                 

CREATE TABLE groups(
id SERIAL PRIMARY KEY,
name TEXT NOT NULL);

CREATE TABLE rooms(
id SERIAL PRIMARY KEY,
name TEXT NOT NULL
);


CREATE TABLE user_chats(
id BIGSERIAL PRIMARY KEY,
user_1 INTEGER REFERENCES users(id),
user_2 INTEGER REFERENCES users(id)
);

CREATE TABLE room_members(
room_id INTEGER REFERENCES rooms(id),
user_id INTEGER REFERENCES rooms(id)
);

CREATE TABLE group_members(
group_id INTEGER REFERENCES groups(id),
user_id INTEGER REFERENCES users(id)
);


CREATE TYPE content_type AS ENUM ('None','Text','Audio','Video','Image','Gif','File');

CREATE TABLE contents(
id BIGSERIAL PRIMARY KEY,
content_type content_type NOT NULL,
content BYTEA NOT NULL);


CREATE TABLE user_chat_history(
chat_id BIGINT REFERENCES user_chats(id),
sender_id INTEGER REFERENCES users(id),
content_id BIGINT REFERENCES contents(id)
);

CREATE TABLE group_chat_history(
group_id INTEGER REFERENCES groups(id),
sender_id INTEGER REFERENCES users(id),
content_id BIGINT REFERENCES contents(id)
);


INSERT IN rooms(id, name) VALUES(2, 'General');
 






