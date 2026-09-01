<script setup lang="ts">
import { mdAdd, mdClose, mdComment } from '@/shared/icons'
import {
  MudButton,
  MudIconButton,
  MudMenu,
  MudPaper,
  MudProgressCircular,
  MudSpacer,
  MudStack,
  MudText,
  MudTextField,
  MudTooltip,
} from '@/shared/mud'

defineProps<{
  canCreate: boolean
  isCreating: boolean
}>()

const emit = defineEmits<{ submit: [] }>()

const isOpen = defineModel<boolean>('isOpen', { required: true })
const name = defineModel<string>('name', { required: true })
</script>

<template>
  <MudTooltip text="Добавить авто">
    <MudMenu
      v-model:open="isOpen"
      :max-height="400"
    >
      <template #activator="{ toggle }">
        <MudIconButton
          :aria-expanded="isOpen"
          :icon="isOpen ? mdClose : mdAdd"
          aria-label="Добавить авто"
          data-testid="cars-add-toggle"
          @click="toggle"
        />
      </template>

      <MudPaper
        class="pa-3"
        style="min-width: 320px;"
      >
        <form @submit.prevent="emit('submit')">
          <MudStack :spacing="2">
            <MudText>Добавление авто</MudText>

            <MudTextField
              v-model="name"
              :adornment-icon="mdComment"
              adornment="start"
              adornment-color="tertiary"
              clearable
              data-testid="cars-add-name"
              immediate
              label="Наименование"
              placeholder="Моя машинка"
              sizing="auto"
            />

            <MudStack
              row
              align-items="center"
              class="mt-3"
            >
              <MudIconButton
                :icon="mdClose"
                aria-label="Отмена"
                color="error"
                data-testid="cars-add-cancel"
                @click="isOpen = false"
              />

              <MudSpacer />

              <MudButton
                :disabled="!canCreate || isCreating"
                button-type="submit"
                color="tertiary"
                data-testid="cars-add-submit"
                variant="filled"
              >
                <MudProgressCircular
                  v-if="isCreating"
                  color="inherit"
                  indeterminate
                  size="small"
                />
                <template v-else>
                  Создать
                </template>
              </MudButton>
            </MudStack>
          </MudStack>
        </form>
      </MudPaper>
    </MudMenu>
  </MudTooltip>
</template>
