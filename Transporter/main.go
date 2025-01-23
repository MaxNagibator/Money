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

	newDatabase, err := sqlx.Connect("postgres", databaseConnectionString)
	if err != nil {
		log.Fatalln(err)
	}
	oldDatabase, err := sqlx.Connect("postgres", databaseOldConnectionString)
	if err != nil {
		log.Fatalln(err)
	}

	//oldTags := GetDBTags(&tables.DebtMove.NewColumns[0])
	//newTags := GetDBTags(&tables.DebtMove.OldColumns[0])

	selectColumns, err := GetColumnNames(tables.DebtMove.OldRows, "\"", "\"")
	if err != nil {
		fmt.Println("Ошибка анализа OldColumns:", err)
	}

	fmt.Println("read old table")
	err = oldDatabase.Select(&tables.DebtMove.OldRows, "SELECT "+selectColumns+" FROM "+tables.DebtMove.OldName+"")
	if err != nil {
		fmt.Println(err)
		return
	}
	fmt.Println("huy")

	fmt.Println(tables.DebtMove.OldRows[0].Id)
	fmt.Println(tables.DebtMove.OldRows[0].UserId)
	fmt.Println(tables.DebtMove.OldRows[0].Comment.String)

	tables.DebtMove.Move()

	fmt.Println("insert new table")

	fmt.Println(tables.DebtMove.NewRows[0].Id)
	fmt.Println(tables.DebtMove.NewRows[0].UserId)
	fmt.Println(tables.DebtMove.NewRows[0].Comment.String)

	insertColumns, err := GetColumnNames(tables.DebtMove.NewRows, "", "")
	insertColumnsValues, err := GetColumnNames(tables.DebtMove.NewRows, ":", "")
	insertQuery := "INSERT INTO " + tables.DebtMove.NewName + " (" + insertColumns + ")" +
		" VALUES(" + insertColumnsValues + ")"
	fmt.Println(insertQuery)
	_, err = newDatabase.NamedExec(insertQuery, tables.DebtMove.NewRows)
	if err != nil {
		fmt.Println(err)
	}
	fmt.Println("complete")
}
