import { mount } from '@vue/test-utils'
import { describe, expect, it, onTestFinished } from 'vitest'
import { defineComponent, h } from 'vue'
import { Money } from '@/domain/money'
import type { OperationFormValue } from './operation-form'
import type { OperationSave } from './operation-save'
import { provideOperationSave, SAVE_IN_PROGRESS, useOperationSave } from './operation-save'

describe('последовательность сохранений операции', () => {
  it('второе сохранение во время первого не уходит на сервер и падает с сообщением', async () => {
    const started: OperationFormValue[] = []
    let release!: () => void

    const injected = injectSave(value => {
      started.push(value)

      return new Promise<void>(resolve => {
        release = resolve
      })
    })

    const first = injected(form(150))

    await expect(injected(form(200))).rejects.toThrow(SAVE_IN_PROGRESS)
    expect(started).toHaveLength(1)

    release()
    await first

    const second = injected(form(200))

    expect(started).toHaveLength(2)

    release()
    await second
  })
})

function injectSave(save: OperationSave): OperationSave {
  let injected!: OperationSave

  const child = (): null => {
    injected = useOperationSave()

    return null
  }

  const wrapper = mount(defineComponent({
    setup() {
      provideOperationSave(save)

      return () => h(child)
    },
  }))

  onTestFinished(() => {
    wrapper.unmount()
  })

  return injected
}

function form(sum: number): OperationFormValue {
  return {
    id: null,
    operationTypeId: 1,
    categoryId: 2,
    sum: Money.parseInput(String(sum))!,
    date: null,
    comment: '',
    place: '',
  }
}
