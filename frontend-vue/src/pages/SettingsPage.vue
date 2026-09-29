<script setup lang="ts">
import { currentVersion, VERSION_HISTORY } from '@/features/home'
import { AppInfoCard, DisplaySettingsCard, ThemeScheduleCard, ThemeSettingsCard } from '@/features/settings'

const version = currentVersion(VERSION_HISTORY)
</script>

<template>
  <ThemeSettingsCard />
  <ThemeScheduleCard />
  <DisplaySettingsCard />
  <AppInfoCard :version="version" />
</template>
