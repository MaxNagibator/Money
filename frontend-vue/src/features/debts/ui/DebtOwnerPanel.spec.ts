import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import type { DebtMovement } from '@/domain/debt'
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

function DebtMovement(id: number, extra: Partial<DebtMovement> = {}): DebtMovement {
  return {
    id,
    typeId: 1,
    sum: Money.fromMinorUnits(100_000),
    comment: null,
    ownerName: 'Пётр',
    date: parseLocalDate('2026-09-17')!,
    isDeleted: false,
    ...extra,
  }
}

function owner(debts: readonly DebtMovement[]): DebtOwnerGroup {
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
  it('раскладывает все движения в две постоянные колонки по направлению', async () => {
    const wrapper = mountPanel(owner([
      DebtMovement(1, { typeId: 1 }),
      DebtMovement(2, { typeId: 2 }),
      DebtMovement(3, { typeId: 1 }),
      DebtMovement(4, { typeId: 2 }),
    ]))

    await wrapper.get(TOGGLE).trigger('click')

    const incoming = wrapper.get('[data-testid="debts-owner-incoming"]')
    const outgoing = wrapper.get('[data-testid="debts-owner-outgoing"]')

    expect(incoming.findAll('.debt-row')).toHaveLength(2)
    expect(outgoing.findAll('.debt-row')).toHaveLength(2)
    expect(plain(incoming.text())).toContain('1 000,00')
    expect(plain(outgoing.text())).toContain('1 000,00')
    expect(incoming.find('[data-testid="debts-card-1"]').exists()).toBe(false)
    expect(outgoing.find('[data-testid="debts-card-2"]').exists()).toBe(false)
  })

  it('свёрнутая карточка показывает остаток, оборот и число движений', () => {
    const wrapper = mountPanel(owner([
      DebtMovement(1, { typeId: 1 }),
      DebtMovement(2, { sum: Money.fromMinorUnits(30_000), date: parseLocalDate('2026-09-10')! }),
    ]))

    expect(plain(wrapper.get('[data-testid="debts-owner-remainder"]').text())).toBe('1 300,00')
    expect(plain(wrapper.get('[data-testid="debts-owner-total"]').text())).toBe('1 300,00')
    expect(plain(wrapper.get(META).text())).toContain('2 долга')
    expect(wrapper.find('[data-testid="debts-owner-count"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(false)
  })

  it('раскрытая карточка отдаёт строку на долг, итог по держателю и одну кнопку оплаты', async () => {
    const group = owner([
      DebtMovement(1, { date: parseLocalDate('2026-09-17')! }),
      DebtMovement(2, { date: parseLocalDate('2026-09-10')! }),
    ])
    const wrapper = mountPanel(group)

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.findAll('[data-testid^="debts-card-edit-"]')).toHaveLength(2)
    expect(plain(wrapper.get('[data-testid="debts-owner-sum"]').text())).toBe('2 000,00')
    expect(wrapper.findAll(PAY)).toHaveLength(1)

    await wrapper.get(PAY).trigger('click')

    const emittedDebt = wrapper.emitted('open-payment')?.[0]?.[0] as DebtMovement

    expect(emittedDebt.id).toBe(group.debts[1]?.id)
    expect(emittedDebt.sum.toMinorUnits()).toBe(200_000)
  })

  it('единственный долг не повторяет остаток итоговой строкой', async () => {
    const wrapper = mountPanel(owner([DebtMovement(1)]))

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.findAll('[data-testid^="debts-card-edit-"]')).toHaveLength(1)
    expect(wrapper.find('[data-testid="debts-owner-sum"]').exists()).toBe(false)
  })

  it('тап по строке долга просит правку, удалённый долг предлагает восстановление', async () => {
    const group = owner([DebtMovement(1), DebtMovement(2, { isDeleted: true })])
    const wrapper = mountPanel(group)

    await wrapper.get(TOGGLE).trigger('click')
    await wrapper.get('[data-testid="debts-card-edit-1"]').trigger('click')

    expect(wrapper.emitted('edit')?.[0]).toEqual([group.debts[0]])
    expect(wrapper.get('[data-testid="debts-card-edit-2"]').attributes('disabled')).toBeDefined()
    expect(wrapper.find('[data-testid="debts-card-restore-2"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="debts-card-delete-2"]').exists()).toBe(false)
  })

  it('держателю с нулевым балансом платить нечем', async () => {
    const wrapper = mountPanel(owner([
      DebtMovement(1, { typeId: 1 }),
      DebtMovement(2, { typeId: 2 }),
    ]))

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.get(PAY).attributes('disabled')).toBeDefined()
    expect(wrapper.get(META).text()).not.toContain('·')
  })

  it('открытый платёж не даёт свернуть карточку и объясняет отказ', async () => {
    const wrapper = mountPanel(owner([DebtMovement(1), DebtMovement(2)]))

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
    const wrapper = mountPanel(owner([DebtMovement(1)]), { isSelectable: () => true })

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(true)

    await wrapper.get(TOGGLE).trigger('click')

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(false)
  })

  it('выход из режима прощения не сворачивает раскрытое', async () => {
    const wrapper = mountPanel(owner([DebtMovement(1)]))

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(false)

    await wrapper.setProps({ isSelectable: () => true })

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(true)

    await wrapper.setProps({ isSelectable: () => false })

    expect(wrapper.find('[data-testid="debts-card-edit-1"]').exists()).toBe(true)
  })

  it('на широком экране остаётся прежняя раскладка с чипами', () => {
    isNarrow.value = false

    const wrapper = mountPanel(owner([DebtMovement(1)]))

    expect(wrapper.find('[data-testid="debts-owner-count"]').exists()).toBe(true)
    expect(wrapper.find(TOGGLE).exists()).toBe(false)

    isNarrow.value = true
  })
})
