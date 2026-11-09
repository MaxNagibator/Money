import { describe, expect, it } from 'vitest'
import type { OperationTypeId } from '@/domain/operation-type'
import { operationTypeById, OPERATION_TYPES } from '@/domain/operation-type'
import { mdArrowCircleDown, mdArrowCircleUp } from '@/shared/icons'

describe('вид типа операции', () => {
  it.each<[OperationTypeId, string, string]>([
    [1, 'warning', mdArrowCircleDown],
    [2, 'success', mdArrowCircleUp],
  ])('тип %i несёт цвет и иконку', (id, color, icon) => {
    const type = operationTypeById(id)

    expect(type.color).toBe(color)
    expect(type.icon).toBe(icon)
  })

  it.each<[OperationTypeId, string, string, string]>([
    [1, 'Расход', 'Новый расход', 'Правка расхода'],
    [2, 'Доход', 'Новый доход', 'Правка дохода'],
  ])('тип %i несёт короткую подпись и заголовки формы', (id, shortAddLabel, newTitle, editTitle) => {
    const type = operationTypeById(id)

    expect(type.shortAddLabel).toBe(shortAddLabel)
    expect(type.newTitle).toBe(newTitle)
    expect(type.editTitle).toBe(editTitle)
  })

  it('каждый тип из справочника находится по идентификатору', () => {
    expect(OPERATION_TYPES.map(type => operationTypeById(type.id))).toEqual([...OPERATION_TYPES])
  })
})
