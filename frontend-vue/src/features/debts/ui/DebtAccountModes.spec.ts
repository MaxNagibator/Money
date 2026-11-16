import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { debtAccounts } from '../model/debt-accounts'
import DebtAccountPanel from './DebtAccountPanel.vue'

function account() {
  const base: Debt = {
    id: 1, ownerName: 'Иван', typeId: 1, sum: Money.fromWire('1000'), paySum: Money.zero,
    date: parseLocalDate('2026-11-01')!, comment: 'Выдал на ремонт', payComment: null, isDeleted: false,
  }
  return debtAccounts([base, { ...base, id: 2, typeId: 2, sum: Money.fromWire('400'), date: parseLocalDate('2026-11-02')!, comment: 'Вернул часть' }])[0]!
}

describe('два представления одной карточки', () => {
  it('скрывает действия и счётчик в закрытой карточке, сохраняя её при смене режима', async () => {
    const wrapper = mount(DebtAccountPanel, { props: { account: account(), mode: 'normal' } })
    expect(wrapper.find('[data-testid="debt-account-outgoing"]').exists()).toBe(false)
    expect(wrapper.text()).not.toContain('Записей:')
    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="debt-account-incoming"]').exists()).toBe(false)
    await wrapper.setProps({ mode: 'accounting' })
    expect(wrapper.find('table').exists()).toBe(true)
    wrapper.unmount()
  })

  it('в регистре показывает пять колонок без действий, обороты и сальдо после каждого события', async () => {
    const wrapper = mount(DebtAccountPanel, { props: { account: account(), mode: 'accounting' } })
    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    expect(wrapper.findAll('thead th').map(cell => cell.text())).toEqual(['Дата', 'Основание / Комментарий', 'Уход (Выдано)', 'Приход (Получено)', 'Сальдо'])
    expect(wrapper.findAll('[data-testid^="debt-movement-balance-"]').map(cell => cell.text())).toEqual(['600,00 ₽', '1 000,00 ₽'])
    expect(wrapper.get('tfoot').text()).toContain('ИТОГО обороты')
    expect(wrapper.get('tfoot').text()).toContain('1 000,00 ₽')
    expect(wrapper.get('tfoot').text()).toContain('400,00 ₽')
    expect(wrapper.findAll('.register-actions')).toHaveLength(0)
    expect(wrapper.find('[data-testid="debt-account-forgive"]').exists()).toBe(false)
    wrapper.unmount()
  })

  it('в обычной ленте показывает суммы и направления займов без меню изменения', async () => {
    const wrapper = mount(DebtAccountPanel, { props: { account: account(), mode: 'normal' } })
    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    expect(wrapper.find('table').exists()).toBe(false)
    expect(wrapper.get('[data-testid="debt-movement-out-1"]').text()).toBe('−1 000 ₽')
    expect(wrapper.get('[data-testid="debt-movement-in-2"]').text()).toBe('+400 ₽')
    expect(wrapper.find('[data-testid="debt-movement-menu-1"]').exists()).toBe(false)
    expect(wrapper.findAll('.feed-meta').map(row => row.text())).toEqual(['02.11.2026 · Взял в долг', '01.11.2026 · Дал в долг'])
    expect(wrapper.get('[data-testid="debt-movement-in-2"]').classes()).toContain('cash-incoming')
    wrapper.unmount()
  })

  it('различает собственный заём и возврат денег пользователю', async () => {
    const original = account()
    const payable = debtAccounts(original.debts.map(debt => ({ ...debt, typeId: debt.typeId === 1 ? 2 : 1 })))[0]!
    const wrapper = mount(DebtAccountPanel, { props: { account: payable, mode: 'normal' } })
    await wrapper.get('[data-testid="debt-account-toggle"]').trigger('click')
    expect(wrapper.findAll('.feed-meta').map(row => row.text())).toEqual(['02.11.2026 · Дал в долг', '01.11.2026 · Взял в долг'])
    expect(wrapper.get('[data-testid="debt-movement-in-1"]').classes()).not.toContain('cash-incoming')
    wrapper.unmount()
  })
})
