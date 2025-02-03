package entities

import "database/sql"

type Category struct {
	BaseTable[OldCategory, NewCategory]
}

func (table *Category) Transform(old OldCategory) NewCategory {
	return NewCategory{
		Id:          old.CategoryId,
		UserId:      old.UserId,
		Name:        old.Name,
		Description: sql.NullString{},
		ParentId:    old.ParentId,
		Color:       old.Color,
		TypeId:      old.TypeId,
		Order:       old.Order,
		IsDeleted:   false,
	}
}

type OldCategory struct {
	Id          int            `db:"Id"`
	UserId      int            `db:"UserId"`
	CategoryId  int            `db:"CategoryId"`
	Name        string         `db:"Name"`
	Description sql.NullString `db:"Description"`
	ParentId    sql.NullInt32  `db:"ParentId"`
	Color       sql.NullString `db:"Color"`
	TypeId      int            `db:"TypeId"`
	Order       sql.NullInt32  `db:"Order"`
}

/*
create table [money-dev].Money.Category (
  Id int primary key not null,
  UserId int not null,
  CategoryId int not null,
  Name nvarchar(500) not null,
  Description nvarchar(4000),
  ParentId int,
  Color nvarchar(100),
  TypeId int not null,
  [Order] int,
  foreign key (UserId) references [User] (Id)
);
*/

type NewCategory struct {
	Id          int            `db:"id"`
	UserId      int            `db:"user_id"`
	Name        string         `db:"name"`
	Description sql.NullString `db:"description"`
	ParentId    sql.NullInt32  `db:"parent_id"`
	Color       sql.NullString `db:"color"`
	TypeId      int            `db:"type_id"`
	Order       sql.NullInt32  `db:"order"`
	IsDeleted   bool           `db:"is_deleted"`
}

/*
create table public.categories (
  user_id integer not null,
  id integer not null,
  name character varying(500) not null,
  description character varying(4000),
  parent_id integer,
  color character varying(100),
  type_id integer not null,
  "order" integer,
  is_deleted boolean not null default false,
  primary key (user_id, id),
  foreign key (user_id, parent_id) references public.categories (user_id, id)
  match simple on update no action on delete no action,
  foreign key (user_id) references public.domain_users (id)
  match simple on update no action on delete no action
);
*/
