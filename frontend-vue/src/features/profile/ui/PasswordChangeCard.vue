<script setup lang="ts">
import { computed } from 'vue'
import { mdChangeCircle, mdChangeCircleOutlined, mdVisibility, mdVisibilityOff } from '@/shared/icons'
import { MudButton, MudCard, MudCardContent, MudCardHeader, MudIcon, MudProgressCircular, MudStack, MudText, MudTextField } from '@/shared/mud'
import type { PasswordFormErrors } from '../model/password-form'

defineProps<{
  errors: PasswordFormErrors
  isChangingPassword: boolean
}>()

const emit = defineEmits<{ submit: [] }>()

const currentPassword = defineModel<string>('currentPassword', { default: '' })
const newPassword = defineModel<string>('newPassword', { default: '' })
const isVisible = defineModel<boolean>('isVisible', { default: false })

const VISIBILITY_LABEL = 'Показать или скрыть пароль'

const fieldType = computed(() => isVisible.value ? 'text' : 'password')
const visibilityIcon = computed(() => isVisible.value ? mdVisibility : mdVisibilityOff)

function toggleVisibility(): void {
  isVisible.value = !isVisible.value
}
</script>

<template>
  <MudCard
    class="mb-6"
    data-testid="profile-password-card"
  >
    <MudCardHeader>
      <template #card-header-avatar>
        <MudIcon
          :icon="mdChangeCircle"
          size="medium"
        />
      </template>
      <template #card-header-content>
        <MudText typo="h6">
          Смена пароля
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent>
      <form
        data-testid="profile-password-form"
        novalidate
        @submit.prevent="emit('submit')"
      >
        <MudTextField
          v-model="currentPassword"
          class="mb-2"
          :error="errors.currentPassword !== undefined"
          :error-text="errors.currentPassword ?? ''"
          :input-type="fieldType"
          adornment="end"
          :adornment-icon="visibilityIcon"
          :adornment-aria-label="VISIBILITY_LABEL"
          adornment-test-id="profile-password-visibility"
          autocomplete="current-password"
          data-testid="profile-current-password"
          label="Текущий пароль"
          :on-adornment-click="toggleVisibility"
          required
          variant="outlined"
        />

        <MudTextField
          v-model="newPassword"
          class="mb-2"
          :error="errors.newPassword !== undefined"
          :error-text="errors.newPassword ?? ''"
          :input-type="fieldType"
          adornment="end"
          :adornment-icon="visibilityIcon"
          :adornment-aria-label="VISIBILITY_LABEL"
          adornment-test-id="profile-new-password-visibility"
          autocomplete="new-password"
          data-testid="profile-new-password"
          label="Новый пароль"
          :on-adornment-click="toggleVisibility"
          required
          variant="outlined"
        />

        <MudStack
          row
          align-items="center"
        >
          <MudButton
            :disabled="isChangingPassword"
            :end-icon="isChangingPassword ? undefined : mdChangeCircleOutlined"
            color="primary"
            data-testid="profile-password-submit"
            button-type="submit"
            variant="filled"
          >
            <MudProgressCircular
              v-if="isChangingPassword"
              color="inherit"
              indeterminate
              size="small"
            />
            <template v-else>
              Сменить
            </template>
          </MudButton>
        </MudStack>
      </form>
    </MudCardContent>
  </MudCard>
</template>
