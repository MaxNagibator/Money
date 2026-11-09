import type { ComputedRef, Ref } from 'vue'
import { computed, ref, shallowRef } from 'vue'
import { DEBT_SAVE_ERROR } from '@/api/adapters/debts'
import { trimToNull } from '@/domain/text'
import type { Category } from '@/domain/category'
import type { Debt } from '@/domain/debt'
import type { OperationTypeId } from '@/domain/operation-type'
import type { Clock } from '@/platform/clock'
import { systemClock } from '@/platform/clock'
import type { DebtTypeGroup } from './debt'
import { canForgiveDebt, filterDebtGroups, groupDebts } from './debt'
import type { DebtFormValue } from './debt-form'
import { editDebtForm, newDebtForm, toDebtDraft, validateDebtForm } from './debt-form'
import type { PaymentFormValue } from './payment-form'
import { newPaymentForm, toPaymentDraft, validatePaymentForm } from './payment-form'
import type { DebtOwnersState } from './useDebtOwners'
import { useDebtOwners } from './useDebtOwners'
import type { DebtsState } from './useDebts'
import { useDebts } from './useDebts'

export const DEBT_SAVED_NOTICE = 'Успех!'
export const DEBT_MERGED_NOTICE = 'Долги объединены'
export const DEBT_NOT_FORGIVABLE = 'Нельзя простить этот долг'
export const DEBT_SELECTION_REQUIRED = 'Выберите долги для прощения'
export const DEBT_CATEGORY_REQUIRED = 'Выберите категорию расходов'
export const DEBT_CATEGORY_NOT_EXPENSE = 'Можно прощать долги только в категории расходов'
export const DEBT_FORGIVE_COMMENT = 'Перенос из долгов:'

const EXPENSE_OPERATION_TYPE_ID: OperationTypeId = 1

export interface DebtsNotice {
  text: string
  type: 'success' | 'warning' | 'error'
}

export interface DebtsScreenState {
  debts: DebtsState
  owners: DebtOwnersState
  search: Ref<string>
  showPaid: Ref<boolean>
  groups: ComputedRef<DebtTypeGroup[]>
  notice: Ref<DebtsNotice | null>
  isTypeExpanded: (typeId: number) => boolean
  toggleType: (typeId: number) => void
  form: Ref<DebtFormValue | null>
  isSaving: Ref<boolean>
  startCreate: (typeId?: number | null) => void
  startEdit: (debt: Debt) => void
  cancelEdit: () => void
  submit: (value: DebtFormValue) => Promise<void>
  remove: (debt: Debt) => Promise<void>
  removeEdited: () => Promise<void>
  restore: (debt: Debt) => Promise<void>
  payment: Ref<PaymentFormValue | null>
  isPaying: Ref<boolean>
  startPayment: (debt: Debt) => void
  cancelPayment: (debt: Debt) => void
  submitPayment: (value: PaymentFormValue) => Promise<void>
  isForgiveOpen: Ref<boolean>
  isForgiving: Ref<boolean>
  forgiveComment: Ref<string>
  forgiveCategoryId: Ref<number | null>
  expenseCategories: ComputedRef<Category[]>
  selectedIds: ComputedRef<ReadonlySet<number>>
  selectedCount: ComputedRef<number>
  openForgive: (state: boolean) => void
  isSelectable: (debt: Debt) => boolean
  toggleSelection: (debt: Debt) => void
  selectAllVisible: () => void
  clearSelection: () => void
  forgive: () => Promise<void>
  isMergeOpen: Ref<boolean>
  isMerging: Ref<boolean>
  ownerFromId: Ref<number | null>
  ownerToId: Ref<number | null>
  openMerge: (state: boolean) => void
  swapOwners: () => void
  merge: () => Promise<void>
}

export function useDebtsScreen(
  categories: Ref<readonly Category[]> | ComputedRef<readonly Category[]>,
  clock: Clock = systemClock,
): DebtsScreenState {
  const showPaid = ref(false)
  const debts = useDebts(showPaid)
  const owners = useDebtOwners()

  const search = ref('')
  const notice = ref<DebtsNotice | null>(null)
  const collapsedTypes = ref(new Set<number>())

  const form = shallowRef<DebtFormValue | null>(null)
  const isSaving = ref(false)

  const payment = shallowRef<PaymentFormValue | null>(null)
  const isPaying = ref(false)

  const isForgiveOpen = ref(false)
  const isForgiving = ref(false)
  const forgiveComment = ref(DEBT_FORGIVE_COMMENT)
  const forgiveCategoryId = ref<number | null>(null)
  const selectedIds = ref(new Set<number>())

  const isMergeOpen = ref(false)
  const isMerging = ref(false)
  const ownerFromId = ref<number | null>(null)
  const ownerToId = ref<number | null>(null)

  const groups = computed(() => filterDebtGroups(groupDebts(debts.debts.value), search.value))

  const expenseCategories = computed(() => categories.value.filter(
    category => category.operationTypeId === EXPENSE_OPERATION_TYPE_ID && !category.isDeleted,
  ))

  const isSelectable = (debt: Debt): boolean => isForgiveOpen.value && canForgiveDebt(debt)

  const clearSelection = (): void => {
    selectedIds.value = new Set()
  }

  const removeById = async (id: number): Promise<void> => {
    try {
      await debts.remove(id)
    } catch (error) {
      notice.value = { text: messageOf(error, 'Не удалось удалить долг'), type: 'error' }
    }
  }

  return {
    debts,
    owners,
    search,
    showPaid,
    groups,
    notice,
    isTypeExpanded: typeId => !collapsedTypes.value.has(typeId),
    toggleType: typeId => {
      const next = new Set(collapsedTypes.value)

      if (!next.delete(typeId)) {
        next.add(typeId)
      }

      collapsedTypes.value = next
    },
    form,
    isSaving,
    startCreate: (typeId = null) => {
      form.value = newDebtForm(clock.today(), typeId)
    },
    startEdit: debt => {
      form.value = editDebtForm(debt)
    },
    cancelEdit: () => {
      form.value = null
    },
    submit: async value => {
      if (isSaving.value || Object.keys(validateDebtForm(value)).length > 0) {
        return
      }

      isSaving.value = true

      try {
        await debts.save(value.id, toDebtDraft(value))
        form.value = null
        notice.value = { text: DEBT_SAVED_NOTICE, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, DEBT_SAVE_ERROR), type: 'error' }
      } finally {
        isSaving.value = false
      }
    },
    remove: debt => removeById(debt.id),
    removeEdited: async () => {
      const id = form.value?.id ?? null

      if (id === null) {
        return
      }

      form.value = null

      await removeById(id)
    },
    restore: async debt => {
      try {
        await debts.restore(debt.id)
      } catch (error) {
        notice.value = { text: messageOf(error, 'Не удалось восстановить долг'), type: 'error' }
      }
    },
    payment,
    isPaying,
    startPayment: debt => {
      if (isPaying.value) {
        return
      }

      payment.value = newPaymentForm(debt, clock.today())
    },
    cancelPayment: debt => {
      if (payment.value?.debtId !== debt.id) {
        return
      }

      payment.value = null
    },
    submitPayment: async value => {
      if (isPaying.value || Object.keys(validatePaymentForm(value)).length > 0) {
        return
      }

      isPaying.value = true

      try {
        await debts.pay(value.debtId, toPaymentDraft(value))

        if (payment.value?.debtId === value.debtId) {
          payment.value = null
        }
      } catch (error) {
        notice.value = { text: messageOf(error, 'Не удалось оплатить долг'), type: 'error' }
      } finally {
        isPaying.value = false
      }
    },
    isForgiveOpen,
    isForgiving,
    forgiveComment,
    forgiveCategoryId,
    expenseCategories,
    selectedIds: computed(() => selectedIds.value),
    selectedCount: computed(() => selectedIds.value.size),
    openForgive: state => {
      isForgiveOpen.value = state

      if (!state) {
        clearSelection()
      }
    },
    isSelectable,
    toggleSelection: debt => {
      if (!isSelectable(debt)) {
        notice.value = { text: DEBT_NOT_FORGIVABLE, type: 'warning' }
        return
      }

      const next = new Set(selectedIds.value)

      if (!next.delete(debt.id)) {
        next.add(debt.id)
      }

      selectedIds.value = next
    },
    selectAllVisible: () => {
      const next = new Set(selectedIds.value)

      for (const group of groups.value) {
        for (const owner of group.owners) {
          for (const debt of owner.debts.filter(isSelectable)) {
            next.add(debt.id)
          }
        }
      }

      selectedIds.value = next
    },
    clearSelection,
    forgive: async () => {
      if (isForgiving.value) {
        return
      }

      if (selectedIds.value.size === 0) {
        notice.value = { text: DEBT_SELECTION_REQUIRED, type: 'warning' }
        return
      }

      const debtIds = debts.debts.value
        .filter(debt => selectedIds.value.has(debt.id) && canForgiveDebt(debt))
        .map(debt => debt.id)

      if (debtIds.length === 0) {
        notice.value = { text: DEBT_NOT_FORGIVABLE, type: 'warning' }
        return
      }

      const categoryId = forgiveCategoryId.value

      if (categoryId === null) {
        notice.value = { text: DEBT_CATEGORY_REQUIRED, type: 'warning' }
        return
      }

      const category = categories.value.find(candidate => candidate.id === categoryId)

      if (category === undefined || category.operationTypeId !== EXPENSE_OPERATION_TYPE_ID) {
        notice.value = { text: DEBT_CATEGORY_NOT_EXPENSE, type: 'warning' }
        return
      }

      isForgiving.value = true

      try {
        await debts.forgive({ debtIds, categoryId, comment: trimToNull(forgiveComment.value) })
        clearSelection()
        isForgiveOpen.value = false
        notice.value = { text: `Успешно прощено долгов: ${debtIds.length}`, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, 'Не удалось простить долги'), type: 'error' }
      } finally {
        isForgiving.value = false
      }
    },
    isMergeOpen,
    isMerging,
    ownerFromId,
    ownerToId,
    openMerge: state => {
      isMergeOpen.value = state

      if (state) {
        owners.load()
      }
    },
    swapOwners: () => {
      const from = ownerFromId.value

      ownerFromId.value = ownerToId.value
      ownerToId.value = from
    },
    merge: async () => {
      const fromUserId = ownerFromId.value
      const toUserId = ownerToId.value

      if (isMerging.value || fromUserId === null || toUserId === null || fromUserId === toUserId) {
        return
      }

      isMerging.value = true

      try {
        await owners.merge(fromUserId, toUserId)
        ownerFromId.value = null
        ownerToId.value = null
        isMergeOpen.value = false
        notice.value = { text: DEBT_MERGED_NOTICE, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, 'Не удалось объединить держателей'), type: 'error' }
      } finally {
        isMerging.value = false
      }
    },
  }
}

function messageOf(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}
