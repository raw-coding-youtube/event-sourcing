-- https://www.postgresql.org/docs/14/sql-createtable.html
create database snapshotter;

create table events
(
    id  uuid  primary key,
    event jsonb,
    type varchar
);

create table projections
(
    id  uuid,
    payload jsonb,
    type varchar,
    constraint type_id primary key(type, id)
);
