<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { buildNavigation, isNavGroup } from '@/app/navigation'
import { prefetchRoute } from '@/app/prefetch'
import { DEFAULT_RETURN_URL } from '@/domain/return-url'
import { useCurrentUserName, useSessionStore } from '@/features/auth'
import { useThemeSettings } from '@/features/settings'
import { mdBrightness4, mdCheck, mdLogout, mdMenu, mdSwapHoriz } from '@/shared/icons'
import {
  MudAppBar,
  MudBadge,
  MudContainer,
  MudDrawer,
  MudDrawerHeader,
  MudIconButton,
  MudImage,
  MudLayout,
  MudMainContent,
  MudNavGroup,
  MudNavLink,
  MudNavMenu,
  MudStack,
  MudText,
  MudTooltip,
} from '@/shared/mud'
import AppErrorBoundary from './AppErrorBoundary.vue'

const router = useRouter()
const session = useSessionStore()
const userName = useCurrentUserName()

const isDrawerOpen = ref(true)
const theme = useThemeSettings()
const manualLabel = computed(() => theme.isManual.value ? 'Ручной' : 'Авто')
const brightnessLabel = computed(() => theme.isDark.value ? 'Переключить на светлую тему' : 'Переключить на темную тему')

const navigation = computed(() => buildNavigation({
  isAuthenticated: session.isAuthenticated,
  userName: userName.value,
}))

function prefetch(path: string): void {
  prefetchRoute(router, path)
}

function toggleManualMode(): void {
  theme.setMode(theme.isManual.value ? 'system' : theme.effective.value)
}

async function signOut(): Promise<void> {
  await session.signOut()
  await router.push(DEFAULT_RETURN_URL)
}
</script>

<template>
  <MudLayout>
    <MudAppBar
      class="mud-theme-transparent"
      :elevation="0"
      style="pointer-events: none;"
    >
      <MudStack
        class="mt-3 ml-n4"
        :spacing="1"
      >
        <MudIconButton
          :icon="mdMenu"
          aria-label="Открыть меню"
          color="inherit"
          edge="start"
          size="small"
          style="pointer-events: all;"
          title="Меню"
          @click="isDrawerOpen = !isDrawerOpen"
        />
        <MudTooltip :text="manualLabel">
          <MudBadge
            bordered
            class="cursor-pointer"
            :color="theme.isManual.value ? 'primary' : 'secondary'"
            data-testid="theme-mode-switch"
            :icon="theme.isManual.value ? mdCheck : mdSwapHoriz"
            origin="bottom-right"
            overlap
            @click="toggleManualMode()"
          >
            <MudTooltip :text="theme.isDark.value ? 'Темный' : 'Светлый'">
              <MudIconButton
                :aria-label="brightnessLabel"
                color="inherit"
                data-testid="theme-switch"
                :icon="mdBrightness4"
                size="small"
                style="pointer-events: all;"
                @click="theme.toggleBrightness()"
              />
            </MudTooltip>
          </MudBadge>
        </MudTooltip>
      </MudStack>
    </MudAppBar>

    <MudDrawer
      v-model:open="isDrawerOpen"
      :elevation="2"
    >
      <MudDrawerHeader>
        <router-link
          :to="{ name: 'home' }"
          class="app-brand d-flex align-center"
          data-testid="brand-home"
        >
          <MudStack
            row
            align-items="center"
            justify="space-between"
            :spacing="3"
          >
            <MudImage
              alt=""
              class="cursor-pointer"
              :height="64"
              src="/icon-192.png"
            />
            <MudText
              class="cursor-pointer"
              typo="h5"
            >
              Филочки
            </MudText>
          </MudStack>
        </router-link>
      </MudDrawerHeader>

      <MudNavMenu data-testid="nav-menu">
        <template
          v-for="entry in navigation"
          :key="entry.title"
        >
          <MudNavGroup
            v-if="isNavGroup(entry)"
            :icon="entry.icon"
            :title="entry.title"
            expanded
          >
            <MudNavLink
              v-for="item in entry.items"
              :key="item.to"
              :data-testid="item.testId"
              :href="item.to"
              :icon="item.icon"
              @focusin="prefetch(item.to)"
              @mouseenter="prefetch(item.to)"
            >
              {{ item.title }}
            </MudNavLink>
          </MudNavGroup>

          <MudNavLink
            v-else
            :data-testid="entry.testId"
            :href="entry.to"
            :icon="entry.icon"
            @focusin="prefetch(entry.to)"
            @mouseenter="prefetch(entry.to)"
          >
            {{ entry.title }}
          </MudNavLink>
        </template>

        <template v-if="session.isAuthenticated">
          <MudNavLink
            :icon="mdLogout"
            data-testid="nav-sign-out"
            icon-color="info"
            @click="signOut"
          >
            Выйти
          </MudNavLink>
        </template>
      </MudNavMenu>
    </MudDrawer>

    <MudMainContent class="pt-3 px-8">
      <MudContainer
        max-width="lg"
        gutters
      >
        <AppErrorBoundary>
          <router-view />
        </AppErrorBoundary>
      </MudContainer>
    </MudMainContent>
  </MudLayout>
</template>

<style scoped>
.app-brand {
  color: inherit;
  text-decoration: none;
}
</style>
