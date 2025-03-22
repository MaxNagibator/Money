package entities

type Car struct {
	BaseTable[OldCar, NewCar]
}

func (table *Car) Transform(old OldCar) NewCar {
	return NewCar{
		Id:        old.CarId,
		UserId:    old.UserId,
		Name:      old.Name,
		IsDeleted: false,
	}
}

type OldCar struct {
	Id     int    `db:"Id"`
	UserId int    `db:"UserId"`
	CarId  int    `db:"CarId"`
	Name   string `db:"Name"`
}

/*
create table [money-dev].Money.Car
(
    Id     int primary key not null,
    UserId int             not null,
    CarId  int             not null,
    Name   nvarchar(1000)  not null,
);
*/

type NewCar struct {
	Id        int    `db:"id"`
	UserId    int    `db:"user_id"`
	Name      string `db:"name"`
	IsDeleted bool   `db:"is_deleted"`
}

/*
create table public.cars
(
    user_id    integer                 not null,
    id         integer                 not null,
    name       character varying(1000) not null,
    is_deleted boolean                 not null default false,
);
*/
