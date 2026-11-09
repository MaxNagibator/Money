import { mount } from '@vue/test-utils'
import { afterEach, describe, expect, it } from 'vitest'
import { nextTick } from 'vue'
import MudDialog from './MudDialog.vue'

function mountDialog(closeOnEscapeKey?: boolean) {
  return mount(MudDialog, {
    props: {
      visible: true,
      ...closeOnEscapeKey === undefined ? {} : { closeOnEscapeKey },
    },
    slots: {
      content: '<button type="button" id="first">Первая</button><button type="button" id="last">Вторая</button>',
    },
    attachTo: document.body,
  })
}

function press(key: string, shiftKey = false): void {
  document.dispatchEvent(new KeyboardEvent('keydown', { key, shiftKey, bubbles: true }))
}

let wrapper: ReturnType<typeof mountDialog> | undefined

afterEach(() => {
  wrapper?.unmount()
  wrapper = undefined
})

describe('закрытие диалога по Escape', () => {
  it('по умолчанию Escape не закрывает диалог', async () => {
    wrapper = mountDialog()

    press('Escape')
    await nextTick()

    expect(wrapper.emitted('update:visible')).toBeUndefined()
    expect(document.querySelector('.mud-dialog')).not.toBeNull()
  })

  it('closeOnEscapeKey возвращает закрытие по Escape', async () => {
    wrapper = mountDialog(true)

    press('Escape')
    await nextTick()

    expect(wrapper.emitted('update:visible')).toEqual([[false]])
  })

  it('Escape без закрытия не ломает ловушку фокуса', async () => {
    wrapper = mountDialog()

    press('Escape')
    press('Tab', true)
    await nextTick()

    const bumpers = document.querySelectorAll<HTMLElement>('.mud-dialog > .fixed')
    bumpers[1]?.dispatchEvent(new FocusEvent('focus'))
    await nextTick()

    expect(document.activeElement?.id).toBe('last')
  })
})
