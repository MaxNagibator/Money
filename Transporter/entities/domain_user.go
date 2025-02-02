package entities

import (
	"database/sql"
	"github.com/denisenkom/go-mssqldb"
)

type DomainUser struct {
	BaseTable[OldDomainUser, NewDomainUser]
}

func (table *DomainUser) Transform(old OldDomainUser) NewDomainUser {
	return NewDomainUser{
		Id:                    old.Id,
		TransporterPassword:   old.Password,
		TransporterCreateDate: old.CreateDate,
		AuthUserId:            old.Guid.String(),
	}
}

type OldDomainUser struct {
	Id           int                    `db:"Id"`
	Guid         mssql.UniqueIdentifier `db:"Guid"`
	Login        sql.NullString         `db:"Login"`
	Password     sql.NullString         `db:"Password"`
	Email        sql.NullString         `db:"Email"`
	EmailConfirm bool                   `db:"EmailConfirm"`
	CreateDate   sql.NullString         `db:"CreateDate"`
}

type NewDomainUser struct {
	Id                    int            `db:"id"`
	AuthUserId            string         `db:"auth_user_id"`
	TransporterCreateDate sql.NullString `db:"transporter_create_date"`
	TransporterEmail      sql.NullString `db:"transporter_email"`
	TransporterLogin      sql.NullString `db:"transporter_login"`
	TransporterPassword   sql.NullString `db:"transporter_password"`
}
