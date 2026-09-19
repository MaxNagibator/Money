import type { QueryClient } from '@tanstack/vue-query'
import type { InjectionKey } from 'vue'
import { inject, provide } from 'vue'
import type { OperationFormValue } from './operation-form'

export type OperationSave = (value: OperationFormValue) => Promise<void>

export const SAVE_IN_PROGRESS = 'Сохранение операции ещё идёт, повторите после его завершения'

const OPERATION_SAVE: InjectionKey<OperationSave> = Symbol('operation-save')

export function provideOperationSave(save: OperationSave): void {
  provide(OPERATION_SAVE, serialized(save))
}

export function provideOperationWriter(queryClient: QueryClient): void {
  provideOperationSave(async value => {
    const { saveOperation } = await import('./operation-writer')

    await saveOperation(queryClient, value)
  })
}

export function useOperationSave(): OperationSave {
  const save = inject(OPERATION_SAVE, null)

  if (save === null) {
    throw new Error('Диалог операции открыт вне оболочки приложения: сохранять его нечем')
  }

  return save
}

function serialized(save: OperationSave): OperationSave {
  let pending: Promise<void> | null = null

  return async value => {
    if (pending !== null) {
      throw new Error(SAVE_IN_PROGRESS)
    }

    pending = save(value)

    try {
      await pending
    } finally {
      pending = null
    }
  }
}
