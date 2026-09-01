<script setup lang="ts">
import { computed } from 'vue'
import { EmailConfirmCard, PasswordChangeCard, ProfileGreetingCard, useProfileScreen } from '@/features/profile'
import { MudAlert, MudProgressCircular, MudSnackbar, MudText } from '@/shared/mud'

const screen = useProfileScreen()

const noticeVisible = computed({
  get: () => screen.notice.value !== null,
  set: (value: boolean) => {
    if (!value) {
      screen.notice.value = null
    }
  },
})

const noticeMessage = computed(() => screen.notice.value?.text ?? '')
const noticeSeverity = computed(() => screen.notice.value?.type ?? 'normal')
</script>

<template>
  <div
    v-if="screen.profile.isLoading.value"
    class="d-flex align-center gap-3"
  >
    <MudProgressCircular indeterminate />
    <MudText html-tag="span">
      Загрузка..
    </MudText>
  </div>

  <MudAlert
    v-else-if="screen.profile.loadError.value !== null"
    data-testid="profile-error"
    severity="error"
    variant="filled"
  >
    {{ screen.profile.loadError.value }}
  </MudAlert>

  <template v-else>
    <ProfileGreetingCard
      :email="screen.email.value"
      :is-email-confirmed="screen.isEmailConfirmed.value"
      :name="screen.name.value"
    />

    <EmailConfirmCard
      v-if="!screen.isEmailConfirmed.value"
      v-model:code="screen.confirmForm.value.code"
      :can-resend="screen.canResend.value"
      :errors="screen.confirmErrors.value"
      :is-confirming="screen.isConfirming.value"
      :is-resending="screen.isResending.value"
      :resend-label="screen.resendLabel.value"
      @resend="screen.resendCode"
      @submit="screen.submitConfirm"
    />

    <PasswordChangeCard
      v-model:current-password="screen.passwordForm.value.currentPassword"
      v-model:is-visible="screen.isPasswordVisible.value"
      v-model:new-password="screen.passwordForm.value.newPassword"
      :errors="screen.passwordErrors.value"
      :is-changing-password="screen.isChangingPassword.value"
      @submit="screen.submitPassword"
    />
  </template>

  <MudSnackbar
    v-model="noticeVisible"
    data-testid="profile-notice"
    :message="noticeMessage"
    :severity="noticeSeverity"
  />
</template>
