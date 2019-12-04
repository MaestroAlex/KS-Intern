create table users (
	id serial primary key,
	login varchar not null unique,
	password varchar not null,
	check (length(login) > 0 AND length(password) > 0)
);
create table room (
	id serial primary key,
	name varchar unique,
	check (length(name) > 0)
);
create table user_in_room (
	user_id integer references users(id),
	room_id integer references room(id),
	primary key (user_id, room_id)
);
create table message (
	id bigserial primary key,
	user_id integer references users(id),
	room_id integer references room(id),
	date timestamp not null default CURRENT_TIMESTAMP,
	message varchar check (length(message) > 0)
);
CREATE OR REPLACE FUNCTION RemoveRoom()
 RETURNS trigger AS
$BODY$
BEGIN 
  IF OLD.id=1 THEN RAISE EXCEPTION 'Cant remove General chat room!';
  ELSE RETURN NEW;
  END IF;
END;
$BODY$ language plpgsql;
CREATE TRIGGER removeRoomTrigger
  BEFORE DELETE ON room
  FOR EACH ROW
  EXECUTE PROCEDURE RemoveRoom();
INSERT INTO room (name) VALUES ('General');