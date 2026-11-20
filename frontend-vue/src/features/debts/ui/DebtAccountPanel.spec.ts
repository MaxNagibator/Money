import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { defineComponent } from 'vue'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { debtAccounts } from '../model/debt-accounts'
import DebtAccountPanel from './DebtAccountPanel.vue'

vi.mock('vue-chartjs', () => ({
  Line: defineComponent({
    name: 'LineStub',
    props: {
      data: { type: Object, required: true },
      options: { type: Object, required: true },
      plugins: { type: Array, default: () => [] },
    },
    template: '<canvas />',
  }),
}))

function debt(id: number, extra: Partial<Debt> = {}): Debt {
  return {
    id, ownerName: 'Иван', typeId: 1, sum: Money.fromWire('1000'), paySum: Money.zero,
    date: parseLocalDate('2026-11-10')!, comment: 'за обед', payComment: null, isDeleted: false,
    ...extra,
  }
}

function mountPanel(props: InstanceType<typeof DebtAccountPanel>['$props']) {
  return mount(DebtAccountPanel, { props })
}

describe('единая карточка человека', () => {
  it('показывает баланс и разносит движения по колонкам без отрицательных сумм', async () => {
    const wrapper = mount(DebtAccountPanel, {
      props: { account: debtAccounts([debt(1), debt(2, { typeId: 2, sum: Money.fromWire('1200') })])[0]! },
    })

    expect(wrapper.get('[data-testid="debt-account-balance"]').text()).toContain('Я должен')
    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    expect(wrapper.get('[data-testid="debt-movement-out-1"]').text()).toContain('1 000')
    expect(wrapper.get('[data-testid="debt-movement-in-2"]').text()).toContain('1 200')
    expect(wrapper.get('[data-testid="debt-movement-in-2"]').text()).not.toContain('-')

    expect(wrapper.find('[data-testid="debt-account-incoming"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="debt-account-outgoing"]').exists()).toBe(false)
    expect(wrapper.findAll('[data-testid^="debt-movement-menu-"]')).toHaveLength(0)
    expect(wrapper.emitted('add')).toBeUndefined()
    wrapper.unmount()
  })

  it('не предлагает редактировать агрегат старых оплат, сохраняя исходный текст', async () => {
    const wrapper = mount(DebtAccountPanel, {
      props: { account: debtAccounts([debt(1, { paySum: Money.fromWire('300'), payComment: '2026.11.01 300\nпояснение' })])[0]! },
    })

    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    expect(wrapper.text()).toContain('Старые оплаты')
    expect(wrapper.text()).toContain('пояснение')
    expect(wrapper.findAll('[data-testid^="debt-movement-menu-"]')).toHaveLength(0)
    expect(wrapper.text()).toContain('Дата не сохранена')
    wrapper.unmount()
  })

  it('показывает старую полную оплату без недоступного на сервере редактирования', async () => {
    const wrapper = mount(DebtAccountPanel, {
      props: { account: debtAccounts([debt(1, { paySum: Money.fromWire('1000') })])[0]! },
    })

    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    expect(wrapper.findAll('[data-testid^="debt-movement-edit-"]')).toHaveLength(0)
    expect(wrapper.get('[data-testid="debt-account-balance"]').text()).toContain('Расчёты закрыты')
    wrapper.unmount()
  })

  it('показывает режим debts-rework в две колонки без кнопок редактирования', async () => {
    const account = debtAccounts([
      debt(1, { comment: 'Обед' }),
      debt(2, { typeId: 2, sum: Money.fromWire('1200'), comment: 'Такси' }),
    ])[0]!
    const wrapper = mount(DebtAccountPanel, {
      props: { account, mode: 'rework' },
    })

    expect(wrapper.get('[data-testid="debt-account-balance"]').text()).toContain('-200')
    expect(wrapper.get('[data-testid="debt-account-turnover"]').text()).toContain('2 / 2')

    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    expect(wrapper.text()).toContain('Уход')
    expect(wrapper.text()).toContain('Приход')
    expect(wrapper.get('[data-testid="debt-movement-out-1"]').text()).toContain('1 000')
    expect(wrapper.get('[data-testid="debt-movement-in-2"]').text()).toContain('1 200')
    expect(wrapper.findAll('button')).toHaveLength(0)
    wrapper.unmount()
  })

  it('показывает доработанный режим с тегами, итогами колонок и примечаниями без правок', () => {
    const account = debtAccounts([
      debt(1, { comment: 'На ремонт машины', paySum: Money.fromWire('300'), payComment: '2026-11-12 300 Перевод' }),
      debt(2, { typeId: 2, sum: Money.fromWire('500'), isDeleted: true, comment: 'Удалённый' }),
    ])[0]!
    const wrapper = mount(DebtAccountPanel, {
      props: { account, mode: 'refined', initiallyExpanded: true },
    })

    expect(wrapper.get('[data-testid="debt-account-balance"]').text()).toContain('Мне должны: 700')
    expect(wrapper.text()).toContain('Дал')
    expect(wrapper.text()).toContain('Вернули')
    expect(wrapper.text()).toContain('Итого уход')
    expect(wrapper.text()).toContain('Итого приход')
    expect(wrapper.text()).toContain('Удалённые записи показаны зачёркнутыми')
    expect(wrapper.findAll('[data-testid^="debt-movement-edit-"]')).toHaveLength(0)
    expect(wrapper.findAll('[data-testid="debts-owner-pay"]')).toHaveLength(0)
    wrapper.unmount()
  })

  it('сохраняет развёрнутое состояние при переключении всех четырёх режимов', async () => {
    const account = debtAccounts([debt(1), debt(2, { typeId: 2, sum: Money.fromWire('1200') })])[0]!
    const wrapper = mount(DebtAccountPanel, {
      props: { account, mode: 'normal' },
    })

    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    expect(wrapper.get('[data-testid="debt-movement-out-1"]').isVisible()).toBe(true)

    for (const mode of ['accounting', 'rework', 'refined', 'normal'] as const) {
      await wrapper.setProps({ mode })
      expect(wrapper.find('[data-testid="debt-movement-out-1"]').exists()).toBe(true)
    }

    wrapper.unmount()
  })

  it('показывает компактные полосы в обычном режиме с индикатором, датой, тегом и суммой', async () => {
    const account = debtAccounts([
      debt(1, { comment: 'Кофе', paySum: Money.fromWire('300'), payComment: '2026.11.12 300 Вернул часть' }),
      debt(2, { typeId: 2, sum: Money.fromWire('400'), date: parseLocalDate('2026-11-11')!, comment: 'Билеты' }),
    ])[0]!
    const wrapper = mountPanel({ account, mode: 'normal' })

    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    const rows = wrapper.findAll('.feed-row')
    expect(rows).toHaveLength(3)
    expect(rows[0]!.find('.feed-indicator--payment').exists()).toBe(true)
    expect(rows[0]!.get('.feed-date').text()).toBe('12.11.2026')
    expect(rows[0]!.get('.feed-tag').text()).toBe('Вернули')
    expect(rows[0]!.get('.feed-meta').text()).toBe('12.11.2026 · Вернули')
    expect(rows[0]!.get('[data-testid="debt-movement-in-1"]').classes()).toContain('cash-incoming')
    expect(rows[1]!.find('.feed-indicator--debt').exists()).toBe(true)
    expect(rows[1]!.get('.feed-tag').text()).toBe('Взял в долг')
    expect(rows[2]!.find('.feed-indicator--debt').exists()).toBe(true)
    expect(rows[2]!.get('.feed-tag').text()).toBe('Дал в долг')
    wrapper.unmount()
  })

  it('управляет личным графиком баланса в обычном и бухгалтерском режимах', async () => {
    const account = debtAccounts([
      debt(1, { comment: 'Займ' }),
      debt(2, { typeId: 2, sum: Money.fromWire('400'), date: parseLocalDate('2026-11-12')!, comment: 'Часть' }),
    ])[0]!
    const wrapper = mountPanel({ account, mode: 'normal', today: parseLocalDate('2026-11-15')! })

    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    const chartToggle = wrapper.get('[data-testid="debt-account-chart-toggle"]')
    expect(chartToggle.attributes('aria-expanded')).toBe('false')
    expect(wrapper.find('[data-testid="debt-account-chart"]').exists()).toBe(false)

    await chartToggle.trigger('click')
    expect(chartToggle.attributes('aria-expanded')).toBe('true')
    expect(wrapper.find('[data-testid="debt-account-chart"]').exists()).toBe(true)

    await wrapper.setProps({ mode: 'accounting' })
    expect(wrapper.find('[data-testid="debt-account-chart"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="debt-account-chart-undated-note"]').exists()).toBe(false)
    wrapper.unmount()

    const preExpanded = mountPanel({
      account,
      mode: 'normal',
      expanded: true,
      chartInitiallyOpen: true,
      today: parseLocalDate('2026-11-15')!,
    })
    expect(preExpanded.get('[data-testid="debt-account-chart-toggle"]').attributes('aria-expanded')).toBe('true')
    expect(preExpanded.find('[data-testid="debt-account-chart"]').exists()).toBe(true)
    preExpanded.unmount()
  })

  it('показывает предупреждение под личным графиком, если есть оплаты без даты', () => {
    const account = debtAccounts([
      debt(1, { comment: 'Займ', paySum: Money.fromWire('300'), payComment: 'старый возврат' }),
    ])[0]!
    const wrapper = mountPanel({ account, mode: 'normal', initiallyExpanded: true, today: parseLocalDate('2026-11-15')! })

    const note = wrapper.get('[data-testid="debt-account-chart-undated-note"]')
    expect(note.text()).toContain('Оплаты без даты (1) не включены в график: остаток по графику может отличаться от остатка карточки.')
    wrapper.unmount()
  })

  it('скрывает переключатель личного графика, если нет не-удалённых датированных движений', () => {
    const account = debtAccounts([debt(1, { isDeleted: true })])[0]!
    const wrapper = mountPanel({ account, mode: 'normal', initiallyExpanded: true })

    expect(wrapper.find('[data-testid="debt-account-chart-toggle"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="debt-account-chart"]').exists()).toBe(false)
    wrapper.unmount()
  })
})
