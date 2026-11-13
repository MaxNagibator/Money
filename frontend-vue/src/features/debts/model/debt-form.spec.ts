import { describe, expect, it } from 'vitest'
import type { DebtMovement } from '@/domain/debt'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { DebtFormValue } from './debt-form'
import { DATE_UNPARSED, dateFieldError, editDebtForm, FIELD_REQUIRED, newDebtForm, SUM_UNPARSED, toDebtDraft, validateDebtForm } from './debt-form'
import { newPaymentForm, PAYMENT_SUM_INVALID, toPaymentDraft, validatePaymentForm } from './payment-form'

function date(value: string): LocalDate {
  return parseLocalDate(value)!
}

function money(value: string): Money {
  return Money.parseInput(value)!
}

function DebtMovement(extra: Partial<DebtMovement> = {}): DebtMovement {
  return {
    id: 7,
    typeId: 2,
    sum: money('1000'),
    comment: 'за обед',
    ownerName: 'Иван',
    date: date('2026-08-20'),
    isDeleted: false,
    ...extra,
  }
}

function form(extra: Partial<DebtFormValue> = {}): DebtFormValue {
  return { ...newDebtForm(date('2026-08-22'), 1), ownerName: 'Иван', ...extra }
}

describe('форма долга', () => {
  it('новый долг открывается с сегодняшней датой, типом кнопки, пустым держателем и нулевой суммой', () => {
    const value = newDebtForm(date('2026-08-22'), 2)

    expect(value.id).toBeNull()
    expect(value.typeId).toBe(2)
    expect(value.date).toBe('2026-08-22')
    expect(value.ownerName).toBe('')
    expect(value.comment).toBe('')
    expect(value.sum?.isZero).toBe(true)
  })

  it('нейтральная кнопка оставляет тип пустым', () => {
    expect(newDebtForm(date('2026-08-22')).typeId).toBeNull()
  })

  it('редактирование предзаполнено значениями долга', () => {
    const value = editDebtForm(DebtMovement())

    expect(value.id).toBe(7)
    expect(value.typeId).toBe(2)
    expect(value.ownerName).toBe('Иван')
    expect(value.comment).toBe('за обед')
    expect(value.date).toBe('2026-08-20')
    expect(value.sum?.toMinorUnits()).toBe(100_000)
  })

  it.each([
    [{ typeId: null }, 'typeId'],
    [{ ownerName: '' }, 'ownerName'],
    [{ ownerName: '   ' }, 'ownerName'],
    [{ date: null }, 'date'],
  ])('поле %j обязательно и получает сообщение «Заполни меня»', (extra, field) => {
    const errors = validateDebtForm(form(extra))

    expect(errors[field as keyof typeof errors]).toBe(FIELD_REQUIRED)
    expect(Object.keys(errors)).toEqual([field])
  })

  it('комментарий и нулевая сумма проходят проверку', () => {
    expect(validateDebtForm(form({ comment: '', sum: Money.zero }))).toEqual({})
  })

  it('нераспознанная сумма отменяет отправку', () => {
    expect(() => toDebtDraft(form({ sum: null }))).toThrow(SUM_UNPARSED)
  })

  it('черновик обрезает имя держателя и превращает пустой комментарий в null', () => {
    const draft = toDebtDraft(form({ ownerName: '  Иван  ', comment: '   ', sum: money('12,34') }))

    expect(draft.typeId).toBe(1)
    expect(draft.ownerName).toBe('Иван')
    expect(draft.comment).toBeNull()
    expect(draft.date).toBe('2026-08-22')
    expect(draft.sum.toMinorUnits()).toBe(1234)
  })
})

describe('дата в формах долга', () => {
  it.each([
    ['нераспознанный ввод поверх прежней даты', date('2026-08-20'), null, DATE_UNPARSED],
    ['очищенное поле', null, null, FIELD_REQUIRED],
    ['разобранный ввод', date('2026-08-20'), date('2026-08-22'), ''],
  ])('%s задаёт сообщение поля даты', (_, shown, parsed, expected) => {
    expect(dateFieldError(shown, parsed)).toBe(expected)
  })

  it.each([
    ['долга', (): unknown => toDebtDraft(form({ date: null }))],
    ['платежа', (): unknown => toPaymentDraft({ debtId: 7, sum: money('100'), date: null, comment: '' })],
  ])('черновик %s не собирается без разобранной даты', (_, build) => {
    expect(build).toThrow(FIELD_REQUIRED)
  })
})

describe('форма оплаты', () => {
  it('предзаполняется суммой движения и сегодняшней датой', () => {
    const value = newPaymentForm(DebtMovement({ sum: money('1000') }), date('2026-08-22'))

    expect(value.debtId).toBe(7)
    expect(value.sum?.toMinorUnits()).toBe(100_000)
    expect(value.date).toBe('2026-08-22')
    expect(value.comment).toBe('')
  })

  it.each([
    [Money.zero, PAYMENT_SUM_INVALID],
    [money('0,99'), PAYMENT_SUM_INVALID],
    [null, PAYMENT_SUM_INVALID],
  ])('платёж %s меньше рубля отвергается', (sum, expected) => {
    expect(validatePaymentForm({ debtId: 7, sum, date: date('2026-08-22'), comment: '' }).sum).toBe(expected)
  })

  it('платёж ровно в рубль проходит, дата обязательна', () => {
    expect(validatePaymentForm({ debtId: 7, sum: money('1'), date: date('2026-08-22'), comment: '' })).toEqual({})
    expect(validatePaymentForm({ debtId: 7, sum: money('1'), date: null, comment: '' }).date).toBe(FIELD_REQUIRED)
  })

  it('черновик платежа отдаёт пустой комментарий как null', () => {
    const draft = toPaymentDraft({ debtId: 7, sum: money('100'), date: date('2026-08-22'), comment: '  ' })

    expect(draft.comment).toBeNull()
    expect(draft.date).toBe('2026-08-22')
    expect(draft.sum.toMinorUnits()).toBe(10_000)
  })
})
