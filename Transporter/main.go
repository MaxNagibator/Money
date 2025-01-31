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

	transporter := CreateTransporter()

	newDatabase, err := sqlx.Connect("postgres", databaseConnectionString)
	if err != nil {
		log.Fatalln(err)
	}
	oldDatabase, err := sqlx.Connect("postgres", databaseOldConnectionString)
	if err != nil {
		log.Fatalln(err)
	}

	ProcessTable(newDatabase, oldDatabase, &transporter.AuthUser)
	ProcessTable(newDatabase, oldDatabase, &transporter.DomainUser)
	ProcessTable(newDatabase, oldDatabase, &transporter.DebtOwner)
	ProcessTable(newDatabase, oldDatabase, &transporter.Debt)
}

func ProcessTable[O any, N any](newDatabase *sqlx.DB, oldDatabase *sqlx.DB, table TableMapping[O, N]) {
	baseTable := table.GetBaseTable()

	oldColumns, err := baseTable.GetEscapedColumnNames()
	if err != nil {
		log.Fatalln(err)
	}

	fmt.Println("read old table")
	selectQuery := "SELECT " + oldColumns + " FROM " + baseTable.OldName + ""
	fmt.Println(selectQuery)

	var oldRows []O
	err = oldDatabase.Select(&oldRows, selectQuery)
	if err != nil {
		log.Fatalln("ошибка при выполнении запроса: %v\n", err)
	}

	var newRows []N
	for _, oldRow := range oldRows {
		newRows = append(newRows, table.Transform(oldRow))
	}

	newColumns, err := baseTable.GetNewColumnNames()
	if err != nil {
		log.Fatalln(err)
	}

	insertColumns, err := baseTable.GetInsertColumnNames()
	if err != nil {
		log.Fatalln(err)
	}

	insertQuery := "INSERT INTO " + baseTable.NewName + " (" + newColumns + ")" +
		" VALUES(" + insertColumns + ")"

	fmt.Println(insertQuery)

	_, err = newDatabase.NamedExec(insertQuery, newRows)
	if err != nil {
		log.Fatalln(err)
	}

	fmt.Println("complete")
}
