package entities

import (
	"database/sql"
)

type AuthUser struct {
	BaseTable
	OldRows []OldAuthUser
	NewRows []NewAuthUser
}

func (table *AuthUser) Move() {
	for i := 0; i < len(table.OldRows); i++ {
		row := NewAuthUser{
			Id:                    table.OldRows[i].Id,
			TransporterLogin:      table.OldRows[i].Login,
			TransporterEmail:      table.OldRows[i].GetEmail(),
			TransporterPassword:   table.OldRows[i].Password,
			TransporterCreateDate: table.OldRows[i].CreateDate,
			AuthUserId:            sql.NullString{String: "4377d3e7-6ab8-4ea9-b27f-d44fc25c902c", Valid: true},
		}
		table.NewRows = append(table.NewRows, row)
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
	Login        sql.NullString `db:"Login"`
	Password     sql.NullString `db:"Password"`
	Email        sql.NullString `db:"Email"`
	EmailConfirm bool           `db:"EmailConfirm"`
	CreateDate   sql.NullString `db:"CreateDate"`
}

type NewAuthUser struct {
	Id                 int            `db:"id"`
	UserName           sql.NullString `db:"user_name"`
	UserNameNormalized sql.NullString `db:"normalized_user_name"`
	Email              sql.NullString `db:"email"`
	EmailNormalized    sql.NullString `db:"normalized_email"`
	EmailConfirmed     sql.NullString `db:"email_confirmed"`
}
