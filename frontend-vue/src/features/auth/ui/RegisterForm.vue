<script setup lang="ts">
import { computed } from 'vue'
import { mdVisibility, mdVisibilityOff } from '@/shared/icons'
import {
  MudAlert,
  MudButton,
  MudGrid,
  MudItem,
  MudProgressCircular,
  MudText,
  MudTextField,
} from '@/shared/mud'
import { useRegisterScreen } from '../model/useRegisterScreen'

const emit = defineEmits<{ registered: [] }>()

const screen = useRegisterScreen()

const VISIBILITY_LABEL = 'Показать или скрыть пароль'

const isPasswordVisible = computed(() => !screen.form.value.useConfirmPassword)
const passwordType = computed(() => isPasswordVisible.value ? 'text' : 'password')
const visibilityIcon = computed(() => isPasswordVisible.value ? mdVisibility : mdVisibilityOff)

async function submit(): Promise<void> {
  if (await screen.submit()) {
    emit('registered')
  }
}

function toggleVisibility(): void {
  screen.form.value.useConfirmPassword = !screen.form.value.useConfirmPassword
}
</script>

<template>
  <form
    data-testid="register-form"
    @submit.prevent="submit"
  >
    <MudText
      gutter-bottom
      typo="body1"
    >
      Пожалуйста, заполните следующие поля:
    </MudText>

    <MudGrid>
      <MudItem
        :md="12"
        :xs="12"
      >
        <MudTextField
          v-model="screen.form.value.userName"
          :error="screen.errors.value.userName !== undefined"
          :error-text="screen.errors.value.userName ?? ''"
          aria-required="true"
          autocomplete="username"
          data-testid="register-name"
          label="Логин"
          name="username"
          placeholder="username"
        />
      </MudItem>

      <MudItem
        :md="12"
        :xs="12"
      >
        <MudTextField
          v-model="screen.form.value.email"
          :error="screen.errors.value.email !== undefined"
          :error-text="screen.errors.value.email ?? ''"
          autocomplete="email"
          data-testid="register-email"
          helper-text="Необязательна, но вдруг забудете пароль"
          label="Электронная почта"
          name="email"
          placeholder="name@example.com"
        />
      </MudItem>

      <MudItem
        :md="12"
        :xs="12"
      >
        <MudTextField
          v-model="screen.form.value.password"
          :error="screen.errors.value.password !== undefined"
          :error-text="screen.errors.value.password ?? ''"
          :input-type="passwordType"
          adornment="end"
          :adornment-icon="visibilityIcon"
          :adornment-aria-label="VISIBILITY_LABEL"
          adornment-test-id="register-password-visibility"
          :on-adornment-click="toggleVisibility"
          aria-required="true"
          autocomplete="new-password"
          data-testid="register-password"
          label="Пароль"
          name="new-password"
          placeholder="пароль"
        />
      </MudItem>

      <MudItem
        v-if="screen.form.value.useConfirmPassword"
        :md="12"
        :xs="12"
      >
        <MudTextField
          v-model="screen.form.value.confirmPassword"
          :error="screen.errors.value.confirmPassword !== undefined"
          :error-text="screen.errors.value.confirmPassword ?? ''"
          aria-required="true"
          autocomplete="new-password"
          data-testid="register-confirm-password"
          label="Подтвердите пароль"
          name="confirm-password"
          placeholder="подтвердите пароль"
          input-type="password"
        />
      </MudItem>

      <MudItem
        v-if="screen.notice.value !== null"
        :md="12"
        :xs="12"
      >
        <MudAlert
          :severity="screen.notice.value.type"
          data-testid="register-error"
        >
          {{ screen.notice.value.text }}
        </MudAlert>
      </MudItem>

      <MudItem
        :md="12"
        :xs="12"
      >
        <MudButton
          :disabled="screen.isBusy.value"
          full-width
          color="primary"
          data-testid="register-submit"
          button-type="submit"
          variant="filled"
        >
          <MudProgressCircular
            v-if="screen.isBusy.value"
            color="inherit"
            indeterminate
            size="small"
          />
          <template v-else>
            Зарегистрироваться
          </template>
        </MudButton>
      </MudItem>
    </MudGrid>
  </form>
</template>
