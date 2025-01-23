package main

import (
	"strings"
)

type Table struct {
	OldName string
	NewName string
	Columns []Column
}

func (table *Table) GetColumnNames(isOld bool) string {
	var stringBuilder strings.Builder
	for index, column := range table.Columns {
		if index > 0 {
			stringBuilder.WriteString(",")
		}
		if isOld {
			stringBuilder.WriteString(column.OldName)
		} else {
			stringBuilder.WriteString(column.NewName)
		}
	}
	return stringBuilder.String()
}

type Column struct {
	OldName string
	NewName string
}

func GetTables() []Table {
	tables := []Table{
		{
			OldName: "Debt",
			NewName: "debt",
			Columns: []Column{
				{
					OldName: "Id",
					NewName: "id",
				},
				{
					OldName: "UserId",
					NewName: "user_id",
				},
			},
		},
	}
	return tables
}
