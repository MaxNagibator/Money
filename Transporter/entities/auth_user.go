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
	Login        string                 `db:"Login"`
	Password     sql.NullString         `db:"Password"`
	CreateDate   sql.NullString         `db:"CreateDate"`
	Email        sql.NullString         `db:"Email"`
	EmailConfirm bool                   `db:"EmailConfirm"`
	Guid         mssql.UniqueIdentifier `db:"Guid"`
}

/*
create table [money-dev].System.[User]
(
    Id                int primary key   not null,
    Login             nvarchar(128)     not null,
    Password          nvarchar(max)     not null,
    Token             nvarchar(2048)    not null,
    CreateDate        datetime          not null,
    Email             nvarchar(1024),
    EmailConfirm      bit default ((0)) not null,
    EmailSendCode     nvarchar(max),
    EmailSendCodeDate datetime
);
*/

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

/*
create table public."AspNetUsers"
(
    id                     uuid primary key not null,
    user_name              character varying(256),
    normalized_user_name   character varying(256),
    email                  character varying(256),
    normalized_email       character varying(256),
    email_confirmed        boolean          not null,
    password_hash          text,
    security_stamp         text,
    concurrency_stamp      text,
    phone_number           text,
    phone_number_confirmed boolean          not null,
    two_factor_enabled     boolean          not null,
    lockout_end            timestamp with time zone,
    lockout_enabled        boolean          not null,
    access_failed_count    integer          not null
);
*/
