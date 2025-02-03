package entities

import (
	"database/sql"
	"github.com/denisenkom/go-mssqldb"
	"strings"
)

type AuthUser struct {
	BaseTable[OldAuthUser, NewAuthUser]
}

func (table *AuthUser) Transform(old OldAuthUser) NewAuthUser {
	return NewAuthUser{
		Id:                 old.Guid.String(),
		UserName:           old.Login,
		UserNameNormalized: strings.ToUpper(old.Login),
		EmailConfirmed:     old.EmailConfirm,
		Email:              old.GetEmail(),
		EmailNormalized:    sql.NullString{String: strings.ToUpper(old.GetEmail().String)},
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
	Id           int                    `db:"Id"`
	Guid         mssql.UniqueIdentifier `db:"Guid"`
	Login        string                 `db:"Login"`
	Password     sql.NullString         `db:"Password"`
	Email        sql.NullString         `db:"Email"`
	EmailConfirm bool                   `db:"EmailConfirm"`
	CreateDate   sql.NullString         `db:"CreateDate"`
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
