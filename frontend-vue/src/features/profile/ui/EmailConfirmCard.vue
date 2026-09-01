<script setup lang="ts">
import { mdMarkEmailUnread, mdRefreshOutlined, mdVerifiedUserOutlined } from '@/shared/icons'
import { MudButton, MudCard, MudCardContent, MudCardHeader, MudIcon, MudProgressCircular, MudSpacer, MudStack, MudText, MudTextField } from '@/shared/mud'
import type { ConfirmFormErrors } from '../model/confirm-form'

defineProps<{
  errors: ConfirmFormErrors
  isConfirming: boolean
  isResending: boolean
  canResend: boolean
  resendLabel: string
}>()

const emit = defineEmits<{
  submit: []
  resend: []
}>()

const code = defineModel<string>('code', { default: '' })
</script>

<template>
  <MudCard
    class="mb-6"
    data-testid="profile-confirm-card"
  >
    <MudCardHeader>
      <template #card-header-avatar>
        <MudIcon
          :icon="mdMarkEmailUnread"
          size="medium"
        />
      </template>
      <template #card-header-content>
        <MudText typo="h6">
          Подтвердите почту
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent>
      <form
        data-testid="profile-confirm-form"
        novalidate
        @submit.prevent="emit('submit')"
      >
        <MudTextField
          v-model="code"
          class="mb-2"
          :error="errors.code !== undefined"
          :error-text="errors.code ?? ''"
          autocomplete="one-time-code"
          data-testid="profile-confirm-code"
          input-mode="numeric"
          label="Код подтверждения"
          required
          variant="outlined"
        />

        <MudStack
          row
          align-items="center"
        >
          <MudButton
            :disabled="!canResend"
            :end-icon="isResending ? undefined : mdRefreshOutlined"
            color="secondary"
            data-testid="profile-resend-code"
            variant="outlined"
            @click="emit('resend')"
          >
            <MudProgressCircular
              v-if="isResending"
              color="inherit"
              indeterminate
              size="small"
            />
            <template v-else>
              {{ resendLabel }}
            </template>
          </MudButton>

          <MudSpacer />

          <MudButton
            :disabled="isConfirming"
            :end-icon="isConfirming ? undefined : mdVerifiedUserOutlined"
            color="primary"
            data-testid="profile-confirm-submit"
            button-type="submit"
            variant="filled"
          >
            <MudProgressCircular
              v-if="isConfirming"
              color="inherit"
              indeterminate
              size="small"
            />
            <template v-else>
              Подтвердить
            </template>
          </MudButton>
        </MudStack>
      </form>
    </MudCardContent>
  </MudCard>
</template>
