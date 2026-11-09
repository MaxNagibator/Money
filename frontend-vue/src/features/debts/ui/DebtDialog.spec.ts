import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { nextTick, ref } from 'vue'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { editDebtForm, newDebtForm } from '../model/debt-form'
import DebtDialog from './DebtDialog.vue'

const isNarrow = ref(true)

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
