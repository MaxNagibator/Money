<script setup lang="ts">
import { watch } from 'vue'
import { mdAdd, mdClose, mdComment } from '@/shared/icons'
import {
  MudButton,
  MudDialog,
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
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'

defineProps<{
  canCreate: boolean
  isCreating: boolean
}>()

const emit = defineEmits<{ submit: [] }>()

const isOpen = defineModel<boolean>('isOpen', { required: true })
const name = defineModel<string>('name', { required: true })

const isNarrow = useNarrowViewport()

watch(isNarrow, () => {
  isOpen.value = false
})
</script>

<template>
  <template v-if="isNarrow">
    <MudIconButton
      :icon="mdAdd"
      aria-haspopup="dialog"
      aria-label="Добавить авто"
      data-testid="cars-add-toggle"
      @click="isOpen = true"
    />

    <MudDialog
      v-model:visible="isOpen"
      class="car-add-dialog"
    >
      <template #title>
        <MudText typo="h6">
          Добавление авто
        </MudText>
      </template>

      <template #content>
        <form
          id="cars-add-form"
          @submit.prevent="emit('submit')"
        >
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
        </form>
      </template>

      <template #actions>
        <MudButton
          data-testid="cars-add-cancel"
          @click="isOpen = false"
        >
          Отмена
        </MudButton>

        <MudButton
          :disabled="!canCreate || isCreating"
          button-type="submit"
          color="tertiary"
          data-testid="cars-add-submit"
          form="cars-add-form"
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
      </template>
    </MudDialog>
  </template>

  <MudTooltip
    v-else
    text="Добавить авто"
  >
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

<style>
@media (max-width: 599.98px) {
  .mud-dialog.car-add-dialog {
    width: 100%;
    max-width: none;
    max-height: calc(var(--viewport-height, 100dvh) * 0.85);
    margin: auto 0 var(--keyboard-inset, 0px);
    border-radius: 12px 12px 0 0;
  }
}
</style>
