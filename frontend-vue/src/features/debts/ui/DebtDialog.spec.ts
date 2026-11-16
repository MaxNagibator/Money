import { DOMWrapper, enableAutoUnmount, mount } from '@vue/test-utils'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { nextTick, ref } from 'vue'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { systemClock } from '@/platform/clock'
import { editDebtForm, newDebtForm } from '../model/debt-form'
import { debtAccounts } from '../model/debt-accounts'
import DebtDialog from './DebtDialog.vue'

const isNarrow = ref(true)
enableAutoUnmount(afterEach)
afterEach(() => vi.restoreAllMocks())

vi.mock('@/shared/ui/useNarrowViewport', () => ({
  NARROW_VIEWPORT: '(max-width: 599.98px)',
  useNarrowViewport: () => isNarrow,
}))

vi.mock('@/shared/ui/useOwnerSuggestions', () => ({
  useOwnerSuggestions: () => ({ find: () => Promise.resolve([]), reset: () => {} }),
}))

const REMOVE = '[data-testid="debts-dialog-remove"]'
const SCRIM = '.mud-overlay-dialog'

function debt(): Debt {
  return {
    id: 7,
    typeId: 1,
    sum: Money.fromMinorUnits(100_000),
    paySum: Money.zero,
    comment: null,
    ownerName: 'Пётр',
    date: parseLocalDate('2026-09-17')!,
    payComment: null,
    isDeleted: false,
  }
}

function openDialog(options: { removable?: boolean, existing?: boolean, isSaving?: boolean } = {}) {
  return mount(DebtDialog, {
    attachTo: document.body,
    props: {
      isSaving: options.isSaving === true,
      removable: options.removable === true,
      value: options.existing === false
        ? newDebtForm(parseLocalDate('2026-09-17')!)
        : editDebtForm(debt()),
    },
  })
}

describe('лист долга на узком экране', () => {
  it.each([
    ['тап по затемнению не закрывает лист', { isSaving: false }, true],
    ['тап по затемнению во время сохранения не закрывает', { isSaving: true }, true],
    ['клик по затемнению на широком экране ничего не делает', { isSaving: false }, false],
  ])('%s', async (_case, options, narrow) => {
    isNarrow.value = narrow

    const wrapper = openDialog(options)

    const scrims = document.querySelectorAll<HTMLElement>(SCRIM)

    expect(scrims).toHaveLength(1)

    scrims[0]?.click()
    await nextTick()

    expect(wrapper.emitted('cancel') ?? []).toHaveLength(0)
    expect(document.querySelector('[data-testid="debts-dialog"]')).not.toBeNull()

    wrapper.unmount()
  })
})

describe('удаление долга из диалога правки', () => {
  it('кнопка удаления есть у сохранённого долга на узком экране', async () => {
    isNarrow.value = true

    const wrapper = openDialog({ removable: true })
    const button = document.querySelector<HTMLElement>(REMOVE)

    button?.click()
    await nextTick()

    expect(wrapper.emitted('remove')).toHaveLength(1)

    wrapper.unmount()
  })

  it.each([
    ['широком экране', { removable: true }, false],
    ['создании долга', { removable: true, existing: false }, true],
    ['диалоге без разрешения на удаление', {}, true],
  ])('кнопки удаления нет при %s', (_case, options, narrow) => {
    isNarrow.value = narrow

    const wrapper = openDialog(options)

    expect(document.querySelector(REMOVE)).toBeNull()

    wrapper.unmount()
  })
})

describe('форма записи в двух режимах', () => {
  it('открывает понятную форму, форматирует сумму и считает замену записи вместо повторного начисления', async () => {
    const original = { ...debt(), sum: Money.fromWire('8000.0') }
    const wrapper = mount(DebtDialog, {
      attachTo: document.body,
      props: { value: editDebtForm(original), isSaving: false, movement: true, mode: 'normal', accounts: debtAccounts([original]) },
    })
    const body = new DOMWrapper(document.body)
    await nextTick()
    expect(body.text()).toContain('Редактировать запись')
    expect(body.get('[data-testid="debts-dialog-owner"]').text()).toContain('Человек')
    expect(body.get<HTMLInputElement>('[data-testid="sum-numeric"] input').element.value).toBe('8 000')
    expect(document.activeElement).toBe(body.get('[data-testid="sum-numeric"] input').element)
    const direction = body.get('[data-testid="debts-dialog-directions"]')
    expect(direction.text()).toContain('Дал ещё')
    expect(direction.element.compareDocumentPosition(body.get('[data-testid="debts-dialog-sum"]').element) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy()
    await body.get('[data-testid="sum-numeric"] input').trigger('keydown', { key: '+' })
    await body.get('[data-testid="sum-expression"] input').setValue('350+150')
    expect(body.get('[data-testid="debts-dialog-preview"]').text()).toContain('мне должны 8 000 ₽ → мне должны 500 ₽')
    await body.get('[data-testid="debts-dialog-save"]').trigger('click')
    const saved = wrapper.emitted('submit')?.[0]?.[0] as ReturnType<typeof editDebtForm> | undefined
    expect(saved?.sum?.toDecimal().toString()).toBe('500')
    expect(saved?.typeId).toBe(1)
    wrapper.unmount()
  })

  it('показывает переход через ноль, скрывает неверное превью и сохраняет короткую дату', async () => {
    const source = debt()
    vi.spyOn(systemClock, 'today').mockReturnValue(source.date)
    const wrapper = mount(DebtDialog, {
      attachTo: document.body,
      props: { value: { ...newDebtForm(source.date), ownerName: source.ownerName, typeId: 2, sum: Money.fromWire('1500') }, isSaving: false, movement: true, mode: 'normal', accounts: debtAccounts([source]) },
    })
    const body = new DOMWrapper(document.body)
    expect(body.get('[data-testid="debts-dialog-preview"]').text()).toContain('мне должны 1 000 ₽ → я должен 500 ₽')
    const amount = body.get('[data-testid="sum-numeric"] input')
    await amount.setValue('не сумма')
    expect(body.find('[data-testid="debts-dialog-preview"]').exists()).toBe(false)
    await amount.setValue('500')
    await body.get('[data-testid="debts-dialog-date"] input').setValue('пв')
    await body.get('[data-testid="debts-dialog-save"]').trigger('click')
    const saved = wrapper.emitted('submit')?.[0]?.[0] as ReturnType<typeof editDebtForm> | undefined
    expect(saved?.typeId).toBe(2)
    expect(saved?.sum?.toDecimal().toString()).toBe('500')
    expect(saved?.date).toBe('2026-09-15')
    wrapper.unmount()
  })

  it('учитывает старую оплату при переносе записи между контактами и сохраняет строгие подписи', async () => {
    const original = { ...debt(), sum: Money.fromWire('1000'), paySum: Money.fromWire('400') }
    const other = { ...debt(), id: 8, ownerName: 'Анна', typeId: 2, sum: Money.fromWire('2000') }
    const wrapper = mount(DebtDialog, {
      attachTo: document.body,
      props: { value: editDebtForm(original), isSaving: false, movement: true, mode: 'accounting', accounts: debtAccounts([original, other]) },
    })
    const body = new DOMWrapper(document.body)
    expect(body.get('[data-testid="debts-dialog-directions"]').text()).toContain('Уход (Выдано)')
    expect(body.get('[data-testid="debts-dialog-owner"]').text()).toContain('Контрагент')
    await body.get('[data-testid="debts-dialog-owner"] input').setValue('Анна')
    await body.get('[data-testid="sum-numeric"] input').setValue('800')
    expect(body.get('[data-testid="debts-dialog-preview"]').text()).toContain('нужно отдать 2 000,00 ₽ → нужно отдать 1 600,00 ₽')
    wrapper.unmount()
  })
})
