package entities

import (
	"database/sql"
)

type DebtOwner struct {
	BaseTable[OldDebtOwner, NewDebtOwner]
}

func (table *DebtOwner) Transform(old OldDebtOwner) NewDebtOwner {
	return NewDebtOwner{
		Id:     old.Id,
		UserId: old.UserId,
		Name:   old.Name,
	}
}

type OldDebtOwner struct {
	Id     int            `db:"DebtUserId"`
	UserId int            `db:"UserId"`
	Name   sql.NullString `db:"Name"`
}

type NewDebtOwner struct {
	Id     int            `db:"id"`
	UserId int            `db:"user_id"`
	Name   sql.NullString `db:"name"`
}
