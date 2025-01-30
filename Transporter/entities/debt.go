package entities

import (
	"database/sql"
	"fmt"
)

type DebtMove struct {
	BaseTable
	OldRows []OldDebt
	NewRows []NewDebt
}

func (table *DebtMove) Move() {
	for i := 0; i < len(table.OldRows); i++ {
		row := NewDebt{
			Id:        table.OldRows[i].Id,
			UserId:    table.OldRows[i].UserId,
			Comment:   table.OldRows[i].Comment,
			Date:      table.OldRows[i].Date,
			Sum:       table.OldRows[i].Sum,
			TypeId:    table.OldRows[i].TypeId,
			PaySum:    table.OldRows[i].PaySum,
			StatusId:  table.OldRows[i].StatusId,
			OwnerId:   table.OldRows[i].OwnerId,
			IsDeleted: false,
		}
		table.NewRows = append(table.NewRows, row)
		fmt.Println(table.NewRows[i].OwnerId)
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
