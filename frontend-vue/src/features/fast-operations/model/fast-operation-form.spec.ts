import { describe, expect, it } from 'vitest'
import type { Category } from '@/domain/category'
import { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import type { FastOperationFormValue } from './fast-operation-form'
import {
  CATEGORY_REQUIRED,
  categoryOptions,
  editFastOperationForm,
  NAME_REQUIRED,
  newFastOperationForm,
  searchCategories,
  shouldResetPlaces,
  toFastOperationDraft,
  validateFastOperationForm,
} from './fast-operation-form'
import type { FastOperationView } from './fast-operation-list'

function category(id: number, name: string, operationTypeId: OperationTypeId, parentId: number | null = null): Category {
  return { id, parentId, name, color: null, order: null, operationTypeId, isDeleted: false }
}

const expenses = category(1, 'Еда', 1)
const expensesChild = category(2, 'Кофе', 1, 1)
const incomes = category(3, 'Работа', 2)

const all = [incomes, expensesChild, expenses]

function view(extra: Partial<FastOperationView> = {}): FastOperationView {
  return {
    id: 7,
    name: 'Кофе на вынос',
    order: 3,
    categoryId: expenses.id,
    sum: Money.parseInput('100')!,
    comment: 'утро',
    place: 'Кафе',
    isDeleted: false,
    category: expenses,
    ...extra,
  }
}

function form(extra: Partial<FastOperationFormValue> = {}): FastOperationFormValue {
  return { ...newFastOperationForm(1), name: 'Кофе', categoryId: expenses.id, sum: Money.parseInput('100')!, ...extra }
}

describe('список категорий в диалоге', () => {
  it('выбранный тип оставляет только свои категории в порядке дерева', () => {
    expect(categoryOptions(all, 1).map(item => item.id)).toEqual([expenses.id, expensesChild.id])
    expect(categoryOptions(all, 2).map(item => item.id)).toEqual([incomes.id])
  })

  it('без типа отдаёт все категории', () => {
    expect(categoryOptions(all, null).map(item => item.id)).toEqual([expenses.id, expensesChild.id, incomes.id])
  })

  it('при изменении тип берётся из категории операции, у неизвестной остаются все', () => {
    expect(editFastOperationForm(view()).operationTypeId).toBe(1)
    expect(editFastOperationForm(view({ category: incomes })).operationTypeId).toBe(2)
    expect(editFastOperationForm(view({ category: null })).operationTypeId).toBeNull()
  })

  it('исчезнувшая из справочника категория сохраняет идентификатор', () => {
    expect(editFastOperationForm(view({ category: null, categoryId: 99 })).categoryId).toBe(99)
  })
})

describe('поиск категории', () => {
  it('ищет подстроку без учёта регистра', () => {
    expect(searchCategories(all, 'кОф').map(item => item.id)).toEqual([expensesChild.id])
    expect(searchCategories(all, 'РАБ').map(item => item.id)).toEqual([incomes.id])
  })

  it('пустой запрос отдаёт весь список', () => {
    expect(searchCategories(all, '   ')).toHaveLength(all.length)
  })
})

describe('обязательные поля', () => {
  it('наименование и категория обязательны', () => {
    const errors = validateFastOperationForm(form({ name: '  ', categoryId: null }))

    expect(errors).toEqual({ name: NAME_REQUIRED, categoryId: CATEGORY_REQUIRED })
  })

  it('заполненная форма ошибок не даёт, а черновик несёт обрезанное имя', () => {
    const value = form({ name: '  Кофе  ' })

    expect(validateFastOperationForm(value)).toEqual({})
    expect(toFastOperationDraft(value).name).toBe('Кофе')
  })

  it('пустые место и комментарий уходят как отсутствующие', () => {
    const draft = toFastOperationDraft(form({ place: '   ', comment: '' }))

    expect(draft.place).toBeNull()
    expect(draft.comment).toBeNull()
  })
})

describe('открытие диалога', () => {
  it('создание начинается с пустых полей и без категории', () => {
    expect(newFastOperationForm(1)).toEqual({
      id: null,
      operationTypeId: 1,
      categoryId: null,
      name: '',
      sum: null,
      order: null,
      comment: '',
      place: '',
    })
  })

  it('изменение переносит поля операции, нулевая сумма показывается пустой', () => {
    const value = editFastOperationForm(view({ sum: Money.zero, comment: null, place: null }))

    expect(value).toMatchObject({
      id: 7,
      categoryId: expenses.id,
      name: 'Кофе на вынос',
      order: 3,
      sum: null,
      comment: '',
      place: '',
    })
  })
})

describe('кэш мест', () => {
  it('сбрасывается только когда место непустое и отличается от прежнего', () => {
    expect(shouldResetPlaces('Кафе', 'Пекарня')).toBe(true)
    expect(shouldResetPlaces(null, 'Пекарня')).toBe(true)
    expect(shouldResetPlaces('Кафе', 'Кафе')).toBe(false)
    expect(shouldResetPlaces('Кафе', '   ')).toBe(false)
    expect(shouldResetPlaces(null, '')).toBe(false)
  })
})
