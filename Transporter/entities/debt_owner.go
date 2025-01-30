package entities

import (
	"database/sql"
)

type DebtOwnerMove struct {
	BaseTable
	OldRows []OldDebtOwner
	NewRows []NewDebtOwner
}

func (table *DebtOwnerMove) Move() {
	for i := 0; i < len(table.OldRows); i++ {
		row := NewDebtOwner{
			Id:     table.OldRows[i].Id,
			UserId: table.OldRows[i].UserId,
			Name:   table.OldRows[i].Name,
		}
		table.NewRows = append(table.NewRows, row)
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
