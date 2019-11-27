DROP TABLE IF EXISTS users_by_chats;
DROP TABLE IF EXISTS messages;
DROP TABLE IF EXISTS chats;
DROP TABLE IF EXISTS users;
CREATE TABLE users
(
   id SERIAL PRIMARY KEY,
   name varchar(100) NOT NULL,
   nick varchar(100) DEFAULT NULL
);
CREATE TABLE chats
(
   id SERIAL PRImARY KEY,
   name varchar(100) NOT NULL
);
CREATE TABLE messages
(
   id SERIAL PRIMARY KEY,
   chat_id integer references chats(id),
   user_id integer references users(id),
   message text
);
CREATE TABLE users_by_chats
(
   chat_id integer NOT NULL,
   user_id integer NOT NULL,
   PRIMARY KEY(chat_id,user_id),
   FOREIGN KEY (chat_id) REFERENCES chats(id),
   FOREIGN KEY (user_id) REFERENCES users(id)
);
INSERT INTO chats(name) VALUES('general');