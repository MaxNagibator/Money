import type { RouteRecordRaw } from 'vue-router'
import { createRouter, createWebHistory } from 'vue-router'
import { resolveNavigation } from '@/app/route-guard'
import { buildRoutes } from '@/app/routes'
import { useSessionStore } from '@/features/auth'
import { setDocumentTitle } from '@/platform/title'

declare module 'vue-router' {
  interface RouteMeta {
    isPublic?: boolean
    title?: string
  }
}

const developmentRoutes: RouteRecordRaw[] = import.meta.env.DEV
  ? [{
      path: '/sandbox',
      name: 'sandbox',
      meta: { title: 'Песочница' },
      component: () => import('@/pages/SandboxPage.vue'),
    }, {
      path: '/mud',
      name: 'mud',
      meta: { title: 'Витрина MudBlazor', isPublic: true },
      component: () => import('@/pages/MudGalleryPage.vue'),
    }]
  : []

export const router = createRouter({
  history: createWebHistory(),
  routes: buildRoutes(developmentRoutes),
})

router.beforeEach((to, from) => resolveNavigation(to, from, useSessionStore().isAuthenticated))

router.afterEach(to => {
  setDocumentTitle(to.meta.title)
})
