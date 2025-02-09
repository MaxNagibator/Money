package entities

import "database/sql"

type FastOperation struct {
	BaseTable[OldFastOperation, NewFastOperation]
}

func (table *FastOperation) Transform(old OldFastOperation) NewFastOperation {
	return NewFastOperation{
		Id:         old.FastOperationId,
		UserId:     old.UserId,
		Order:      old.Order,
		Name:       old.Name,
		Sum:        old.Sum,
		CategoryId: old.CategoryId.Int32,
		Comment:    old.Comment,
		PlaceId:    old.PlaceId,
		IsDeleted:  false,
	}
}

type OldFastOperation struct {
	Id              int            `db:"Id"`
	UserId          int            `db:"UserId"`
	FastOperationId int            `db:"FastOperationId"`
	Name            string         `db:"Name"`
	Sum             string         `db:"sum"`
	CategoryId      sql.NullInt32  `db:"CategoryId"`
	TypeId          int            `db:"TypeId"`
	Comment         sql.NullString `db:"Comment"`
	PlaceId         sql.NullInt32  `db:"PlaceId"`
	Order           sql.NullInt32  `db:"Order"`
}

/*
create table [money-dev].Money.FastOperation
(
    Id              int primary key not null,
    UserId          int             not null,
    FastOperationId int             not null,
    Name            nvarchar(max)   not null,
    Sum             decimal(18, 2)  not null,
    CategoryId      int,
    TypeId          int             not null,
    Comment         nvarchar(4000),
    PlaceId         int,
    [Order]         int
);
*/

type NewFastOperation struct {
	Id         int            `db:"id"`
	UserId     int            `db:"user_id"`
	Order      sql.NullInt32  `db:"order"`
	Name       string         `db:"name"`
	Sum        string         `db:"sum"`
	CategoryId int32          `db:"category_id"`
	Comment    sql.NullString `db:"comment"`
	PlaceId    sql.NullInt32  `db:"place_id"`
	IsDeleted  bool           `db:"is_deleted"`
}

/*
create table public.fast_operations
(
    user_id     integer                not null,
    id          integer                not null,
    "order"     integer,
    name        character varying(500) not null,
    sum         numeric                not null,
    category_id integer                not null,
    comment     character varying(4000),
    place_id    integer,
    is_deleted  boolean                not null
);
*/
