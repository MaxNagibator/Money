import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { DebtOwnerGroup } from '../model/debt'
import type { PaymentFormValue } from '../model/payment-form'
import DebtOwnerPanel from './DebtOwnerPanel.vue'

const isNarrow = ref(true)

vi.mock('@/shared/ui/useNarrowViewport', () => ({
  NARROW_VIEWPORT: '(max-width: 599.98px)',
  useNarrowViewport: () => isNarrow,
}))

const TOGGLE = '[data-testid="debts-owner-toggle-Пётр"]'
const META = '[data-testid="debts-owner-meta"]'
const PAY = '[data-testid="debts-owner-pay"]'
const FORM = '[data-testid="debts-payment"]'
const HINT = '[data-testid="debts-owner-collapse-hint"]'

function debt(id: number, extra: Partial<Debt> = {}): Debt {
  return {
    id,
    typeId: 1,
    sum: Money.fromMinorUnits(100_000),
    paySum: Money.zero,
    comment: null,
    ownerName: 'Пётр',
    date: parseLocalDate('2026-09-17')!,
    payComment: null,
    isDeleted: false,
    ...extra,
  }
}

function owner(debts: readonly Debt[]): DebtOwnerGroup {
  return { name: 'Пётр', debts: [...debts] }
}

function plain(value: string): string {
  return value.replaceAll(' ', ' ')
}

function mountPanel(group: DebtOwnerGroup, extra: Partial<InstanceType<typeof DebtOwnerPanel>['$props']> = {}) {
  return mount(DebtOwnerPanel, {
    props: {
      owner: group,
      isSelectable: () => false,
      selectedIds: new Set<number>(),
      paymentValue: null,
      isPaying: false,
      ...extra,
    },
  })
}

function payment(debtId: number): PaymentFormValue {
  return { debtId, sum: Money.fromMinorUnits(10_000), date: parseLocalDate('2026-09-17')!, comment: '' }
}

describe('книжная карточка держателя на узком экране', () => {
  it('свёрнутая карточка показывает остаток, исходную сумму и сводку строкой', () => {
    const wrapper = mountPanel(owner([
      debt(1, { paySum: Money.fromMinorUnits(50_000) }),
      debt(2, { sum: Money.fromMinorUnits(30_000), date: parseLocalDate('2026-09-10')! }),
    ]))

    expect(plain(wrapper.get('[data-testid="debts-owner-remainder"]').text())).toBe('800,00')
    expect(plain(wrapper.get('[data-testid="debts-owner-total"]').text())).toBe('из 1 300,00')
    expect(plain(wrapper.get(META).text())).toContain('2 долга')
    expect(plain(wrapper.get(META).text())).toContain('выплачено 38 %')
    expect(wrapper.find('[data-testid="debts-owner-count"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(false)
  })

  it('раскрытая карточка отдаёт строку на долг, итог по держателю и одну кнопку оплаты', async () => {
    const group = owner([
      debt(1, { date: parseLocalDate('2026-09-17')! }),
      debt(2, { date: parseLocalDate('2026-09-10')! }),
    ])
    const wrapper = mountPanel(group)

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.findAll('[data-testid^="debts-card-edit-"]')).toHaveLength(2)
    expect(plain(wrapper.get('[data-testid="debts-owner-sum"]').text())).toBe('2 000,00')
    expect(wrapper.findAll(PAY)).toHaveLength(1)

    await wrapper.get(PAY).trigger('click')

    expect(wrapper.emitted('open-payment')?.[0]).toEqual([group.debts[1]])
  })

  it('единственный долг не повторяет остаток итоговой строкой', async () => {
    const wrapper = mountPanel(owner([debt(1)]))

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.findAll('[data-testid^="debts-card-edit-"]')).toHaveLength(1)
    expect(wrapper.find('[data-testid="debts-owner-sum"]').exists()).toBe(false)
  })

  it('тап по строке долга просит правку, удалённый долг предлагает восстановление', async () => {
    const group = owner([debt(1), debt(2, { isDeleted: true })])
    const wrapper = mountPanel(group)

    await wrapper.get(TOGGLE).trigger('click')
    await wrapper.get('[data-testid="debts-card-edit-1"]').trigger('click')

    expect(wrapper.emitted('edit')?.[0]).toEqual([group.debts[0]])
    expect(wrapper.get('[data-testid="debts-card-edit-2"]').attributes('disabled')).toBeDefined()
    expect(wrapper.find('[data-testid="debts-card-restore-2"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="debts-card-delete-2"]').exists()).toBe(false)
  })

  it('полностью выплаченному держателю платить нечем', async () => {
    const wrapper = mountPanel(owner([debt(1, { paySum: Money.fromMinorUnits(100_000) })]))

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.get(PAY).attributes('disabled')).toBeDefined()
    expect(wrapper.get(META).text()).not.toContain('·')
  })

  it('открытый платёж не даёт свернуть карточку и объясняет отказ', async () => {
    const wrapper = mountPanel(owner([debt(1), debt(2)]))

    await wrapper.get(TOGGLE).trigger('click')
    await wrapper.setProps({ paymentValue: payment(2) })

    expect(wrapper.find(FORM).exists()).toBe(true)

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.find(FORM).exists()).toBe(true)
    expect(wrapper.get(TOGGLE).attributes('aria-expanded')).toBe('true')
    expect(wrapper.get(HINT).text()).toBe('сначала завершите или отмените платёж')

    await wrapper.setProps({ paymentValue: null })

    expect(wrapper.get(HINT).text()).toBe('')

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(false)
  })

  it('режим прощения раскрывает держателя, не отбирая ручное управление', async () => {
    const wrapper = mountPanel(owner([debt(1)]), { isSelectable: () => true })

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(true)

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(false)
  })

  it('выход из режима прощения не сворачивает раскрытое', async () => {
    const wrapper = mountPanel(owner([debt(1)]))

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(false)

    await wrapper.setProps({ isSelectable: () => true })

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(true)

    await wrapper.setProps({ isSelectable: () => false })

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(true)
  })

  it('на широком экране остаётся прежняя раскладка с чипами', () => {
    isNarrow.value = false

    const wrapper = mountPanel(owner([debt(1)]))

    expect(wrapper.find('[data-testid="debts-owner-count"]').exists()).toBe(true)
    expect(wrapper.find(TOGGLE).exists()).toBe(false)

    isNarrow.value = true
  })
})
