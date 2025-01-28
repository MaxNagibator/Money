package main

import (
	"database/sql"
	"fmt"
)

type DomainUser struct {
	BaseTable
	OldRows []OldDomainUser
	NewRows []NewDomainUser
}

func (table *DomainUser) Move() {
	for i := 0; i < len(table.OldRows); i++ {
		row := NewDomainUser{
			Id: table.OldRows[i].Id,
			//TransporterLogin:      table.OldRows[i].Login,
			//TransporterEmail:      table.OldRows[i].GetEmail(),
			TransporterPassword:   table.OldRows[i].Password,
			TransporterCreateDate: table.OldRows[i].CreateDate,
			AuthUserId:            sql.NullString{String: "4377d3e7-6ab8-4ea9-b27f-d44fc25c902c", Valid: true},
		}
		table.NewRows = append(table.NewRows, row)
		fmt.Println(table.NewRows[i].AuthUserId)
	}
}

func (user *OldDomainUser) GetEmail() sql.NullString {
	if user.EmailConfirm == true {
		return user.Email
	} else {
		return sql.NullString{}
	}
}

type OldDomainUser struct {
	Id           int            `db:"Id"`
	Login        sql.NullString `db:"Login"`
	Password     sql.NullString `db:"Password"`
	Email        sql.NullString `db:"Email"`
	EmailConfirm bool           `db:"EmailConfirm"`
	CreateDate   sql.NullString `db:"CreateDate"`
}

type NewDomainUser struct {
	Id                    int            `db:"id"`
	AuthUserId            sql.NullString `db:"auth_user_id"`
	TransporterCreateDate sql.NullString `db:"transporter_create_date"`
	TransporterEmail      sql.NullString `db:"transporter_email"`
	TransporterLogin      sql.NullString `db:"transporter_login"`
	TransporterPassword   sql.NullString `db:"transporter_password"`
}
