import { describe, expect, it } from 'vitest'
import type { FastOperation } from '@/api/adapters/fast-operations'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import type { OperationFormValue } from './operation-form'
import {
  CATEGORY_REQUIRED,
  categoriesForType,
  DATE_REQUIRED,
  editOperationForm,
  fastOperationForm,
  newOperationForm,
  shouldResetPlaces,
  SUM_UNPARSED,
  toOperationDraft,
  validateOperationForm,
} from './operation-form'

const food: Category = {
  id: 1,
  parentId: null,
  name: 'Еда',
  color: null,
  order: null,
  operationTypeId: 1,
  isDeleted: false,
}

const cafe: Category = { ...food, id: 2, parentId: food.id, name: 'Кафе' }
const detached: Category = { ...food, id: 3, parentId: 99, name: 'Такси' }
const salary: Category = { ...food, id: 4, parentId: null, name: 'Зарплата', operationTypeId: 2 }

const TODAY = localDate('2026-08-23')

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function operation(extra: Partial<Operation> = {}): Operation {
  return {
    id: 17,
    categoryId: cafe.id,
    category: cafe,
    sum: Money.fromMinorUnits(123_45),
    comment: 'обед',
    place: 'Пекарня',
    date: localDate('2026-08-20'),
    createdTaskId: null,
    isDeleted: false,
    ...extra,
  }
}

const brokenForms: [string, Partial<OperationFormValue>, string][] = [
  ['без категории', { categoryId: null }, CATEGORY_REQUIRED],
  ['без даты', { date: null }, DATE_REQUIRED],
  ['с нераспознанной суммой', { sum: null }, SUM_UNPARSED],
]

function filledForm(extra: Partial<OperationFormValue> = {}): OperationFormValue {
  return {
    ...newOperationForm(1, TODAY),
    categoryId: cafe.id,
    sum: Money.fromMinorUnits(500),
    ...extra,
  }
}

describe('новая операция', () => {
  it('берёт дату дня карточки и оставляет категорию и сумму пустыми', () => {
    const form = newOperationForm(2, TODAY)

    expect(form).toEqual({
      id: null,
      operationTypeId: 2,
      categoryId: null,
      sum: null,
      date: TODAY,
      comment: '',
      place: '',
    })
  })
})

describe('операция в редактировании', () => {
  it('берёт тип из категории операции и переносит её поля', () => {
    const form = editOperationForm(operation({ category: salary, categoryId: salary.id }))

    expect(form).toMatchObject({
      id: 17,
      operationTypeId: 2,
      categoryId: salary.id,
      date: localDate('2026-08-20'),
      comment: 'обед',
      place: 'Пекарня',
    })

    expect(form.sum?.toMinorUnits()).toBe(123_45)
  })

  it('подставляет расходы, когда категория операции не загружена', () => {
    const form = editOperationForm(operation({ category: null }))

    expect(form.operationTypeId).toBe(1)
  })

  it('сохраняет категорию, которой нет в справочнике', () => {
    const form = editOperationForm(operation({ category: null, categoryId: 99 }))

    expect(form.categoryId).toBe(99)
    expect(validateOperationForm(form)).toEqual({})
    expect(toOperationDraft(form)).toMatchObject({ categoryId: 99 })
  })

  it('показывает нулевую сумму пустым полем', () => {
    const form = editOperationForm(operation({ sum: Money.zero }))

    expect(form.sum).toBeNull()
  })

  it('заменяет отсутствующие комментарий и место пустой строкой', () => {
    const form = editOperationForm(operation({ comment: null, place: null }))

    expect(form).toMatchObject({ comment: '', place: '' })
  })
})

describe('операция из быстрой', () => {
  const quick: FastOperation = {
    id: 5,
    name: 'Обед',
    order: 1,
    categoryId: cafe.id,
    sum: Money.fromMinorUnits(250_00),
    comment: 'бизнес-ланч',
    place: 'Пекарня',
    isDeleted: false,
  }

  it('переносит категорию, сумму, комментарий и место, дату берёт у дня', () => {
    const form = fastOperationForm(quick, cafe, TODAY)

    expect(form).toMatchObject({
      id: null,
      operationTypeId: cafe.operationTypeId,
      categoryId: cafe.id,
      date: TODAY,
      comment: 'бизнес-ланч',
      place: 'Пекарня',
    })

    expect(form.sum?.toMinorUnits()).toBe(250_00)
  })

  it('без категории оставляет её пустой и не даёт собрать черновик', () => {
    const form = fastOperationForm(quick, null, TODAY)

    expect(form.categoryId).toBeNull()
    expect(() => toOperationDraft(form)).toThrow(CATEGORY_REQUIRED)
  })

  it('нулевую сумму показывает пустым полем', () => {
    expect(fastOperationForm({ ...quick, sum: Money.zero }, cafe, TODAY).sum).toBeNull()
  })
})

describe('список категорий диалога', () => {
  it('оставляет только категории выбранного типа и ставит потомка за родителем', () => {
    const options = categoriesForType([salary, cafe, food], 1)

    expect(options.map(category => category.id)).toEqual([food.id, cafe.id])
  })

  it('не теряет категорию с недоступным родителем', () => {
    const options = categoriesForType([food, cafe, detached, salary], 1)

    expect(options.map(category => category.id)).toEqual([food.id, cafe.id, detached.id])
  })

  it('переключение типа меняет набор категорий', () => {
    expect(categoriesForType([food, cafe, salary], 2).map(category => category.id)).toEqual([salary.id])
  })
})

describe('проверка формы операции', () => {
  it('требует категорию и дату', () => {
    const errors = validateOperationForm(filledForm({ categoryId: null, date: null }))

    expect(errors).toEqual({ categoryId: CATEGORY_REQUIRED, date: DATE_REQUIRED })
  })

  it('не жалуется на заполненную форму', () => {
    expect(validateOperationForm(filledForm())).toEqual({})
  })
})

describe('черновик операции', () => {
  it('переносит категорию, сумму и дату, пустые комментарий и место уходят как null', () => {
    const draft = toOperationDraft(filledForm({ comment: '   ', place: ' Пекарня ' }))

    expect(draft).toEqual({
      categoryId: cafe.id,
      sum: Money.fromMinorUnits(500),
      comment: null,
      place: 'Пекарня',
      date: TODAY,
    })
  })

  it.each(brokenForms)('отказывается собираться %s', (_, broken, message) => {
    expect(() => toOperationDraft(filledForm(broken))).toThrow(message)
  })

  it('создание и изменение отличаются идентификатором, в черновик он не попадает', () => {
    const created = filledForm()
    const edited = editOperationForm(operation())

    expect(created.id).toBeNull()
    expect(edited.id).toBe(17)
    expect(toOperationDraft(edited)).not.toHaveProperty('id')
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
