<script setup lang="ts">
import { ref } from 'vue'
import { MudAlert, MudButton, MudGrid, MudItem, MudProgressCircular, MudText, MudTextField } from '@/shared/mud'
import { DEMO_HINT, DEMO_PASSWORD, DEMO_USER_NAME } from '../model/demo-account'
import { useSessionStore } from '../model/session'
import { SIGN_IN_CREDENTIALS_ERROR, SIGN_IN_ERROR, SIGN_IN_FAILURE } from '../model/sign-in-messages'

const emit = defineEmits<{ signedIn: [] }>()

const session = useSessionStore()

const userName = ref('')
const password = ref('')
const isBusy = ref(false)
const errorMessage = ref('')

async function submit(): Promise<void> {
  const login = userName.value.trim()

  if (login.length === 0 || password.value.length === 0) {
    errorMessage.value = SIGN_IN_CREDENTIALS_ERROR
    return
  }

  await signIn(login, password.value)
}

async function signInAsDemo(): Promise<void> {
  await signIn(DEMO_USER_NAME, DEMO_PASSWORD)
}

async function signIn(login: string, secret: string): Promise<void> {
  isBusy.value = true
  errorMessage.value = ''

  try {
    await session.signIn(login, secret)
    emit('signedIn')
  } catch (error) {
    errorMessage.value = `${SIGN_IN_FAILURE} ${error instanceof Error ? error.message : SIGN_IN_ERROR}`
  } finally {
    isBusy.value = false
  }
}
</script>

<template>
  <form
    data-testid="login-form"
    @submit.prevent="submit"
  >
    <MudText
      gutter-bottom
      typo="body1"
    >
      Пожалуйста, введите свои учетные данные
    </MudText>

    <MudGrid>
      <MudItem
        :md="12"
        :xs="12"
      >
        <MudTextField
          v-model="userName"
          autocomplete="username"
          class="login-name-field"
          data-testid="login-name"
          label="Электронная почта или имя пользователя"
          name="username"
          placeholder="name@example.com"
          aria-required="true"
        />
      </MudItem>

      <MudItem
        :md="12"
        :xs="12"
      >
        <MudTextField
          v-model="password"
          autocomplete="current-password"
          data-testid="login-password"
          input-type="password"
          label="Пароль"
          name="password"
          placeholder="пароль"
          aria-required="true"
        />
      </MudItem>

      <MudItem
        v-if="errorMessage.length > 0"
        :md="12"
        :xs="12"
      >
        <MudAlert
          data-testid="login-error"
          severity="error"
        >
          {{ errorMessage }}
        </MudAlert>
      </MudItem>

      <MudItem
        :md="12"
        :xs="12"
      >
        <MudButton
          :disabled="isBusy"
          full-width
          color="primary"
          data-testid="login-submit"
          button-type="submit"
          variant="filled"
        >
          <MudProgressCircular
            v-if="isBusy"
            color="inherit"
            indeterminate
            size="small"
          />
          <template v-else>
            Войти
          </template>
        </MudButton>
      </MudItem>

      <MudItem
        :md="12"
        :xs="12"
      >
        <MudButton
          :disabled="isBusy"
          :title="DEMO_HINT"
          full-width
          color="primary"
          data-testid="login-demo"
          variant="outlined"
          @click="signInAsDemo"
        >
          Войти как демо
        </MudButton>
      </MudItem>
    </MudGrid>
  </form>
</template>

<style scoped>
@media (max-width: 599.98px) {
  .login-name-field :deep(.mud-input-label) {
    font-size: 0.85rem !important;
  }
}
</style>
