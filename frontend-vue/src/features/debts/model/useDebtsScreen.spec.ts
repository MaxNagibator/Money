import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { computed, defineComponent, shallowRef } from 'vue'
import type { DebtDraft } from '@/api/adapters/debts'
import {
  createDebt,
  deleteDebt,
  fetchDebt,
  fetchDebtOwners,
  fetchDebts,
  forgiveDebts,
  mergeDebtOwners,
  payDebt,
  restoreDebt,
  updateDebt,
} from '@/api/adapters/debts'
import { ApiError } from '@/api/adapters/problem'
import type { Category } from '@/domain/category'
import type { DebtMovement } from '@/domain/debt'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Clock } from '@/platform/clock'
import { newDebtForm } from './debt-form'
import type { DebtsScreenState } from './useDebtsScreen'
import { DEBT_CATEGORY_NOT_EXPENSE, DEBT_CATEGORY_REQUIRED, DEBT_NOT_FORGIVABLE, DEBT_SELECTION_REQUIRED, useDebtsScreen } from './useDebtsScreen'

vi.mock('@/api/adapters/debts', async importOriginal => {
  const actual = await importOriginal<typeof import('@/api/adapters/debts')>()

  return {
    ...actual,
    fetchDebts: vi.fn(),
    fetchDebt: vi.fn(),
    fetchDebtOwners: vi.fn(),
    createDebt: vi.fn(),
    updateDebt: vi.fn(),
    deleteDebt: vi.fn(),
    restoreDebt: vi.fn(),
    payDebt: vi.fn(),
    forgiveDebts: vi.fn(),
    mergeDebtOwners: vi.fn(),
  }
})

const fetchDebtsMock = vi.mocked(fetchDebts)
const fetchDebtMock = vi.mocked(fetchDebt)
const fetchDebtOwnersMock = vi.mocked(fetchDebtOwners)
const createDebtMock = vi.mocked(createDebt)
const updateDebtMock = vi.mocked(updateDebt)
const deleteDebtMock = vi.mocked(deleteDebt)
const restoreDebtMock = vi.mocked(restoreDebt)
const payDebtMock = vi.mocked(payDebt)
const forgiveDebtsMock = vi.mocked(forgiveDebts)
const mergeDebtOwnersMock = vi.mocked(mergeDebtOwners)

const CATEGORIES: readonly Category[] = [
  { id: 3, parentId: null, name: 'Долги', color: null, order: null, operationTypeId: 1, isDeleted: false },
  { id: 4, parentId: null, name: 'Зарплата', color: null, order: null, operationTypeId: 2, isDeleted: false },
]

const clock: Clock = {
  now: () => new Date('2026-08-22T00:00:00'),
  today: () => date('2026-08-22'),
}

let rows: DebtMovement[] = []
let nextId = 100

beforeEach(() => {
  vi.clearAllMocks()

  rows = [
    row(1, { typeId: 1, ownerName: 'Иван', date: date('2026-08-20'), comment: 'за обед' }),
    row(2, { typeId: 1, ownerName: 'Пётр', date: date('2026-08-21') }),
    row(3, { typeId: 2, ownerName: 'Мария', date: date('2026-08-22') }),
  ]

  nextId = 100

  fetchDebtsMock.mockImplementation(() => Promise.resolve(rows.map(entry => ({ ...entry }))))

  createDebtMock.mockImplementation((draft: DebtDraft) => {
    const id = nextId++

    rows.push(row(id, { typeId: draft.typeId, ownerName: draft.ownerName, date: draft.date, sum: draft.sum }))

    return Promise.resolve(id)
  })

  updateDebtMock.mockImplementation((id, draft: DebtDraft) => {
    rows = rows.map(entry => entry.id === id
      ? { ...entry, typeId: draft.typeId, ownerName: draft.ownerName, date: draft.date, sum: draft.sum }
      : entry)

    return Promise.resolve()
  })

  deleteDebtMock.mockImplementation(id => {
    rows = rows.filter(entry => entry.id !== id)

    return Promise.resolve()
  })

  fetchDebtOwnersMock.mockResolvedValue([{ id: 5, name: 'Иван' }, { id: 6, name: 'Пётр' }])
  restoreDebtMock.mockResolvedValue(undefined)
  payDebtMock.mockImplementation((id, draft) => {
    const source = rows.find(entry => entry.id === id)

    if (source !== undefined) {
      rows.push(row(nextId++, {
        typeId: source.typeId === 1 ? 2 : 1,
        ownerName: source.ownerName,
        date: draft.date,
        sum: draft.sum,
        comment: draft.comment,
      }))
    }

    return Promise.resolve()
  })
  forgiveDebtsMock.mockResolvedValue(undefined)
  mergeDebtOwnersMock.mockResolvedValue(undefined)
})

describe('секция держателей', () => {
  it('раскрыта по умолчанию и сворачивается кнопкой', async () => {
    const state = await setup()

    expect(state.groups.value.map(group => group.typeId)).toEqual([0])
    expect(state.isTypeExpanded(0)).toBe(true)

    state.toggleType(0)

    expect(state.isTypeExpanded(0)).toBe(false)
    state.toggleType(0)

    expect(state.isTypeExpanded(0)).toBe(true)
  })

  it('сохранение долга не разворачивает свёрнутую секцию', async () => {
    const state = await setup()

    state.toggleType(0)
    await state.submit({ ...newDebtForm(date('2026-08-22'), 1), ownerName: 'Иван', sum: money('500') })
    await vi.waitFor(() => {
      expect(fetchDebtsMock).toHaveBeenCalledTimes(2)
    })

    expect(state.isTypeExpanded(0)).toBe(false)
  })
})

describe('сохранение долга', () => {
  it('созданный долг встаёт по своей дате, а не первым в списке держателя', async () => {
    const state = await setup()

    await state.submit({ ...newDebtForm(date('2026-08-19'), 1), ownerName: 'Иван', sum: money('500') })
    await vi.waitFor(() => {
      expect(debtIds(state, 1, 'Иван')).toEqual([1, 100])
    })

    expect(state.form.value).toBeNull()
    expect(state.notice.value?.text).toBe('Успех!')
  })

  it('смена типа оставляет движение у того же держателя', async () => {
    const state = await setup()

    rows = rows.filter(entry => entry.id !== 2)

    await state.submit({
      id: 1,
      typeId: 2,
      ownerName: 'Иван',
      sum: money('1000'),
      date: date('2026-08-20'),
      comment: '',
    })

    await vi.waitFor(() => {
      expect(state.groups.value.map(group => group.typeId)).toEqual([0])
    })

    expect(state.groups.value[0]?.owners.map(owner => owner.name)).toEqual(['Иван', 'Мария'])
  })

  it('ошибка сохранения оставляет форму открытой и говорит текстом сервера', async () => {
    const state = await setup()

    createDebtMock.mockRejectedValue(new ApiError('Извините, но долг не найден'))

    const value = { ...newDebtForm(date('2026-08-22'), 1), ownerName: 'Иван', sum: money('500') }

    state.form.value = value
    await state.submit(value)

    expect(state.form.value).not.toBeNull()
    expect(state.notice.value).toEqual({ text: 'Извините, но долг не найден', type: 'error' })
    expect(state.isSaving.value).toBe(false)
  })
})

describe('прощение долгов', () => {
  it('пока панель закрыта, выбрать долг нельзя', async () => {
    const state = await setup()

    expect(state.isSelectable(debtOf(state, 1))).toBe(false)

    state.toggleSelection(debtOf(state, 1))

    expect(state.selectedCount.value).toBe(0)
    expect(state.notice.value?.text).toBe(DEBT_NOT_FORGIVABLE)
  })

  it('при открытой панели выбираются только долги «Нужно забрать»', async () => {
    const state = await setup()

    state.openForgive(true)
    state.toggleSelection(debtOf(state, 1))
    state.toggleSelection(debtOf(state, 3))

    expect(state.selectedIds.value.has(1)).toBe(true)
    expect(state.selectedIds.value.has(3)).toBe(false)
    expect(state.notice.value?.text).toBe(DEBT_NOT_FORGIVABLE)
  })

  it('«Выбрать все» берёт только видимые после поиска прощаемые долги', async () => {
    const state = await setup()

    state.openForgive(true)
    state.search.value = 'обед'
    state.selectAllVisible()

    expect([...state.selectedIds.value]).toEqual([1])
  })

  it('закрытие панели очищает выбор', async () => {
    const state = await setup()

    state.openForgive(true)
    state.toggleSelection(debtOf(state, 1))
    state.openForgive(false)

    expect(state.selectedCount.value).toBe(0)
  })

  it.each([
    [null, DEBT_CATEGORY_REQUIRED],
    [4, DEBT_CATEGORY_NOT_EXPENSE],
  ])('категория %s отклоняется предупреждением', async (categoryId, expected) => {
    const state = await setup()

    state.openForgive(true)
    state.toggleSelection(debtOf(state, 1))
    state.forgiveCategoryId.value = categoryId

    await state.forgive()

    expect(state.notice.value?.text).toBe(expected)
    expect(forgiveDebtsMock).not.toHaveBeenCalled()
  })

  it('прощение без выбора отклоняется предупреждением', async () => {
    const state = await setup()

    state.openForgive(true)
    state.forgiveCategoryId.value = 3

    await state.forgive()

    expect(state.notice.value?.text).toBe(DEBT_SELECTION_REQUIRED)
    expect(forgiveDebtsMock).not.toHaveBeenCalled()
  })

  it('прощаются только те выбранные долги, что ещё можно простить, счётчик считает их', async () => {
    const state = await setup()

    state.openForgive(true)
    state.toggleSelection(debtOf(state, 1))
    state.toggleSelection(debtOf(state, 2))
    state.forgiveCategoryId.value = 3

    await state.remove(debtOf(state, 2))
    await state.forgive()

    expect(forgiveDebtsMock).toHaveBeenCalledWith({ debtIds: [1], categoryId: 3, comment: 'Перенос из долгов:' })
    expect(state.notice.value).toEqual({ text: 'Успешно прощено долгов: 1', type: 'success' })
  })

  it('выбор, потерявший право на прощение целиком, отклоняется предупреждением', async () => {
    const state = await setup()

    state.openForgive(true)
    state.toggleSelection(debtOf(state, 1))
    state.forgiveCategoryId.value = 3

    await state.remove(debtOf(state, 1))
    await state.forgive()

    expect(forgiveDebtsMock).not.toHaveBeenCalled()
    expect(state.notice.value?.text).toBe(DEBT_NOT_FORGIVABLE)
  })

  it('успешное прощение чистит выбор, закрывает панель и перезапрашивает список', async () => {
    const state = await setup()

    state.openForgive(true)
    state.toggleSelection(debtOf(state, 1))
    state.forgiveCategoryId.value = 3
    rows = rows.filter(entry => entry.id !== 1)

    await state.forgive()

    expect(forgiveDebtsMock).toHaveBeenCalledWith({ debtIds: [1], categoryId: 3, comment: 'Перенос из долгов:' })
    expect(state.selectedCount.value).toBe(0)
    expect(state.isForgiveOpen.value).toBe(false)
    expect(state.notice.value).toEqual({ text: 'Успешно прощено долгов: 1', type: 'success' })

    await vi.waitFor(() => {
      expect(fetchDebtsMock).toHaveBeenCalledTimes(2)
    })
  })

  it('отказ сервера снимает признак прощения и оставляет выбор', async () => {
    const state = await setup()

    forgiveDebtsMock.mockRejectedValue(new ApiError('Извините, но прощать можно только непогашенные долги'))

    state.openForgive(true)
    state.toggleSelection(debtOf(state, 1))
    state.forgiveCategoryId.value = 3

    await state.forgive()

    expect(state.isForgiving.value).toBe(false)
    expect(state.isForgiveOpen.value).toBe(true)
    expect(state.selectedCount.value).toBe(1)
    expect(state.notice.value?.type).toBe('error')
  })
})

describe('удаление и оплата', () => {
  it('удалённый долг остаётся карточкой и возвращается восстановлением', async () => {
    const state = await setup()

    await state.remove(debtOf(state, 1))

    expect(deleteDebtMock).toHaveBeenCalledWith(1)
    expect(debtOf(state, 1).isDeleted).toBe(true)

    rows.push(row(1, { typeId: 1, ownerName: 'Иван', date: date('2026-08-20') }))
    await state.restore(debtOf(state, 1))

    expect(restoreDebtMock).toHaveBeenCalledWith(1)
    expect(debtOf(state, 1).isDeleted).toBe(false)
  })

  it('удаление из диалога правки закрывает форму и помечает открытый долг', async () => {
    const state = await setup()

    state.startEdit(debtOf(state, 1))
    await state.removeEdited()

    expect(deleteDebtMock).toHaveBeenCalledWith(1)
    expect(state.form.value).toBeNull()
    expect(debtOf(state, 1).isDeleted).toBe(true)
  })

  it('без открытой формы удалять нечего', async () => {
    const state = await setup()

    await state.removeEdited()

    expect(deleteDebtMock).not.toHaveBeenCalled()
  })

  it('восстановление возвращает долг, которого уже нет в ответе сервера', async () => {
    const state = await setup()

    await state.remove(debtOf(state, 1))
    await state.submit({ ...newDebtForm(date('2026-08-22'), 1), ownerName: 'Пётр', sum: money('500') })
    await vi.waitFor(() => {
      expect(fetchDebtsMock).toHaveBeenCalledTimes(2)
    })

    expect(debtOf(state, 1).isDeleted).toBe(true)

    rows.push(row(1, { typeId: 1, ownerName: 'Иван', date: date('2026-08-20') }))
    await state.restore(debtOf(state, 1))

    await vi.waitFor(() => {
      expect(debtIds(state, 1, 'Иван')).toEqual([1])
    })

    expect(debtOf(state, 1).isDeleted).toBe(false)
  })

  it('оплата создаёт встречное движение и перечитывает список', async () => {
    const state = await setup()

    state.startPayment(debtOf(state, 1))

    expect(state.payment.value?.sum?.toMinorUnits()).toBe(100_000)
    expect(state.payment.value?.date).toBe('2026-08-22')

    await state.submitPayment({ debtId: 1, sum: money('400'), date: date('2026-08-22'), comment: '' })

    expect(payDebtMock).toHaveBeenCalledTimes(1)
    expect(state.payment.value).toBeNull()
    await vi.waitFor(() => {
      expect(debtOf(state, 100).sum.toMinorUnits()).toBe(40_000)
    })
    expect(debtOf(state, 100).typeId).toBe(2)
    expect(fetchDebtsMock).toHaveBeenCalledTimes(2)
  })

  it('отказ перечитывания не отменяет платёж и не оставляет форму на повтор', async () => {
    const state = await setup()

    fetchDebtMock.mockRejectedValue(new ApiError('Не удалось загрузить долг'))

    state.startPayment(debtOf(state, 1))
    await state.submitPayment({ debtId: 1, sum: money('400'), date: date('2026-08-22'), comment: '' })

    expect(payDebtMock).toHaveBeenCalledTimes(1)
    expect(state.payment.value).toBeNull()
    expect(state.notice.value).toBeNull()
    expect(state.isPaying.value).toBe(false)
  })

  it('поздний ответ списка после оплаты приводит к повторной загрузке движений', async () => {
    const state = await setup()

    const stale = rows.map(entry => ({ ...entry }))
    let releaseList: (value: DebtMovement[]) => void = () => undefined

    fetchDebtsMock.mockImplementationOnce(() => new Promise<DebtMovement[]>(resolve => {
      releaseList = resolve
    }))

    void state.submit({ ...newDebtForm(date('2026-08-22'), 1), ownerName: 'Иван', sum: money('500') })
    await vi.waitFor(() => {
      expect(fetchDebtsMock).toHaveBeenCalledTimes(2)
    })

    await state.submitPayment({ debtId: 1, sum: money('400'), date: date('2026-08-22'), comment: '' })

    releaseList(stale)
    await settle()

    await vi.waitFor(() => {
      expect(debtOf(state, 101).sum.toMinorUnits()).toBe(40_000)
    })
  })

  it('пока платёж выполняется, форма другого долга не открывается', async () => {
    const state = await setup()

    let releasePayment: () => void = () => undefined

    payDebtMock.mockImplementationOnce(() => new Promise<void>(resolve => {
      releasePayment = () => {
        resolve()
      }
    }))
    fetchDebtMock.mockResolvedValue(paidDebt())

    state.startPayment(debtOf(state, 1))

    const pending = state.submitPayment({ debtId: 1, sum: money('400'), date: date('2026-08-22'), comment: '' })

    await vi.waitFor(() => {
      expect(payDebtMock).toHaveBeenCalledTimes(1)
    })

    state.startPayment(debtOf(state, 2))

    expect(state.payment.value?.debtId).toBe(1)

    releasePayment()
    await pending

    expect(state.payment.value).toBeNull()
  })

  it('чужая карточка не гасит открытую форму платежа', async () => {
    const state = await setup()

    state.startPayment(debtOf(state, 1))
    state.cancelPayment(debtOf(state, 2))

    expect(state.payment.value?.debtId).toBe(1)

    state.cancelPayment(debtOf(state, 1))

    expect(state.payment.value).toBeNull()
  })

  it('движения с нулевым итогом показываются при включённом тумблере истории', async () => {
    const state = await setup()

    rows = [...rows, row(4, { typeId: 2, ownerName: 'Иван', date: date('2026-08-18'), sum: money('1000') })]
    state.showPaid.value = true

    await vi.waitFor(() => {
      expect(fetchDebtsMock).toHaveBeenLastCalledWith(true)
      expect(debtIds(state, 1, 'Иван')).toEqual([1, 4])
    })
  })
})

describe('слияние держателей', () => {
  it('каждое открытие панели обновляет держателей, слияние обновляет и список долгов', async () => {
    const state = await setup()

    state.openMerge(true)

    await vi.waitFor(() => {
      expect(state.owners.owners.value).toHaveLength(2)
    })

    state.openMerge(false)
    state.openMerge(true)

    await vi.waitFor(() => {
      expect(fetchDebtOwnersMock).toHaveBeenCalledTimes(2)
    })

    state.ownerFromId.value = 5
    state.ownerToId.value = 6

    await state.merge()

    expect(mergeDebtOwnersMock).toHaveBeenCalledWith(5, 6)
    await vi.waitFor(() => {
      expect(fetchDebtOwnersMock).toHaveBeenCalledTimes(3)
    })
    expect(state.isMergeOpen.value).toBe(false)
    expect(state.ownerFromId.value).toBeNull()

    await vi.waitFor(() => {
      expect(fetchDebtsMock).toHaveBeenCalledTimes(2)
    })
  })

  it('ошибка загрузки держателей видна и снимается повторным открытием панели', async () => {
    const state = await setup()

    fetchDebtOwnersMock.mockRejectedValueOnce(new ApiError('Не удалось загрузить держателей'))

    state.openMerge(true)

    await vi.waitFor(() => {
      expect(state.owners.loadError.value).toBe('Не удалось загрузить держателей')
    })

    state.openMerge(false)
    state.openMerge(true)

    await vi.waitFor(() => {
      expect(state.owners.owners.value).toHaveLength(2)
    })

    expect(state.owners.loadError.value).toBeNull()
  })

  it('одинаковые держатели не сливаются', async () => {
    const state = await setup()

    state.ownerFromId.value = 5
    state.ownerToId.value = 5

    await state.merge()

    expect(mergeDebtOwnersMock).not.toHaveBeenCalled()
  })
})

async function setup(): Promise<DebtsScreenState> {
  const categories = shallowRef<Category[]>([...CATEGORIES])
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

  let state!: DebtsScreenState

  const wrapper = mount(
    defineComponent({
      setup() {
        state = useDebtsScreen(computed(() => categories.value), clock)

        return () => null
      },
    }),
    { global: { plugins: [[VueQueryPlugin, { queryClient }]] } },
  )

  onTestFinished(() => {
    wrapper.unmount()
  })

  await vi.waitFor(() => {
    expect(state.debts.isLoading.value).toBe(false)
  })

  return state
}

function debtOf(state: DebtsScreenState, id: number): DebtMovement {
  const found = state.debts.debts.value.find(entry => entry.id === id)

  if (found === undefined) {
    throw new Error(`Долг ${id} не найден`)
  }

  return found
}

function debtIds(state: DebtsScreenState, typeId: number, ownerName: string): number[] {
  void typeId
  const group = state.groups.value[0]
  const owner = group?.owners.find(entry => entry.name === ownerName)

  return owner?.debts.map(entry => entry.id) ?? []
}

function paidDebt(): DebtMovement {
  return row(1, {
    typeId: 1,
    ownerName: 'Иван',
    date: date('2026-08-20'),
    comment: 'взнос',
  })
}

async function settle(): Promise<void> {
  await new Promise(resolve => {
    setTimeout(resolve, 0)
  })
}

function row(id: number, extra: Partial<DebtMovement> = {}): DebtMovement {
  return {
    id,
    typeId: 1,
    sum: money('1000'),
    comment: null,
    ownerName: 'Иван',
    date: date('2026-08-22'),
    isDeleted: false,
    ...extra,
  }
}

function money(value: string): Money {
  return Money.parseInput(value)!
}

function date(value: string): LocalDate {
  return value as LocalDate
}
