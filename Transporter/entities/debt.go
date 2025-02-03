package entities

import (
	"database/sql"
)

type Debt struct {
	BaseTable[OldDebt, NewDebt]
}

func (table *Debt) Transform(old OldDebt) NewDebt {
	return NewDebt{
		Id:        old.Id,
		UserId:    old.UserId,
		Comment:   old.Comment,
		Date:      old.Date,
		Sum:       old.Sum,
		TypeId:    old.TypeId,
		PaySum:    old.PaySum,
		StatusId:  old.StatusId,
		OwnerId:   old.OwnerId,
		IsDeleted: false,
	}
}

type OldDebt struct {
	Id       int            `db:"Id"`
	UserId   int            `db:"UserId"`
	Comment  sql.NullString `db:"Comment"`
	Date     string         `db:"Date"`
	Sum      string         `db:"Sum"`
	TypeId   int            `db:"Type"`
	PaySum   string         `db:"PaySum"`
	StatusId string         `db:"StatusId"`
	OwnerId  string         `db:"DebtUserId"`
}

type NewDebt struct {
	Id        int            `db:"id"`
	UserId    int            `db:"user_id"`
	Comment   sql.NullString `db:"comment"`
	Date      string         `db:"date"`
	Sum       string         `db:"sum"`
	TypeId    int            `db:"type_id"`
	PaySum    string         `db:"pay_sum"`
	StatusId  string         `db:"status_id"`
	OwnerId   string         `db:"owner_id"`
	IsDeleted bool           `db:"is_deleted"`
}
