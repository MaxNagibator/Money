import { mount } from '@vue/test-utils'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import type { Category } from '@/domain/category'
import SmartCategory from './SmartCategory.vue'

const BLUR_DELAY_MS = 200
const CATEGORY_MISSING = 'Категория удалена, выберите другую'
const DELETED_CATEGORY_ID = 42

const food: Category = {
  id: 1,
  parentId: null,
  name: 'Еда',
  color: null,
  order: null,
  operationTypeId: 1,
  isDeleted: false,
}

const cafe: Category = { ...food, id: 2, name: 'Кафе' }

function picker(modelValue: number | null, options: readonly Category[] = [food, cafe]) {
  return mount(SmartCategory, { props: { modelValue, options } })
}

async function loseFocus(input: { trigger: (event: string) => Promise<void> }): Promise<void> {
  await input.trigger('blur')
  vi.advanceTimersByTime(BLUR_DELAY_MS)
  await nextTick()
}

describe('поле категории', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('категорию, которой нет в справочнике, не обнуляет и объясняет пустое поле', async () => {
    const wrapper = picker(DELETED_CATEGORY_ID)
    const input = wrapper.get('input')

    expect(input.element.value).toBe('')
    expect(wrapper.text()).toContain(CATEGORY_MISSING)

    await input.trigger('focus')
    await loseFocus(input)

    expect(wrapper.emitted('update:modelValue')).toBeUndefined()

    wrapper.unmount()
  })

  it('очистка поля снимает выбранную категорию', async () => {
    const wrapper = picker(cafe.id)
    const input = wrapper.get('input')

    expect(input.element.value).toBe('Кафе')

    await input.setValue('')

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual([null])

    wrapper.unmount()
  })

  it('нераспознанный ввод при потере фокуса возвращает имя выбранной категории', async () => {
    const wrapper = picker(cafe.id)
    const input = wrapper.get('input')

    await input.setValue('Каф')
    await loseFocus(input)

    expect(input.element.value).toBe('Кафе')
    expect(wrapper.emitted('update:modelValue')).toBeUndefined()

    wrapper.unmount()
  })

  it('Enter принимает подсвеченную категорию и гасит отправку формы', async () => {
    const wrapper = picker(null)
    const input = wrapper.get('input')

    await input.trigger('focus')
    await input.trigger('keydown', { key: 'ArrowDown' })

    const event = new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true })

    input.element.dispatchEvent(event)

    expect(event.defaultPrevented).toBe(true)
    expect(wrapper.emitted('update:modelValue')).toEqual([[food.id]])

    wrapper.unmount()
  })
})
