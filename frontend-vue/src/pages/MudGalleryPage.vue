<script setup lang="ts">
import { ref } from 'vue'
import { mdCheck, mdDelete, mdEdit } from '@/shared/icons'
import MudDialog from '@/shared/mud/MudDialog.vue'
import MudNumericField from '@/shared/mud/MudNumericField.vue'
import MudTextField from '@/shared/mud/MudTextField.vue'
import MudSnackbar from '@/shared/mud/MudSnackbar.vue'
import {
  MudAlert,
  MudButton,
  MudCard,
  MudCardContent,
  MudChip,
  MudIconButton,
  MudPaper,
  MudPopover,
  MudStack,
  MudText,
  MudTooltip,
} from '@/shared/mud'

const popoverOpen = ref(false)
const dialogOpen = ref(false)
const noticeOpen = ref(false)
const text = ref('')
const amount = ref<number | null>(null)
</script>

<template>
  <MudStack
    :spacing="4"
    class="pa-4"
  >
    <MudText typo="h4">
      Витрина компонентов
    </MudText>

    <MudPaper class="pa-4">
      <MudStack
        row
        :spacing="2"
      >
        <MudTooltip text="Копировать">
          <MudIconButton :icon="mdCheck" />
        </MudTooltip>
        <MudTooltip
          text="Изменить"
          placement="top"
          color="primary"
        >
          <MudIconButton
            :icon="mdEdit"
            color="primary"
          />
        </MudTooltip>
        <MudTooltip
          text="Удалить"
          placement="right"
          color="error"
        >
          <MudIconButton
            :icon="mdDelete"
            color="error"
          />
        </MudTooltip>
      </MudStack>
    </MudPaper>

    <MudPaper class="pa-4">
      <MudButton
        variant="filled"
        color="primary"
        @click="popoverOpen = !popoverOpen"
      >
        Поповер
      </MudButton>
      <MudPopover
        :open="popoverOpen"
        anchor-origin="bottom-left"
        transform-origin="top-left"
        class="pa-4"
      >
        <MudText>Содержимое поповера</MudText>
      </MudPopover>
    </MudPaper>

    <MudStack
      row
      :spacing="2"
    >
      <MudChip color="primary">
        Метка
      </MudChip>
      <MudChip
        color="success"
        variant="outlined"
      >
        Успех
      </MudChip>
    </MudStack>

    <MudAlert severity="info">
      Сообщение
    </MudAlert>

    <MudPaper class="pa-4">
      <MudStack :spacing="4">
        <MudTextField
          v-model="text"
          label="Название"
          placeholder="Введите название"
          helper-text="Подсказка под полем"
          data-testid="gallery-text"
          clearable
        />
        <MudTextField
          v-model="text"
          label="С ошибкой"
          variant="outlined"
          error
          error-text="Поле обязательно"
        />
        <MudNumericField
          v-model="amount"
          label="Сумма"
          variant="filled"
          :min="0"
          :max="1000"
        />
      </MudStack>
    </MudPaper>

    <MudStack
      row
      :spacing="2"
    >
      <MudButton
        variant="filled"
        color="secondary"
        @click="dialogOpen = true"
      >
        Диалог
      </MudButton>
      <MudButton
        variant="filled"
        color="info"
        @click="noticeOpen = true"
      >
        Уведомление
      </MudButton>
    </MudStack>

    <MudDialog v-model:visible="dialogOpen">
      <template #title>
        <MudText typo="h6">
          Заголовок диалога
        </MudText>
      </template>
      <template #content>
        <MudText>Содержимое диалога</MudText>
      </template>
      <template #actions>
        <MudButton @click="dialogOpen = false">
          Отмена
        </MudButton>
        <MudButton
          variant="filled"
          color="primary"
          @click="dialogOpen = false"
        >
          Сохранить
        </MudButton>
      </template>
    </MudDialog>

    <MudSnackbar
      v-model="noticeOpen"
      severity="success"
      message="Запись сохранена"
    />

    <MudCard>
      <MudCardContent>
        <MudText typo="body1">
          Карточка
        </MudText>
      </MudCardContent>
    </MudCard>
  </MudStack>
</template>
