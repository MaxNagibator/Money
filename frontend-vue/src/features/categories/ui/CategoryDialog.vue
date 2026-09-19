<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import {
  MudButton,
  MudColorPicker,
  MudDialog,
  MudNumericField,
  MudProgressCircular,
  MudStack,
  MudText,
  MudTextField,
} from '@/shared/mud'
import type { CategoryFormValue } from '../model/category-form'

const props = defineProps<{
  value: CategoryFormValue | null
  title: string
  isBusy: boolean
}>()

const emit = defineEmits<{
  submit: [value: CategoryFormValue]
  cancel: []
}>()

const name = ref('')
const order = ref<number | null>(null)
const color = ref<string | null>(null)
const nameError = ref('')

const dialogVisible = computed({
  get: () => props.value !== null,
  set: (visible: boolean) => {
    if (!visible) {
      emit('cancel')
    }
  },
})

watch(() => props.value, value => {
  if (value === null) {
    return
  }

  name.value = value.name
  order.value = value.order
  color.value = value.color
  nameError.value = ''
}, { immediate: true })

function submit(): void {
  const current = props.value

  if (current === null) {
    return
  }

  if (name.value.trim().length === 0) {
    nameError.value = 'Заполни меня'
    return
  }

  nameError.value = ''
  emit('submit', {
    ...current,
    name: name.value,
    order: order.value,
    color: color.value,
  })
}
</script>

<template>
  <MudDialog
    v-model:visible="dialogVisible"
    :aria-label="title"
    class="category-dialog"
  >
    <template #title>
      <MudText typo="h6">
        {{ title }}
      </MudText>
    </template>

    <template #content>
      <form
        data-testid="category-dialog"
        @submit.prevent="submit"
      >
        <MudStack :spacing="3">
          <MudTextField
            v-model="name"
            clearable
            :error="nameError.length > 0"
            :error-text="nameError"
            data-testid="category-name"
            helper-text="Отображаемое имя"
            label="Наименование"
            required
          />

          <MudNumericField
            v-model="order"
            data-testid="category-order"
            helper-text="Меньше -> выше"
            label="Значение для сортировки"
          />

          <MudStack :spacing="0">
            <MudText typo="subtitle2">
              Цвет
            </MudText>
            <MudColorPicker
              v-model="color"
              color-picker-mode="hex"
              data-testid="category-color"
              picker-variant="static"
              rounded
              show-alpha
              show-color-field
              show-inputs
              show-mode-switch
              show-preview
              show-sliders
              show-toolbar
            />
          </MudStack>
        </MudStack>
      </form>
    </template>

    <template #actions>
      <MudButton
        data-testid="category-cancel"
        @click="emit('cancel')"
      >
        Отмена
      </MudButton>

      <MudButton
        :disabled="isBusy"
        button-type="submit"
        color="success"
        data-testid="category-save"
        @click="submit"
      >
        <MudProgressCircular
          v-if="isBusy"
          class="ms-n1"
          color="inherit"
          indeterminate
          size="small"
        />
        <MudText
          v-else
          typo="button"
        >
          Сохранить
        </MudText>
      </MudButton>
    </template>
  </MudDialog>
</template>

<style>
.mud-dialog.category-dialog {
  max-width: 440px;
}

@media (max-width: 599.98px) {
  .mud-dialog.category-dialog {
    width: 100%;
    max-width: none;
    max-height: calc(var(--viewport-height, 100dvh) * 0.85);
    margin: auto 0 var(--keyboard-inset, 0px);
    border-radius: 12px 12px 0 0;
  }

  .mud-dialog.category-dialog .mud-picker-static {
    min-width: 0;
    max-width: 100%;
  }
}
</style>
