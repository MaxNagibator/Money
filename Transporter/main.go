package main

import (
	"database/sql"
	"fmt"
	"github.com/jmoiron/sqlx"
	_ "github.com/lib/pq"
	"log"
)

func main() {
	const (
		host     = "localhost"
		port     = 5432
		user     = "postgres"
		password = "RjirfLeyz"
		dbname   = "money-dev2"
	)

	databaseConnectionString := fmt.Sprintf("host=%s port=%d user=%s "+
		"password=%s dbname=%s sslmode=disable",
		host, port, user, password, dbname)

	var dbProvider = sqlProvider{
		connectionString: databaseConnectionString,
		state:            false,
	}

	tables := GetTables()
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

	type Debt struct {
		Id      int `db:"id"`
		UserId  int `db:"user_id"`
		Comment sql.NullString
	}

	debts := []Debt{}

	db222, err := sqlx.Connect("postgres", databaseConnectionString)
	if err != nil {
		log.Fatalln(err)
	}

	err = db222.Select(&debts, "SELECT id, user_id, comment FROM debts")
	if err != nil {
		fmt.Println(err)
		return
	}
	fmt.Println(debts[0].Id)
	fmt.Println(debts[0].UserId)
	fmt.Println(debts[0].Comment.String)
	return
	for index, table := range tables {
		fmt.Println(index)

		sqlColumns := table.GetColumnNames(true)
		rows, err := dbProvider.ExecuteQuery("SELECT " + sqlColumns + " FROM public.debts")
		if err != nil {
			fmt.Println("Error execute: %v\n", err)
			return
		}

		for rows.Next() {
			var id int
			rows.Columns()
			rows.Scan(&id)
			fmt.Println(id)
		}
	}
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
