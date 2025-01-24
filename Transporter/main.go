package main

import (
	"fmt"
	"github.com/jmoiron/sqlx"
	_ "github.com/lib/pq"
	"log"
	"reflect"
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

	tables := GetMapping()

	newDatabase, err := sqlx.Connect("postgres", databaseConnectionString)
	if err != nil {
		log.Fatalln(err)
	}
	oldDatabase, err := sqlx.Connect("postgres", databaseOldConnectionString)
	if err != nil {
		log.Fatalln(err)
	}

	test(newDatabase, oldDatabase, &tables.DebtOwnerMove)
	test(newDatabase, oldDatabase, &tables.DebtMove)
}

func test(newDatabase *sqlx.DB, oldDatabase *sqlx.DB, table TableMapping) {
	oldColumnNames := reflect.ValueOf(table).Elem().FieldByName("OldRows")
	oldName := reflect.ValueOf(table).Elem().FieldByName("OldName").String()
	selectColumns, err := GetColumnNames(oldColumnNames.Interface(), "\"", "\"")
	if err != nil {
		fmt.Println("Ошибка анализа OldColumns:", err)
	}

	fmt.Println("read old table")
	rows, err := oldDatabase.Queryx("SELECT " + selectColumns + " FROM " + oldName + "")
	if err != nil {
		log.Fatalln(err)
	}

	readArrayType := reflect.ValueOf(oldColumnNames.Interface()).Type().Elem()
	oldRowsArray := reflect.MakeSlice(reflect.SliceOf(readArrayType), 0, 0)
	for rows.Next() {
		intPtr := reflect.New(readArrayType).Interface()
		err2 := rows.StructScan(intPtr)
		if err2 != nil {
			log.Fatalln(err2)
		}
		oldRowsArray = reflect.Append(oldRowsArray, reflect.ValueOf(intPtr).Elem())
	}
	reflect.ValueOf(table).Elem().FieldByName("OldRows").Set(oldRowsArray)

	Move(table)

	newColumnNames := reflect.ValueOf(table).Elem().FieldByName("NewRows")
	insertColumns, err := GetColumnNames(newColumnNames.Interface(), "", "")
	insertColumnsValues, err := GetColumnNames(newColumnNames.Interface(), ":", "")
	newName := reflect.ValueOf(table).Elem().FieldByName("NewName").String()

	insertQuery := "INSERT INTO " + newName + " (" + insertColumns + ")" +
		" VALUES(" + insertColumnsValues + ")"

	_, err = newDatabase.NamedExec(insertQuery, newColumnNames.Interface())
	if err != nil {
		fmt.Println(err)
	}
	fmt.Println("complete")
}
