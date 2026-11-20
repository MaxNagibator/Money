import type { ChartOptions, TooltipItem } from 'chart.js'
import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { defineComponent } from 'vue'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { debtAccounts } from '../model/debt-accounts'
import DebtCyclesTimelineChart from './DebtCyclesTimelineChart.vue'

vi.mock('vue-chartjs', () => ({
  Bar: defineComponent({
    name: 'BarStub',
    props: {
      data: { type: Object, required: true },
      options: { type: Object, required: true },
    },
    template: '<canvas />',
  }),
}))

function debt(id: number, extra: Partial<Debt> = {}): Debt {
  return {
    id,
    ownerName: 'Иван',
    typeId: 1,
    sum: Money.fromWire('1000'),
    paySum: Money.zero,
    comment: null,
    payComment: null,
    date: parseLocalDate('2026-01-10')!,
    isDeleted: false,
    ...extra,
  }
}

describe('график циклов долгов', () => {
  it('задаёт целочисленные деления оси X и помечает закрытие без даты в подсказке', () => {
    const accounts = debtAccounts([
      debt(1, {
        ownerName: 'Старый',
        date: parseLocalDate('2026-01-10')!,
        sum: Money.fromWire('1000'),
        paySum: Money.fromWire('1000'),
        payComment: 'вернул наличными',
      }),
    ])

    const wrapper = mount(DebtCyclesTimelineChart, {
      props: { accounts, today: parseLocalDate('2026-01-11')! },
    })

    const bar = wrapper.findComponent({ name: 'BarStub' })
    const options = bar.props('options') as ChartOptions<'bar'>
    const xScale = options.scales?.x as { ticks?: { precision?: number } } | undefined
    const label = options.plugins?.tooltip?.callbacks?.label

    expect(xScale?.ticks?.precision).toBe(0)
    expect(label).toBeTypeOf('function')

    const lines = label?.call(
      {} as never,
      { datasetIndex: 0, dataIndex: 0 } as TooltipItem<'bar'>,
    )

    expect(lines).toEqual([
      'Мне должны (Закрыт, дата оплаты не сохранена)',
      'Начало: с 10.01.2026',
      'Пиковый остаток: 1\u00A0000\u00A0₽',
      'Операций: 1',
      'Есть оплаты без даты (1): фактический остаток может отличаться',
    ])

    wrapper.unmount()
  })
})
