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
	columns, err := getColumnNamesFromType[N]()
	if err != nil {
		return "", fmt.Errorf("ошибка при получении колонок: %v", err)
	}
	return joinColumns(columns), nil
}

func (t *BaseTable[O, N]) GetEscapedColumnNames() (string, error) {
	columns, err := getColumnNamesFromType[O]()
	if err != nil {
		return "", fmt.Errorf("ошибка при получении колонок: %v", err)
	}
	return escapeColumns(columns), nil
}

func (t *BaseTable[O, N]) GetInsertColumnNames() (string, error) {
	columns, err := getColumnNamesFromType[N]()
	if err != nil {
		return "", fmt.Errorf("ошибка при получении колонок: %v", err)
	}
	return prepareInsertColumns(columns), nil
}

func getColumnNamesFromType[V any]() ([]string, error) {
	var model V
	tType := reflect.TypeOf(model)

	if tType.Kind() != reflect.Struct {
		return nil, fmt.Errorf("тип не является структурой")
	}

	columns := make([]string, 0, tType.NumField())
	for i := 0; i < tType.NumField(); i++ {
		if tag := tType.Field(i).Tag.Get("db"); tag != "" {
			columns = append(columns, tag)
		}
	}
	return columns, nil
}

func joinColumns(columns []string) string {
	var b strings.Builder
	for i, col := range columns {
		if i > 0 {
			b.WriteString(", ")
		}
		b.WriteString(col)
	}
	return b.String()
}

func escapeColumns(columns []string) string {
	var b strings.Builder
	for i, col := range columns {
		if i > 0 {
			b.WriteString(", ")
		}
		b.WriteString(`"`)
		b.WriteString(col)
		b.WriteString(`"`)
	}
	return b.String()
}

func prepareInsertColumns(columns []string) string {
	var b strings.Builder
	for i, col := range columns {
		if i > 0 {
			b.WriteString(", ")
		}
		b.WriteString(":")
		b.WriteString(col)
	}
	return b.String()
}
