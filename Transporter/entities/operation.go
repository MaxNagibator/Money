package entities

import "database/sql"

type Operation struct {
	BaseTable[OldOperation, NewOperation]
}

func (table *Operation) Transform(old OldOperation) NewOperation {
	return NewOperation{
		Id:            old.PaymentId,
		UserId:        old.UserId,
		Sum:           old.Sum,
		CategoryId:    old.CategoryId.Int32,
		Comment:       old.Comment,
		Date:          old.Date,
		CreatedTaskId: old.CreatedTaskId,
		PlaceId:       old.PlaceId,
		IsDeleted:     false,
	}
}

type OldOperation struct {
	Id            int            `db:"Id"`
	UserId        int            `db:"UserId"`
	PaymentId     int            `db:"PaymentId"`
	Sum           string         `db:"sum"`
	CategoryId    sql.NullInt32  `db:"CategoryId"`
	TypeId        int            `db:"TypeId"`
	Comment       sql.NullString `db:"Comment"`
	Date          string         `db:"Date"`
	TaskId        sql.NullInt32  `db:"TaskId"`
	CreatedTaskId sql.NullInt32  `db:"CreatedTaskId"`
	PlaceId       sql.NullInt32  `db:"PlaceId"`
}

/*
create table [money-dev].Money.Payment (
  Id int primary key not null,
  UserId int not null,
  PaymentId int not null,
  Sum decimal(18,2) not null,
  CategoryId int,
  TypeId int not null,
  Comment nvarchar(4000),
  Date datetime not null,
  TaskId int,
  CreatedTaskId int,
  PlaceId int,
  foreign key (UserId) references [User] (Id)
);
*/

type NewOperation struct {
	Id            int            `db:"id"`
	UserId        int            `db:"user_id"`
	Sum           string         `db:"sum"`
	CategoryId    int32          `db:"category_id"`
	Comment       sql.NullString `db:"comment"`
	Date          string         `db:"date"`
	CreatedTaskId sql.NullInt32  `db:"created_task_id"`
	PlaceId       sql.NullInt32  `db:"place_id"`
	IsDeleted     bool           `db:"is_deleted"`
}

/*
create table public.operations (
  user_id integer not null,
  id integer not null,
  sum numeric not null,
  category_id integer not null,
  comment character varying(4000),
  date date not null,
  created_task_id integer,
  place_id integer,
  is_deleted boolean not null,
  primary key (user_id, id),
  foreign key (user_id) references public.domain_users (id)
  match simple on update no action on delete cascade
);
*/
