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

	test(newDatabase, oldDatabase, &tables.DebtOwnerMove)
	fmt.Println(tables.DebtOwnerMove.OldRows[0])
	fmt.Println(tables.DebtOwnerMove.OldRows[0].UserId)
	fmt.Println(tables.DebtOwnerMove.NewRows[0].UserId)
	//test(newDatabase, oldDatabase, &tables.DebtMove)
}

func test(newDatabase *sqlx.DB, oldDatabase *sqlx.DB, table TableMapping) {
	oldColumnNames := reflect.ValueOf(table).Elem().FieldByName("OldRows")
	oldName := reflect.ValueOf(table).Elem().FieldByName("OldName")
	selectColumns, err := GetColumnNames(oldColumnNames.Interface(), "\"", "\"")
	if err != nil {
		fmt.Println("Ошибка анализа OldColumns:", err)
	}

	fmt.Println("read old table")
	rows, err := oldDatabase.Queryx("SELECT " + selectColumns + " FROM " + oldName.String() + "")
	if err != nil {
		log.Fatalln(err)
	}

	oelemType := reflect.ValueOf(oldColumnNames.Interface()).Type().Elem()
	fmt.Println(oelemType)
	//array222 := reflect.MakeSlice(oelemType, 0, 0)
	array222 := reflect.MakeSlice(reflect.SliceOf(oelemType), 0, 0)
	for rows.Next() {
		intPtr := reflect.New(oelemType).Interface()
		err2 := rows.StructScan(intPtr)
		if err2 != nil {
			log.Fatalln(err2)
		}
		fmt.Println(intPtr)

		fmt.Println(reflect.ValueOf(intPtr).Elem())

		array222 = reflect.Append(array222, reflect.ValueOf(intPtr).Elem())
	}
	reflect.ValueOf(table).Elem().FieldByName("OldRows").Set(array222)

	//fmt.Println(array222)
	////fmt.Println("read old table complete")
	////val := reflect.ValueOf(oldColumnNames)
	////elemType := val.Type().Elem()
	////slice := reflect.Zero(reflect.SliceOf(elemType)).Interface()
	////arrayType := reflect.ArrayOf(0, elemType)
	////slice2 := reflect.Zero(arrayType).Interface()
	//oldRows := GetOldRows(table)
	//err = oldDatabase.Select(&oldRows, "SELECT "+selectColumns+" FROM "+oldName.String()+"")
	//if err != nil {
	//	fmt.Println(err)
	//	return
	//}
	//fmt.Println("huy")
	//
	//fmt.Println(oldColumnNames)

	Move(table)
	//table.Move()
	//
	////fmt.Println("insert new table")
	////
	////fmt.Println(tables.DebtMove.NewRows[0].Id)
	////fmt.Println(tables.DebtMove.NewRows[0].UserId)
	////fmt.Println(tables.DebtMove.NewRows[0].Comment.String)

	//insertColumns, err := GetColumnNames(tables.DebtMove.NewRows, "", "")
	//insertColumnsValues, err := GetColumnNames(tables.DebtMove.NewRows, ":", "")
	//insertQuery := "INSERT INTO " + tables.DebtMove.NewName + " (" + insertColumns + ")" +
	//	" VALUES(" + insertColumnsValues + ")"
	//fmt.Println(insertQuery)
	//_, err = newDatabase.NamedExec(insertQuery, tables.DebtMove.NewRows)
	//if err != nil {
	//	fmt.Println(err)
	//}
	//fmt.Println("complete")
}
