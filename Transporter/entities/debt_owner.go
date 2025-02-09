package entities

type DebtOwner struct {
	BaseTable[OldDebtOwner, NewDebtOwner]
}

func (table *DebtOwner) Transform(old OldDebtOwner) NewDebtOwner {
	return NewDebtOwner{
		Id:     old.DebtUserId,
		UserId: old.UserId,
		Name:   old.Name,
	}
}

type OldDebtOwner struct {
	Id         int    `db:"Id"`
	UserId     int    `db:"UserId"`
	DebtUserId int    `db:"DebtUserId"`
	Name       string `db:"Name"`
}

/*
create table [money-dev].Money.DebtUser
(
    Id         int primary key not null,
    UserId     int             not null,
    DebtUserId int             not null,
    Name       nvarchar(2000)  not null,
);
*/

type NewDebtOwner struct {
	Id     int    `db:"id"`
	UserId int    `db:"user_id"`
	Name   string `db:"name"`
}

/*
create table public.debt_owners
(
    user_id integer                not null,
    id      integer                not null,
    name    character varying(500) not null
);
*/
