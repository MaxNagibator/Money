package entities

import (
	"database/sql"
)

type DomainUser struct {
	BaseTable
	OldRows []OldDomainUser
	NewRows []NewDomainUser
}

// ALTER TABLE [System].[User]
// ADD Guid uniqueidentifier NULL;
// UPDATE [System].[User]
// SET Guid = NEWID()
func (table *DomainUser) Move() {
	for i := 0; i < len(table.OldRows); i++ {
		row := NewDomainUser{
			Id:                    table.OldRows[i].Id,
			TransporterPassword:   table.OldRows[i].Password,
			TransporterCreateDate: table.OldRows[i].CreateDate,
			AuthUserId:            table.OldRows[i].Guid,
		}
		table.NewRows = append(table.NewRows, row)
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
	Guid         sql.NullString `db:"Guid"`
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
