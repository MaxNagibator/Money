<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { AUTH_PROVIDER, GITHUB_PROVIDER, LoginForm, readSignedInReturnUrl, startExternalLogin } from '@/features/auth'
import { mdGitHub, mdLogin } from '@/shared/icons'
import { MudButton, MudDivider, MudGrid, MudItem, MudLink, MudStack, MudText } from '@/shared/mud'

const route = useRoute()
const router = useRouter()

const registerQuery = computed(() => typeof route.query.returnUrl === 'string'
  ? { returnUrl: route.query.returnUrl }
  : {})

function signInWith(provider: string): void {
  void startExternalLogin(provider)
}

async function goBackToRequestedPage(): Promise<void> {
  await router.replace(readSignedInReturnUrl(route.query.returnUrl))
}
</script>

<template>
  <MudGrid justify="center">
    <MudItem
      :md="12"
      :xs="12"
    >
      <MudText
        align="center"
        gutter-bottom
        typo="h3"
      >
        Войти в систему
      </MudText>
    </MudItem>

    <MudItem
      :md="6"
      :xs="12"
    >
      <LoginForm @signed-in="goBackToRequestedPage" />

      <MudDivider class="my-4" />

      <MudStack :spacing="2">
        <MudText typo="h6">
          Вход через внешние сервисы
        </MudText>
        <MudText
          color="info"
          typo="caption"
        >
          Используйте один из провайдеров ниже. Для входа требуется доступ к вашему email.
        </MudText>

        <MudStack align-items="center">
          <MudButton
            :start-icon="mdLogin"
            color="secondary"
            data-testid="login-external-auth"
            full-width
            variant="outlined"
            @click="signInWith(AUTH_PROVIDER)"
          >
            Войти через bob217.Auth
          </MudButton>

          <MudButton
            :start-icon="mdGitHub"
            color="secondary"
            data-testid="login-external-github"
            full-width
            variant="outlined"
            @click="signInWith(GITHUB_PROVIDER)"
          >
            Войти через GitHub
          </MudButton>
        </MudStack>
      </MudStack>

      <MudDivider class="my-4" />

      <MudGrid>
        <MudItem
          :md="12"
          :xs="12"
        >
          <MudLink
            :to="{ name: 'register', query: registerQuery }"
            data-testid="login-to-register"
          >
            Нет аккаунта? Зарегистрируйтесь
          </MudLink>
        </MudItem>
      </MudGrid>
    </MudItem>
  </MudGrid>
</template>
