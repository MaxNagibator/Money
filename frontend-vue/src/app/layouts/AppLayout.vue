<script setup lang="ts">
import { useQueryClient } from '@tanstack/vue-query'
import { computed, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { buildNavigation, isNavGroup } from '@/app/navigation'
import { prefetchRoute } from '@/app/prefetch'
import { DEFAULT_RETURN_URL } from '@/domain/return-url'
import { useCurrentUserName, useSessionStore } from '@/features/auth'
import { clearPinnedOperations, provideOperationWriter } from '@/features/operations'
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
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import { useViewportVariables } from '@/shared/ui/useViewportVariables'
import AppBottomNav from './AppBottomNav.vue'
import AppErrorBoundary from './AppErrorBoundary.vue'
import AppQuickOperationHost from './AppQuickOperationHost.vue'

const route = useRoute()
const router = useRouter()
const session = useSessionStore()
const userName = useCurrentUserName()

const SCROLL_LOCK_CLASS = 'app-drawer-locked'
const BOTTOM_NAV_CLASS = 'app-bottom-nav-shown'

const isDrawerOpen = ref(true)
const isNarrow = useNarrowViewport()
const theme = useThemeSettings()

useViewportVariables()

const isMobileShell = computed(() => isNarrow.value && session.isAuthenticated)

const quickSession = ref(0)

provideOperationWriter(useQueryClient())

watch([isDrawerOpen, isNarrow], ([open, narrow]) => {
  document.body.classList.toggle(SCROLL_LOCK_CLASS, open && narrow)
}, { immediate: true })

watch(isMobileShell, shown => {
  document.body.classList.toggle(BOTTOM_NAV_CLASS, shown)
}, { immediate: true })

watch(() => session.isAuthenticated, authenticated => {
  clearPinnedOperations()

  if (!authenticated) {
    quickSession.value = 0
  }
})

onUnmounted(() => {
  document.body.classList.remove(SCROLL_LOCK_CLASS)
  document.body.classList.remove(BOTTOM_NAV_CLASS)
})

const pageTitle = computed(() => route.meta.title ?? '')
const isPageTitleShown = computed(() => isMobileShell.value && route.meta.ownHeading !== true && pageTitle.value !== '')

const manualLabel = computed(() => theme.isManual.value ? 'Ручной' : 'Авто')
const brightnessLabel = computed(() => theme.isDark.value ? 'Переключить на светлую тему' : 'Переключить на темную тему')

const navigation = computed(() => buildNavigation({
  isAuthenticated: session.isAuthenticated,
  userName: userName.value,
}))

function prefetch(path: string): void {
  prefetchRoute(router, path)
}

function closeMobileDrawer(): void {
  if (isMobileShell.value) {
    isDrawerOpen.value = false
  }
}

function toggleManualMode(): void {
  theme.setMode(theme.isManual.value ? 'system' : theme.effective.value)
}

function openQuickOperation(): void {
  quickSession.value += 1
}

async function signOut(): Promise<void> {
  closeMobileDrawer()

  await session.signOut()
  await router.push(DEFAULT_RETURN_URL)
}
</script>

<template>
  <MudLayout>
    <MudAppBar
      v-if="!isMobileShell"
      class="mud-theme-transparent"
      :elevation="0"
      style="pointer-events: none;"
    >
      <MudStack
        class="mt-3 ml-n4 app-bar-actions"
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
        <MudText
          v-if="isNarrow"
          class="app-bar-title"
          data-testid="app-bar-title"
          typo="h6"
        >
          {{ pageTitle }}
        </MudText>
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
              object-fit="contain"
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
              @click="closeMobileDrawer()"
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
            @click="closeMobileDrawer()"
            @focusin="prefetch(entry.to)"
            @mouseenter="prefetch(entry.to)"
          >
            {{ entry.title }}
          </MudNavLink>
        </template>

        <template v-if="isMobileShell">
          <MudNavLink
            data-testid="nav-theme-switch"
            :icon="mdBrightness4"
            @click="theme.toggleBrightness()"
          >
            {{ theme.isDark.value ? 'Светлая тема' : 'Тёмная тема' }}
          </MudNavLink>

          <MudNavLink
            data-testid="nav-theme-mode-switch"
            :icon="theme.isManual.value ? mdCheck : mdSwapHoriz"
            @click="toggleManualMode()"
          >
            {{ theme.isManual.value ? 'Тема вручную' : 'Тема по системе' }}
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

    <MudMainContent
      :class="['pt-3 px-8', { 'app-mobile-shell': isMobileShell }]"
      role="main"
    >
      <MudContainer
        max-width="lg"
        gutters
      >
        <MudText
          v-if="isPageTitleShown"
          data-testid="app-page-title"
          gutter-bottom
          html-tag="h1"
          typo="h3"
        >
          {{ pageTitle }}
        </MudText>

        <AppErrorBoundary>
          <router-view />
        </AppErrorBoundary>
      </MudContainer>
    </MudMainContent>

    <AppBottomNav
      v-if="isMobileShell"
      @add="openQuickOperation()"
      @more="isDrawerOpen = true"
    />

    <AppQuickOperationHost
      v-if="quickSession > 0"
      :key="quickSession"
    />
  </MudLayout>
</template>

<style scoped>
.app-brand {
  color: inherit;
  text-decoration: none;
}
</style>
