import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { defineComponent, h, nextTick, ref, shallowRef } from 'vue'
import type { Category } from '@/domain/category'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { OperationFormValue } from '../model/operation-form'
import type { OperationSave } from '../model/operation-save'
import { provideOperationSave } from '../model/operation-save'
import OperationDialog from './OperationDialog.vue'

vi.mock('@/shared/ui/useNarrowViewport', () => ({
  NARROW_VIEWPORT: '(max-width: 599.98px)',
  useNarrowViewport: () => ref(true),
}))

vi.mock('../model/category-inference', () => ({
  inferCategoryCached: () => Promise.resolve(null),
  resetCategoryInference: () => {},
}))

vi.mock('@/shared/ui/usePlaceSuggestions', () => ({
  usePlaceSuggestions: () => ({ find: () => Promise.resolve([]), reset: () => {} }),
  findPlaces: () => Promise.resolve([]),
  resetPlaces: () => {},
}))

const COMMENT = 'Обед с коллегами'
const PLACE = 'Кафе у дома'

const category: Category = {
  id: 1,
  parentId: null,
  name: 'Еда',
  color: null,
  order: null,
  operationTypeId: 1,
  isDeleted: false,
}

function draft(): OperationFormValue {
  return {
    id: null,
    operationTypeId: 1,
    categoryId: category.id,
    sum: Money.fromMinorUnits(123400),
    date: parseLocalDate('2026-09-17'),
    comment: COMMENT,
    place: PLACE,
  }
}

interface DialogInstance {
  open: (value: OperationFormValue) => void
}

interface DialogOptions {
  removable?: boolean
  onRemove?: () => void
  value?: OperationFormValue
}

function openDialog(save: OperationSave, options: DialogOptions = {}) {
  const dialog = shallowRef<DialogInstance | null>(null)

  const host = defineComponent({
    setup() {
      provideOperationSave(save)

      return () => h(OperationDialog, {
        ref: dialog,
        categories: [category],
        isSaving: false,
        removable: options.removable === true,
        onRemove: options.onRemove ?? (() => {}),
      })
    },
  })

  const wrapper = mount(host, { global: { stubs: { teleport: true } }, attachTo: document.body })

  dialog.value?.open(options.value ?? draft())

  return wrapper
}

async function settle(): Promise<void> {
  await nextTick()
  await nextTick()
  await nextTick()
}

describe('лист операции на узком экране', () => {
  it('тап по затемнению во время сохранения не закрывает лист и не роняет черновик', async () => {
    let reject: (error: Error) => void = () => {}
    const save = vi.fn<OperationSave>(() => new Promise<void>((_resolve, fail) => {
      reject = fail
    }))

    const wrapper = openDialog(save)
    await settle()

    await wrapper.get('[data-testid="operation-dialog-save"]').trigger('click')
    await nextTick()

    expect(save).toHaveBeenCalledTimes(1)

    await wrapper.get('[data-testid="operation-dialog-scrim"]').trigger('click')
    await nextTick()

    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(true)

    reject(new Error('Сеть недоступна'))
    await settle()

    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(true)
    expect(wrapper.get('[data-testid="operation-dialog-comment"] textarea').element).toHaveProperty('value', COMMENT)
    expect(wrapper.get('[data-testid="operation-dialog-place"] input').element).toHaveProperty('value', PLACE)
    expect(wrapper.get('[data-testid="operation-dialog-sum"] input').element).toHaveProperty('value', '1234')

    wrapper.unmount()
  })

  it('кнопка «Закрыть» во время сохранения закрывает лист, и отказ его не возвращает', async () => {
    let reject: (error: Error) => void = () => {}
    const save = vi.fn<OperationSave>(() => new Promise<void>((_resolve, fail) => {
      reject = fail
    }))

    const wrapper = openDialog(save)
    await settle()

    await wrapper.get('[data-testid="operation-dialog-save"]').trigger('click')
    await nextTick()

    expect(save).toHaveBeenCalledTimes(1)

    await wrapper.get('[data-testid="operation-dialog-cancel"]').trigger('click')
    await nextTick()

    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(false)

    reject(new Error('Сеть недоступна'))
    await settle()

    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(false)
    expect(wrapper.findComponent(OperationDialog).emitted('notice')).toEqual([['Сеть недоступна']])

    wrapper.unmount()
  })

  it('удачное сохранение закрывает лист', async () => {
    const save = vi.fn<OperationSave>(() => Promise.resolve())

    const wrapper = openDialog(save)
    await settle()

    await wrapper.get('[data-testid="operation-dialog-save"]').trigger('click')
    await settle()

    expect(save).toHaveBeenCalledTimes(1)
    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="operation-dialog-scrim"]').exists()).toBe(false)

    wrapper.unmount()
  })

  it('тап по затемнению вне сохранения не закрывает лист', async () => {
    const save = vi.fn<OperationSave>(() => Promise.resolve())

    const wrapper = openDialog(save)
    await settle()

    await wrapper.get('[data-testid="operation-dialog-scrim"]').trigger('click')
    await nextTick()

    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="operation-dialog-scrim"]').exists()).toBe(true)

    wrapper.unmount()
  })
})

describe('удаление из листа операции', () => {
  const saved: OperationFormValue = { ...draft(), id: 42 }

  it('правка отдаёт удаление наверх и закрывает лист', async () => {
    const onRemove = vi.fn()

    const wrapper = openDialog(() => Promise.resolve(), { removable: true, onRemove, value: saved })
    await settle()

    await wrapper.get('[data-testid="operation-dialog-remove"]').trigger('click')
    await nextTick()

    expect(onRemove).toHaveBeenCalledTimes(1)
    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(false)

    wrapper.unmount()
  })

  it('удаление во время сохранения не уходит наверх и не закрывает лист', async () => {
    const onRemove = vi.fn()
    const save = vi.fn<OperationSave>(() => new Promise<void>(() => {}))

    const wrapper = openDialog(save, { removable: true, onRemove, value: saved })
    await settle()

    await wrapper.get('[data-testid="operation-dialog-save"]').trigger('click')
    await nextTick()

    await wrapper.get('[data-testid="operation-dialog-remove"]').trigger('click')
    await nextTick()

    expect(onRemove).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(true)

    wrapper.unmount()
  })

  it.each<[string, DialogOptions]>([
    ['создании записи', { removable: true }],
    ['листе, которому удалять не разрешили', { value: saved }],
  ])('кнопки удаления нет при %s', async (_name, options) => {
    const wrapper = openDialog(() => Promise.resolve(), options)
    await settle()

    expect(wrapper.find('[data-testid="operation-dialog-remove"]').exists()).toBe(false)

    wrapper.unmount()
  })
})
