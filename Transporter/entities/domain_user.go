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
		Id:                     old.Id,
		TransporterPassword:    old.Password,
		TransporterCreateDate:  old.CreateDate,
		AuthUserId:             old.Guid.String(),
		NextOperationID:        old.NextOperationID,
		NextCategoryID:         old.NextCategoryID,
		NextPlaceID:            old.NextPlaceID,
		NextFastOperationID:    old.NextFastOperationID,
		NextRegularOperationID: old.NextRegularOperationID,
		NextDebtID:             old.NextDebtID,
		NextDebtOwnerID:        old.NextDebtOwnerID,
		NextCarEventID:         old.NextCarEventID,
		NextCarID:              old.NextCarID,
	}
}

type OldDomainUser struct {
	Id                     int                    `db:"Id"`
	Guid                   mssql.UniqueIdentifier `db:"Guid"`
	Login                  sql.NullString         `db:"Login"`
	Password               sql.NullString         `db:"Password"`
	Email                  sql.NullString         `db:"Email"`
	EmailConfirm           bool                   `db:"EmailConfirm"`
	CreateDate             sql.NullString         `db:"CreateDate"`
	NextOperationID        int                    `db:"next_operation_id"`
	NextCategoryID         int                    `db:"next_category_id"`
	NextPlaceID            int                    `db:"next_place_id"`
	NextFastOperationID    int                    `db:"next_fast_operation_id"`
	NextRegularOperationID int                    `db:"next_regular_operation_id"`
	NextDebtID             int                    `db:"next_debt_id"`
	NextDebtOwnerID        int                    `db:"next_debt_owner_id"`
	NextCarEventID         int                    `db:"next_car_event_id"`
	NextCarID              int                    `db:"next_car_id"`
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

type NewDomainUser struct {
	Id                     int            `db:"id"`
	AuthUserId             string         `db:"auth_user_id"`
	TransporterCreateDate  sql.NullString `db:"transporter_create_date"`
	TransporterEmail       sql.NullString `db:"transporter_email"`
	TransporterLogin       sql.NullString `db:"transporter_login"`
	TransporterPassword    sql.NullString `db:"transporter_password"`
	NextOperationID        int            `db:"next_operation_id"`
	NextCategoryID         int            `db:"next_category_id"`
	NextPlaceID            int            `db:"next_place_id"`
	NextFastOperationID    int            `db:"next_fast_operation_id"`
	NextRegularOperationID int            `db:"next_regular_operation_id"`
	NextDebtID             int            `db:"next_debt_id"`
	NextDebtOwnerID        int            `db:"next_debt_owner_id"`
	NextCarEventID         int            `db:"next_car_event_id"`
	NextCarID              int            `db:"next_car_id"`
}

/*
create table public.domain_users
(
    id                        integer primary key not null,
    auth_user_id              uuid                not null,
    next_category_id          integer             not null default 1,
    next_operation_id         integer             not null default 1,
    next_place_id             integer             not null default 1,
    next_fast_operation_id    integer             not null default 1,
    next_regular_operation_id integer             not null default 1,
    next_debt_id              integer             not null default 1,
    next_debt_owner_id        integer             not null default 1,
    row_version               bytea               not null default '\x'::bytea,
    transporter_create_date   timestamp with time zone,
    transporter_email         text,
    transporter_login         text,
    transporter_password      text
);
*/
