package main

import (
	"fmt"
	"reflect"
	"strings"
	. "transporter/entities"
)

type TransporterMapping struct {
	DebtOwnerMove DebtOwnerMove
	DebtMove      DebtMove
	//OperationMove OperationMove
}

type TableMapping interface {
	Move()
}

func Move(m TableMapping) {
	m.Move()
}

func GetMapping() TransporterMapping {
	mapping := TransporterMapping{
		DebtOwnerMove: DebtOwnerMove{
			BaseTable: BaseTable{
				OldName: "\"Money\".\"DebtUser\"",
				NewName: "debt_owners",
			},
			OldRows: []OldDebtOwner{},
			NewRows: []NewDebtOwner{},
		},
		DebtMove: DebtMove{
			BaseTable: BaseTable{
				OldName: "\"Money\".\"Debt\"",
				NewName: "debts",
			},
			OldRows: []OldDebt{},
			NewRows: []NewDebt{},
		},
	}
	return mapping
}

func GetColumnNames(slice interface{}, prefix string, postfix string) (string, error) {
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
			stringBuilder.WriteString(prefix + dbTag + postfix)
		}
	}

	return stringBuilder.String(), nil
}
