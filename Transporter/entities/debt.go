package entities

import (
	"database/sql"
)

type Debt struct {
	BaseTable[OldDebt, NewDebt]
}

func (table *Debt) Transform(old OldDebt) NewDebt {
	return NewDebt{
		Id:         old.DebtId,
		UserId:     old.UserId,
		Date:       old.Date,
		Sum:        old.Sum,
		TypeId:     old.Type,
		Comment:    old.Comment,
		PaySum:     old.PaySum,
		StatusId:   old.StatusId,
		PayComment: old.PayComment,
		OwnerId:    old.DebtUserId,
		IsDeleted:  old.StatusId == 2,
	}
}

type OldDebt struct {
	Id         int            `db:"Id"`
	UserId     int            `db:"UserId"`
	DebtId     int            `db:"DebtId"`
	Date       string         `db:"Date"`
	Sum        string         `db:"Sum"`
	Type       int            `db:"Type"`
	Comment    sql.NullString `db:"Comment"`
	PaySum     string         `db:"PaySum"`
	StatusId   int            `db:"StatusId"`
	PayComment sql.NullString `db:"PayComment"`
	DebtUserId int            `db:"DebtUserId"`
}

/*
create table [money-dev].Money.Debt
(
    Id         int primary key not null,
    UserId     int             not null,
    DebtId     int             not null,
    Date       datetime        not null,
    Sum        decimal(18, 2)  not null,
    Type       int             not null,
    Comment    nvarchar(max),
    PaySum     decimal(18, 2)  not null,
    StatusId   int             not null,
    PayComment nvarchar(max),
    DebtUserId int             not null,
);
*/

type NewDebt struct {
	Id         int            `db:"id"`
	UserId     int            `db:"user_id"`
	Date       string         `db:"date"`
	Sum        string         `db:"sum"`
	TypeId     int            `db:"type_id"`
	Comment    sql.NullString `db:"comment"`
	PaySum     string         `db:"pay_sum"`
	StatusId   int            `db:"status_id"`
	PayComment sql.NullString `db:"pay_comment"`
	OwnerId    int            `db:"owner_id"`
	IsDeleted  bool           `db:"is_deleted"`
}

/*
create table public.debts
(
    user_id     integer 				not null,
    id          integer 				not null,
    date        date    				not null,
    sum         numeric 				not null,
    type_id     integer 				not null,
    comment     character varying(4000),
    pay_sum     numeric 				not null,
    status_id   integer 				not null,
    pay_comment character varying(4000),
    owner_id    integer					not null,
    is_deleted  boolean					not null,
);
*/
