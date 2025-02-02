//go:debug x509negativeserial=1
package main

import (
	"fmt"
	_ "github.com/denisenkom/go-mssqldb"
	"github.com/jmoiron/sqlx"
	_ "github.com/lib/pq"
	"log"
)

type config struct {
	host     string
	port     int
	user     string
	password string
	dbName   string
	provider string
}

func main() {
	newConfig := config{
		host:     "localhost",
		port:     5432,
		user:     "postgres",
		password: "RjirfLeyz",
		dbName:   "money-dev3",
		provider: "postgres",
	}

	oldConfig := config{
		host:     "localhost",
		port:     1433,
		user:     "money",
		password: "money",
		dbName:   "money-dev",
		provider: "sqlserver",
	}

	oldDatabaseConnectionString := fmt.Sprintf("Data Source=%s,%d;Initial Catalog=%s;"+
		"Persist Security Info=True;User ID=%s;Password=%s;TrustServerCertificate=True;",
		oldConfig.host, oldConfig.port, oldConfig.dbName, oldConfig.user, oldConfig.password)

	newDatabaseConnectionString := fmt.Sprintf("host=%s port=%d user=%s "+
		"password=%s dbname=%s sslmode=disable",
		newConfig.host, newConfig.port, newConfig.user, newConfig.password, newConfig.dbName)

	oldDatabase, err := sqlx.Connect(oldConfig.provider, oldDatabaseConnectionString)
	if err != nil {
		log.Fatalln("Old connect\n", err)
	}

	newDatabase, err := sqlx.Connect(newConfig.provider, newDatabaseConnectionString)
	if err != nil {
		log.Fatalln("New connect\n", err)
	}

	transporter := CreateTransporter()

	ProcessTable(newDatabase, oldDatabase, &transporter.AuthUser)
	ProcessTable(newDatabase, oldDatabase, &transporter.DomainUser)
	ProcessTable(newDatabase, oldDatabase, &transporter.DebtOwner)
	ProcessTable(newDatabase, oldDatabase, &transporter.Debt)
	ProcessTable(newDatabase, oldDatabase, &transporter.Category)
	ProcessTable(newDatabase, oldDatabase, &transporter.Operation)
}

func ProcessTable[O any, N any](newDatabase *sqlx.DB, oldDatabase *sqlx.DB, table TableMapping[O, N]) {
	baseTable := table.GetBaseTable()

	oldColumns, _ := baseTable.GetEscapedColumnNames()

	fmt.Println("read old table", baseTable.OldName)
	selectQuery := fmt.Sprintf("SELECT %s FROM %s", oldColumns, baseTable.OldName)

	var oldRows []O
	err := oldDatabase.Select(&oldRows, selectQuery)
	if err != nil {
		log.Fatalln("Select\n", err)
	}

	var newRows []N
	for _, oldRow := range oldRows {
		newRow := table.Transform(oldRow)
		newRows = append(newRows, newRow)
	}

	newColumns, _ := baseTable.GetNewColumnNames()
	insertColumns, _ := baseTable.GetInsertColumnNames()

	insertQuery := fmt.Sprintf(
		"INSERT INTO %s (%s) VALUES(%s)",
		baseTable.NewName,
		newColumns,
		insertColumns,
	)

	_, err = newDatabase.NamedExec(insertQuery, newRows)
	if err != nil {
		log.Fatalln("NamedExec\n", err)
	}

	fmt.Println("complete")
}
