package entities

import (
	"database/sql"
	"strconv"
	"strings"
)

type CarEvent struct {
	BaseTable[OldCarEvent, NewCarEvent]
}

func (table *CarEvent) Transform(old OldCarEvent) NewCarEvent {
	var mileage sql.NullInt32
	if old.Mileage.Valid {
		s := strings.TrimSpace(old.Mileage.String)
		if s != "" {
			if f, err := strconv.ParseFloat(s, 64); err == nil {
				mileage.Int32 = int32(f)
				mileage.Valid = true
			}
		}
	}

	return NewCarEvent{
		Id:        old.EventId,
		UserId:    old.UserId,
		CarId:     old.CarId,
		TypeId:    old.Type,
		Title:     old.Title,
		Comment:   old.Comment,
		Mileage:   mileage,
		Date:      old.Date,
		IsDeleted: false,
	}
}

type OldCarEvent struct {
	Id      int            `db:"Id"`
	UserId  int            `db:"UserId"`
	CarId   int            `db:"CarId"`
	EventId int            `db:"EventId"`
	Type    int            `db:"Type"`
	Title   sql.NullString `db:"Title"`
	Comment sql.NullString `db:"Comment"`
	Mileage sql.NullString `db:"Mileage"`
	Date    string         `db:"Date"`
}

/*
create table [money-dev].Money.CarEvent
(
    Id      int primary key not null,
    CarId   int             not null,
    UserId  int             not null,
    EventId int             not null,
    Type    int             not null,
    Title   nvarchar(1000),
    Comment nvarchar(max),
    Mileage decimal(18, 3),
    Date    datetime        not null,
);
*/

type NewCarEvent struct {
	Id        int            `db:"id"`
	UserId    int            `db:"user_id"`
	CarId     int            `db:"car_id"`
	TypeId    int            `db:"type_id"`
	Title     sql.NullString `db:"title"`
	Comment   sql.NullString `db:"comment"`
	Mileage   sql.NullInt32  `db:"mileage"`
	Date      string         `db:"date"`
	IsDeleted bool           `db:"is_deleted"`
}

/*
create table public.car_events
(
    user_id    integer not null,
    id         integer not null,
    car_id     integer not null,
    type_id    integer not null,
    title      character varying(1000),
    comment    text,
    mileage    integer,
    date       date    not null,
    is_deleted boolean not null default false,
);
*/
