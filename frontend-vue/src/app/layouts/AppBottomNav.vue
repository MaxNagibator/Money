<script setup lang="ts">
import { computed } from 'vue'
import { buildBottomNavigation } from '@/app/navigation'
import { mdAdd, mdMoreHoriz } from '@/shared/icons'
import { MudAppBar, MudFab, MudIcon } from '@/shared/mud'
import { useKeyboardHide } from '@/shared/ui/useKeyboardHide'
import { useScrollHide } from '@/shared/ui/useScrollHide'

const ADD_BUTTON_AFTER = 1

const emit = defineEmits<{
  add: []
  more: []
}>()

const items = buildBottomNavigation()

const isScrollHidden = useScrollHide()
const isKeyboardHidden = useKeyboardHide()

const isHidden = computed(() => isScrollHidden.value || isKeyboardHidden.value)
</script>

<template>
  <MudAppBar
    bottom
    :class="['app-bottom-nav', { 'app-bottom-nav-away': isHidden }]"
    data-testid="bottom-nav"
    :elevation="8"
  >
    <nav
      aria-label="Основные разделы"
      class="app-bottom-nav-items"
    >
      <template
        v-for="(item, index) in items"
        :key="item.to"
      >
        <router-link
          class="app-bottom-nav-link"
          :data-testid="item.testId"
          :to="item.to"
        >
          <MudIcon
            color="inherit"
            :icon="item.icon"
            size="medium"
          />
          <span class="app-bottom-nav-label">{{ item.title }}</span>
        </router-link>

        <MudFab
          v-if="index === ADD_BUTTON_AFTER"
          aria-label="Добавить операцию"
          class="app-bottom-nav-add"
          color="primary"
          data-testid="bottom-nav-add"
          :start-icon="mdAdd"
          @click="emit('add')"
        />
      </template>

      <button
        class="app-bottom-nav-link"
        data-testid="bottom-nav-more"
        type="button"
        @click="emit('more')"
      >
        <MudIcon
          color="inherit"
          :icon="mdMoreHoriz"
          size="medium"
        />
        <span class="app-bottom-nav-label">Ещё</span>
      </button>
    </nav>
  </MudAppBar>
</template>

<style scoped>
@media (max-width: 599.98px) {
  .app-bottom-nav {
    z-index: calc(var(--mud-zindex-drawer) - 1);
    padding-bottom: env(safe-area-inset-bottom);
    background-color: var(--mud-palette-surface);
    color: var(--mud-palette-text-primary);
    transition: transform 0.25s, visibility 0.25s;
  }

  .app-bottom-nav-away {
    transform: translateY(100%);
    visibility: hidden;
  }

  .app-bottom-nav-items {
    display: flex;
    flex: 1 1 auto;
    align-items: center;
    justify-content: space-between;
  }

  .app-bottom-nav-link {
    display: flex;
    flex: 1 1 0;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 2px;
    min-width: 44px;
    min-height: 48px;
    padding: 0;
    border: none;
    background: none;
    color: var(--mud-palette-text-secondary);
    font-family: inherit;
    font-size: 0.6875rem;
    line-height: 1;
    text-decoration: none;
    cursor: pointer;
  }

  .app-bottom-nav-link.router-link-active {
    color: var(--mud-palette-primary);
  }

  :global(html[data-mud-theme="dark"] .app-bottom-nav-link) {
    color: var(--mud-palette-action-default);
  }

  :global(html[data-mud-theme="dark"] .app-bottom-nav-link.router-link-active) {
    color: var(--mud-palette-primary-lighten);
  }

  :global(html[data-mud-theme="dark"] .router-link-active .app-bottom-nav-label) {
    color: var(--mud-palette-text-primary);
  }

  .app-bottom-nav-label {
    max-width: 100%;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
  }

  .app-bottom-nav-add {
    flex: 0 0 auto;
    margin: 0 8px;
    transform: translateY(-12px);
  }
}
</style>
