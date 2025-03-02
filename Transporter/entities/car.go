package entities

type Car struct {
    BaseTable[OldCar, NewCar]
}

func (table *Car) Transform(old OldCar) NewCar {
    return NewCar{
        Id:        old.DebtId,
        UserId:    old.UserId,
        Name:      old.Name,
        IsDeleted: false,
    }
}

type OldCar struct {
    Id     int    `db:"Id"`
    UserId int    `db:"UserId"`
    DebtId int    `db:"CarId"`
    Name   string `db:"Name"`
}

type NewCar struct {
    Id        int    `db:"id"`
    UserId    int    `db:"user_id"`
    Name      string `db:"name"`
    IsDeleted bool   `db:"is_deleted"`
}
