package entities

import (
	"database/sql"
	"fmt"
	"strings"
)

type AuthUser struct {
	BaseTable
	OldRows []OldAuthUser
	NewRows []NewAuthUser
}

func (table *AuthUser) Move() {
	for i := 0; i < len(table.OldRows); i++ {
		row := NewAuthUser{
			Id:                 table.OldRows[i].Guid,
			UserName:           table.OldRows[i].Login,
			UserNameNormalized: strings.ToUpper(table.OldRows[i].Login),
			EmailConfirmed:     table.OldRows[i].EmailConfirm,
			Email:              table.OldRows[i].GetEmail(),
			EmailNormalized:    sql.NullString{String: strings.ToUpper(table.OldRows[i].GetEmail().String)},
		}
		table.NewRows = append(table.NewRows, row)
		fmt.Println(table.NewRows[i].UserName)
	}
}

func (user *OldAuthUser) GetEmail() sql.NullString {
	if user.EmailConfirm == true {
		return user.Email
	} else {
		return sql.NullString{}
	}
}

type OldAuthUser struct {
	Id           int            `db:"Id"`
	Guid         string         `db:"Guid"`
	Login        string         `db:"Login"`
	Password     sql.NullString `db:"Password"`
	Email        sql.NullString `db:"Email"`
	EmailConfirm bool           `db:"EmailConfirm"`
	CreateDate   sql.NullString `db:"CreateDate"`
}

type NewAuthUser struct {
	Id                   string         `db:"id"`
	UserName             string         `db:"user_name"`
	UserNameNormalized   string         `db:"normalized_user_name"`
	Email                sql.NullString `db:"email"`
	EmailNormalized      sql.NullString `db:"normalized_email"`
	EmailConfirmed       bool           `db:"email_confirmed"`
	PhoneNumberConfirmed bool           `db:"phone_number_confirmed"`
	TwoFactorEnabled     bool           `db:"two_factor_enabled"`
	LockoutEnabled       bool           `db:"lockout_enabled"`
	AccessFailedCount    int            `db:"access_failed_count"`
}
