import { describe, expect, it } from 'vitest'
import type { RegularOperation } from '@/api/adapters/regular-operations'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import type { RegularOperationFormValue } from './regular-operation-form'
import {
  categoryOptions,
  COMMENT_MAX_LENGTH,
  DATE_TO_BEFORE_FROM,
  DATE_UNPARSED,
  dateFieldError,
  editRegularOperationForm,
  FIELD_REQUIRED,
  NAME_MAX_LENGTH,
  newRegularOperationForm,
  outOfRangeMessage,
  PLACE_MAX_LENGTH,
  searchCategories,
  shouldResetPlaces,
  SUM_UNPARSED,
  toRegularOperationDraft,
  tooLongMessage,
  validateRegularOperationForm,
} from './regular-operation-form'
import type { RegularOperationView } from './regular-operation-list'
import { timeValueHint, timeValueRules } from './time-type'

const food = category(1, 'Еда', 1)
const coffee = category(2, 'Кофе', 1, food.id)
const salary = category(3, 'Зарплата', 2)
const all = [salary, coffee, food]

const TODAY = localDate('2026-08-23')

describe('значения формы', () => {
  it('создание начинается с сегодняшней даты, месячного шага и нулевой суммы', () => {
    expect(newRegularOperationForm(TODAY)).toEqual({
      id: null,
      name: '',
      categoryId: null,
      sum: Money.zero,
      comment: '',
      place: '',
      timeTypeId: 3,
      timeValue: 1,
      dateFrom: TODAY,
      dateTo: null,
    })
  })

  it('изменение переносит поля операции', () => {
    const value = editRegularOperationForm(view({
      id: 7,
      name: 'Аренда',
      comment: 'за месяц',
      place: 'Дом',
      timeTypeId: 2,
      timeValue: 5,
      dateTo: localDate('2026-12-31'),
    }))

    expect(value).toEqual({
      id: 7,
      name: 'Аренда',
      categoryId: food.id,
      sum: Money.parseInput('100'),
      comment: 'за месяц',
      place: 'Дом',
      timeTypeId: 2,
      timeValue: 5,
      dateFrom: localDate('2026-08-23'),
      dateTo: localDate('2026-12-31'),
    })
  })

  it('исчезнувшая из справочника категория сохраняет идентификатор', () => {
    expect(editRegularOperationForm(view({ category: null, categoryId: 99 })).categoryId).toBe(99)
  })
})

describe('обязательные поля', () => {
  it('наименование, категория, временной шаг и дата начала просят заполнить', () => {
    const errors = validateRegularOperationForm(form({
      name: '   ',
      categoryId: null,
      timeTypeId: null,
      dateFrom: null,
    }))

    expect(errors).toEqual({
      name: FIELD_REQUIRED,
      categoryId: FIELD_REQUIRED,
      timeTypeId: FIELD_REQUIRED,
      dateFrom: FIELD_REQUIRED,
    })
  })

  it('место и комментарий не обязательны', () => {
    expect(validateRegularOperationForm(form({ place: '', comment: '' }))).toEqual({})
  })

  it('черновик несёт обрезанное имя, пустые место и комментарий уходят как отсутствующие', () => {
    const draft = toRegularOperationDraft(form({ name: ' Аренда ', place: '  ', comment: '' }))

    expect(draft.name).toBe('Аренда')
    expect(draft.place).toBeNull()
    expect(draft.comment).toBeNull()
  })

  it('нераспознанная сумма не даёт собрать черновик', () => {
    expect(() => toRegularOperationDraft(form({ sum: null }))).toThrow(SUM_UNPARSED)
  })

  it.each([
    ['пустое поле даты просит заполнить', null, null, FIELD_REQUIRED],
    ['неразобранный ввод даты сообщает об этом', localDate('2026-08-23'), null, DATE_UNPARSED],
    ['разобранная дата ошибки не даёт', localDate('2026-08-23'), localDate('2026-08-24'), ''],
  ] as const)('%s', (_, shown, parsed, expected) => {
    expect(dateFieldError(shown, parsed)).toBe(expected)
  })
})

describe('значение временного шага', () => {
  it.each([
    [1, 'Каждый день'],
    [2, 'Каждую неделю'],
    [3, 'Каждый месяц'],
  ])('шаг %i требует значение только кроме ежедневного', (timeTypeId, name) => {
    const errors = validateRegularOperationForm(form({ timeTypeId, timeValue: null }))

    expect(errors.timeValue).toBe(name === 'Каждый день' ? undefined : FIELD_REQUIRED)
  })

  it('для ежедневного шага значение не уходит на сервер', () => {
    expect(toRegularOperationDraft(form({ timeTypeId: 1, timeValue: 5 })).timeValue).toBeNull()
  })

  it('для недельного шага значение уходит как есть', () => {
    expect(toRegularOperationDraft(form({ timeTypeId: 2, timeValue: 5 })).timeValue).toBe(5)
  })

  it.each([
    [2, 0, false],
    [2, 1, true],
    [2, 7, true],
    [2, 8, false],
    [3, 0, false],
    [3, 31, true],
    [3, 32, false],
    [3, 1.5, false],
  ])('шаг %i со значением %s допустим: %s', (timeTypeId, timeValue, isValid) => {
    const errors = validateRegularOperationForm(form({ timeTypeId, timeValue }))
    const rules = timeValueRules(timeTypeId)!

    expect(errors.timeValue).toBe(isValid ? undefined : outOfRangeMessage(rules))
  })

  it.each([
    [1, null],
    [2, 'День недели'],
    [3, 'День месяца'],
  ])('подпись значения шага %i: %s', (timeTypeId, label) => {
    expect(timeValueRules(timeTypeId)?.label ?? null).toBe(label)
  })

  it.each([
    [1, 'Понедельник'],
    [7, 'Воскресенье'],
  ])('день недели %i подсказывается как %s', (timeValue, hint) => {
    expect(timeValueHint(2, timeValue)).toBe(hint)
  })
})

describe('границы значений', () => {
  it('дата окончания раньше даты начала не проходит', () => {
    const errors = validateRegularOperationForm(form({
      dateFrom: localDate('2026-08-23'),
      dateTo: localDate('2026-08-22'),
    }))

    expect(errors.dateTo).toBe(DATE_TO_BEFORE_FROM)
  })

  it('дата окончания в день начала проходит', () => {
    const errors = validateRegularOperationForm(form({
      dateFrom: localDate('2026-08-23'),
      dateTo: localDate('2026-08-23'),
    }))

    expect(errors.dateTo).toBeUndefined()
  })

  it.each([
    ['name', NAME_MAX_LENGTH],
    ['place', PLACE_MAX_LENGTH],
    ['comment', COMMENT_MAX_LENGTH],
  ] as const)('поле %s длиннее %i символов не проходит', (field, maxLength) => {
    const errors = validateRegularOperationForm(form({ [field]: 'я'.repeat(maxLength + 1) }))

    expect(errors[field]).toBe(tooLongMessage(maxLength))
  })
})

describe('категории в диалоге', () => {
  it('выбранный тип оставляет только свои категории в порядке дерева', () => {
    expect(categoryOptions(all, 1, null).map(item => item.id)).toEqual([food.id, coffee.id])
    expect(categoryOptions(all, 2, null).map(item => item.id)).toEqual([salary.id])
  })

  it('при изменении тип берётся из категории операции', () => {
    expect(categoryOptions(all, null, coffee.id).map(item => item.id)).toEqual([food.id, coffee.id])
  })

  it('без типа и без категории отдаются все категории', () => {
    expect(categoryOptions(all, null, null).map(item => item.id)).toEqual([food.id, coffee.id, salary.id])
    expect(categoryOptions(all, null, 99).map(item => item.id)).toEqual([food.id, coffee.id, salary.id])
  })

  it('ищет подстроку без учёта регистра', () => {
    expect(searchCategories(all, 'кОф').map(item => item.id)).toEqual([coffee.id])
    expect(searchCategories(all, 'РАБ')).toEqual([])
  })

  it('пустой запрос отдаёт весь список', () => {
    expect(searchCategories(all, '   ').map(item => item.id)).toEqual(all.map(item => item.id))
  })
})

describe('кэш мест', () => {
  it.each([
    ['Дом', '', false],
    ['Дом', '   ', false],
    ['Дом', 'Дом', false],
    ['Дом', 'Работа', true],
    [null, 'Работа', true],
  ])('прежнее место %j, новое %j сбрасывает кэш: %s', (previous, next, expected) => {
    expect(shouldResetPlaces(previous, next)).toBe(expected)
  })
})

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function category(id: number, name: string, operationTypeId: OperationTypeId, parentId: number | null = null): Category {
  return { id, parentId, name, color: null, order: null, operationTypeId, isDeleted: false }
}

function view(extra: Partial<RegularOperationView> = {}): RegularOperationView {
  const operation: RegularOperation = {
    id: 1,
    name: 'Аренда',
    categoryId: food.id,
    sum: Money.parseInput('100')!,
    comment: null,
    place: null,
    timeTypeId: 3,
    timeValue: 1,
    dateFrom: localDate('2026-08-23'),
    dateTo: null,
    runTime: localDate('2026-09-01'),
    isDeleted: false,
  }

  return { ...operation, category: food, ...extra }
}

function form(extra: Partial<RegularOperationFormValue> = {}): RegularOperationFormValue {
  return {
    ...newRegularOperationForm(TODAY),
    name: 'Аренда',
    categoryId: food.id,
    sum: Money.parseInput('100')!,
    ...extra,
  }
}
