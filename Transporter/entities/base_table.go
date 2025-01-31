package entities

import (
	"fmt"
	"reflect"
	"strings"
)

type BaseTable[O any, N any] struct {
	OldName string
	NewName string
}

func (t *BaseTable[O, N]) GetBaseTable() *BaseTable[O, N] {
	return t
}

func (t *BaseTable[O, N]) GetNewColumnNames() (string, error) {
	newColumns, err := getColumnNamesFromType[N]()
	if err != nil {
		return "", fmt.Errorf("ошибка при получении колонок: %v", err)
	}

	return strings.Join(newColumns, ", "), nil
}

func (t *BaseTable[O, N]) GetEscapedColumnNames() (string, error) {
	oldColumns, err := getColumnNamesFromType[O]()
	if err != nil {
		return "", fmt.Errorf("ошибка при получении колонок: %v", err)
	}

	escapedColumns := make([]string, len(oldColumns))
	for i, col := range oldColumns {
		escapedColumns[i] = `"` + col + `"`
	}

	return strings.Join(escapedColumns, ", "), nil
}

func (t *BaseTable[O, N]) GetInsertColumnNames() (string, error) {
	newColumns, err := getColumnNamesFromType[N]()
	if err != nil {
		return "", fmt.Errorf("ошибка при получении колонок: %v", err)
	}

	insertColumns := make([]string, len(newColumns))
	for i, col := range newColumns {
		insertColumns[i] = ":" + col
	}

	return strings.Join(insertColumns, ", "), nil
}

func getColumnNamesFromType[V any]() ([]string, error) {
	var model V
	tType := reflect.TypeOf(model)

	if tType.Kind() != reflect.Struct {
		return nil, fmt.Errorf("тип не является структурой")
	}

	var columns []string

	for i := 0; i < tType.NumField(); i++ {
		field := tType.Field(i)
		tag := field.Tag.Get("db")
		if tag != "" {
			columns = append(columns, tag)
		}
	}

	return columns, nil
}
