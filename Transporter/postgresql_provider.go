package main

import (
	"database/sql"
	"fmt"
	_ "github.com/lib/pq"
)

type sqlProvider struct {
	connectionString string
	state            bool
	db               *sql.DB
}

func (provider *sqlProvider) ExecuteInt(query string, args ...any) int64 {
	provider.OpenConnection()
	sqlRow := provider.db.QueryRow(query, args...)
	var val int64
	if err := sqlRow.Scan(&val); err != nil { // scan will release the connection
	}
	return val
}

func (provider *sqlProvider) ExecuteNonQuery(query string, args ...any) int64 {
	provider.OpenConnection()
	result, err := provider.db.Exec(query, args...)
	if err != nil {
		fmt.Println("Error execute: %v\n", err)
		return -1
	}
	r, _ := result.RowsAffected()
	return r
}

func (provider *sqlProvider) ExecuteQuery(query string, args ...any) (*sql.Rows, error) {
	provider.OpenConnection()
	return provider.db.Query(query, args...)
}

func (provider *sqlProvider) OpenConnection() {
	if provider.state == false {
		db, err := sql.Open("postgres", provider.connectionString)
		if err != nil {
			fmt.Println("Unable to connect to database: %v\n", err)
			return
		}
		provider.db = db
	}
}
