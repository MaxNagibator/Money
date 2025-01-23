package main

import (
	"database/sql"
	"fmt"
	"reflect"
	"strings"
)

type TransporterMapping struct {
	DebtMove DebtMove
	//OperationMove OperationMove
}

type BaseTable struct {
	OldName string
	NewName string
}

type DebtMove struct {
	BaseTable
	OldColumns []OldDebt
	NewColumns []NewDebt
}

type OldDebt struct {
	Id      int            `db:"Id"`
	UserId  int            `db:"UserId"`
	Comment sql.NullString `db:"Comment"`
}

type NewDebt struct {
	Id      int            `db:"id"`
	UserId  int            `db:"user_id"`
	Comment sql.NullString `db:"comment"`
}

func GetMapping() TransporterMapping {
	mapping := TransporterMapping{
		DebtMove: DebtMove{
			BaseTable: BaseTable{
				OldName: "Debts",
				NewName: "debts",
			},
			OldColumns: []OldDebt{},
			NewColumns: []NewDebt{},
		},
	}
	return mapping
}

func GetColumnNames(slice interface{}) (string, error) {
	// Получаем тип переданного значения
	val := reflect.ValueOf(slice)
	if val.Kind() != reflect.Slice {
		return "", fmt.Errorf("ожидался слайс, получен %v", val.Kind())
	}

	// Получаем тип элемента слайса
	elemType := val.Type().Elem()

	// Если элемент слайса - это указатель, получаем тип, на который он указывает
	if elemType.Kind() == reflect.Ptr {
		elemType = elemType.Elem()
	}
	var stringBuilder strings.Builder
	isFirst := true

	// Анализируем поля структуры
	for i := 0; i < elemType.NumField(); i++ {
		field := elemType.Field(i)
		dbTag := field.Tag.Get("db")
		if dbTag != "" {
			if isFirst == false {
				stringBuilder.WriteString(",")
			}
			isFirst = false
			stringBuilder.WriteString(dbTag)
		}
	}

	return stringBuilder.String(), nil
}
