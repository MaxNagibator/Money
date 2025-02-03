package entities

import "database/sql"

type RegularOperation struct {
	BaseTable[OldRegularOperation, NewRegularOperation]
}

func (table *RegularOperation) Transform(old OldRegularOperation) NewRegularOperation {
	return NewRegularOperation{
		Id:         old.TaskId,
		UserId:     old.UserId,
		Name:       old.Name,
		Sum:        old.Sum.String,
		CategoryId: old.CategoryId.Int32,
		Comment:    old.Comment,
		PlaceId:    old.PlaceId,
		IsDeleted:  false,
		DateFrom:   old.DateFrom,
		DateTo:     old.DateTo,
		RunTime:    old.RunTime,
		TimeTypeId: old.TimeId,
		TimeValue:  old.TimeValue,
	}
}

type OldRegularOperation struct {
	Id         int            `db:"Id"`
	UserId     int            `db:"UserId"`
	TaskId     int            `db:"TaskId"`
	Name       string         `db:"Name"`
	TypeId     int            `db:"TypeId"`
	TimeId     int            `db:"TimeId"`
	TimeValue  sql.NullInt32  `db:"TimeValue"`
	DateFrom   string         `db:"DateFrom"`
	DateTo     sql.NullString `db:"DateTo"`
	RunTime    sql.NullString `db:"RunTime"`
	Sum        sql.NullString `db:"Sum"`
	CategoryId sql.NullInt32  `db:"CategoryId"`
	Comment    sql.NullString `db:"Comment"`
	PlaceId    sql.NullInt32  `db:"PlaceId"`
}

/*
create table [money-dev].Money.RegularTask (
  Id int primary key not null,
  UserId int not null,
  TaskId int not null,
  Name nvarchar(max) not null,
  TypeId int not null,
  TimeId int not null,
  TimeValue int,
  DateFrom date not null,
  DateTo date,
  RunTime datetime,
  Sum decimal(18,2),
  CategoryId int,
  Comment nvarchar(4000),
  PlaceId int,
  foreign key (UserId) references [User] (Id)
);
*/

type NewRegularOperation struct {
	Id         int            `db:"id"`
	UserId     int            `db:"user_id"`
	Name       string         `db:"name"`
	Sum        string         `db:"sum"`
	CategoryId int32          `db:"category_id"`
	Comment    sql.NullString `db:"comment"`
	PlaceId    sql.NullInt32  `db:"place_id"`
	IsDeleted  bool           `db:"is_deleted"`
	DateFrom   string         `db:"date_from"`
	DateTo     sql.NullString `db:"date_to"`
	RunTime    sql.NullString `db:"run_time"`
	TimeTypeId int            `db:"time_type_id"`
	TimeValue  sql.NullInt32  `db:"time_value"`
}

/*
create table public.regular_operations (
  user_id integer not null,
  id integer not null,
  name character varying(500) not null,
  sum numeric not null,
  category_id integer not null,
  comment character varying(4000),
  place_id integer,
  is_deleted boolean not null,
  date_from date not null default '-infinity'::date,
  date_to date,
  run_time date,
  time_type_id integer not null default 0,
  time_value integer,
  primary key (user_id, id),
  foreign key (user_id) references public.domain_users (id)
  match simple on update no action on delete cascade
);
*/
