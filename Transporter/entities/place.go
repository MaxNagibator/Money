package entities

import "database/sql"

type Place struct {
	BaseTable[OldPlace, NewPlace]
}

func (table *Place) Transform(old OldPlace) NewPlace {
	return NewPlace{
		Id:           old.PlaceId,
		UserId:       old.UserId,
		Name:         old.Name,
		LastUsedDate: old.LastUsedDate,
		IsDeleted:    false,
	}
}

type OldPlace struct {
	Id           int            `db:"Id"`
	UserId       int            `db:"UserId"`
	PlaceId      int            `db:"PlaceId"`
	Name         string         `db:"Name"`
	Description  sql.NullString `db:"Description"`
	LastUsedDate string         `db:"LastUsedDate"`
}

/*
create table [money-dev].Money.Place
(
    Id           int primary key not null,
    UserId       int             not null,
    Name         nvarchar(500)   not null,
    Description  nvarchar(4000),
    LastUsedDate datetime,
    PlaceId      int             not null,
);
*/

type NewPlace struct {
	Id           int    `db:"id"`
	UserId       int    `db:"user_id"`
	Name         string `db:"name"`
	LastUsedDate string `db:"last_used_date"`
	IsDeleted    bool   `db:"is_deleted"`
}

/*
create table public.places
(
    user_id        integer                  not null,
    id             integer                  not null,
    name           character varying(500)   not null,
    last_used_date timestamp with time zone not null default '-infinity'::timestamp with time zone,
    is_deleted     boolean                  not null
);
*/
