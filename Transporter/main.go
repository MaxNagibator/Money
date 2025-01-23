package main

import (
	"fmt"
	"github.com/jmoiron/sqlx"
	_ "github.com/lib/pq"
	"log"
)

func main() {
	const (
		host      = "localhost"
		port      = 5432
		user      = "postgres"
		password  = "RjirfLeyz"
		dbNewName = "money-dev2"
		dbOldName = "money-dev"
	)

	databaseConnectionString := fmt.Sprintf("host=%s port=%d user=%s "+
		"password=%s dbname=%s sslmode=disable",
		host, port, user, password, dbNewName)

	databaseOldConnectionString := fmt.Sprintf("host=%s port=%d user=%s "+
		"password=%s dbname=%s sslmode=disable",
		host, port, user, password, dbOldName)

	//var dbProvider = sqlProvider{
	//	connectionString: databaseConnectionString,
	//	state:            false,
	//}

	tables := GetMapping()
	fmt.Println(tables.DebtMove.OldName)
	fmt.Println(tables.DebtMove.BaseTable.NewName)

	//columns := []string{"user_id", "id", "date"}
	//sqlColumns := strings.Join(columns[:], ",")
	//user_id
	//	id integer NOT NULL,
	//	date date NOT NULL,
	//	sum numeric NOT NULL,
	//	type_id integer NOT NULL,
	//	comment character varying(4000) COLLATE pg_catalog."default",
	//	pay_sum numeric NOT NULL,
	//	status_id integer NOT NULL,
	//	pay_comment character varying(4000) COLLATE pg_catalog."default",
	//	owner_id integer NOT NULL,
	//	is_deleted boolean NOT NULL,

	db222, err := sqlx.Connect("postgres", databaseConnectionString)
	if err != nil {
		log.Fatalln(err)
	}
	db333, err := sqlx.Connect("postgres", databaseOldConnectionString)
	if err != nil {
		log.Fatalln(err)
	}

	//oldTags := GetDBTags(&tables.DebtMove.NewColumns[0])
	//newTags := GetDBTags(&tables.DebtMove.OldColumns[0])

	selectColumns, err := GetColumnNames(tables.DebtMove.NewColumns)
	if err != nil {
		fmt.Println("Ошибка анализа OldColumns:", err)
	}

	fmt.Println("read old table")
	err = db333.Select(&tables.DebtMove.OldColumns, "SELECT "+selectColumns+" FROM "+tables.DebtMove.OldName)
	if err != nil {
		fmt.Println(err)
		return
	}
	fmt.Println("huy")

	// todo будем делать вставку
	debts := new NewDebt{
		Id: tables.DebtMove.OldColumns[0].Id,
		Comment: tables.DebtMove.OldColumns[0].Comment,
	}
	fmt.Println("insert new table")
	err = db222.Select(&tables.DebtMove.NewColumns, "SELECT "+oldTags+" FROM "+tables.DebtMove.NewName)
	if err != nil {
		fmt.Println(err)
		return
	}

	fmt.Println(tables.DebtMove.NewColumns[0].Id)
	fmt.Println(tables.DebtMove.NewColumns[0].UserId)
	fmt.Println(tables.DebtMove.NewColumns[0].Comment.String)

	return
	//for index, table := range tables {
	//	fmt.Println(index)
	//
	//	sqlColumns := table.GetColumnNames(true)
	//	rows, err := dbProvider.ExecuteQuery("SELECT " + sqlColumns + " FROM public.debts")
	//	if err != nil {
	//		fmt.Println("Error execute: %v\n", err)
	//		return
	//	}
	//
	//	for rows.Next() {
	//		var id int
	//		rows.Columns()
	//		rows.Scan(&id)
	//		fmt.Println(id)
	//	}
	//}
	//rows, err := dbProvider.ExecuteQuery("SELECT " + sqlColumns + " FROM public.debts")
	//if err != nil {
	//	fmt.Println("Error execute: %v\n", err)
	//	return
	//}-
	//for rows.Next() {
	//	rowIndex++
	//	var userId int
	//	var id int
	//	var date string
	//	rows.Scan(&userId, &id, &date)
	//	fmt.Println(rowIndex, userId, id, date)
	//}
}
