import type { RouteLocation, RouteRecordRaw } from 'vue-router'
import { ACCESS_DENIED_NOTICE, INVALID_USER_NOTICE, LOCKOUT_NOTICE, LOGIN_NOTICES } from '@/shared/ui/login-notice'

export function toAccountRedirect(name: string) {
  return (to: RouteLocation) => ({
    name,
    query: { ...to.query, ReturnUrl: undefined, returnUrl: to.query.returnUrl ?? to.query.ReturnUrl },
  })
}

const appRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'home',
    meta: { isPublic: true, title: 'Проект «Филочки»' },
    component: () => import('@/pages/HomePage.vue'),
  },
  {
    path: '/operations',
    name: 'operations',
    meta: { title: 'Операции' },
    component: () => import('@/pages/OperationsPage.vue'),
  },
  {
    path: '/statistics',
    name: 'statistics',
    meta: { title: 'Статистика' },
    component: () => import('@/pages/StatisticsPage.vue'),
  },
  {
    path: '/categories',
    name: 'categories',
    meta: { ownHeading: true, title: 'Категории' },
    component: () => import('@/pages/CategoriesPage.vue'),
  },
  {
    path: '/fast-operations',
    name: 'fast-operations',
    meta: { ownHeading: true, title: 'Быстрые операции' },
    component: () => import('@/pages/FastOperationsPage.vue'),
  },
  {
    path: '/regular-operations',
    name: 'regular-operations',
    meta: { ownHeading: true, title: 'Регулярные операции' },
    component: () => import('@/pages/RegularOperationsPage.vue'),
  },
  {
    path: '/debts',
    name: 'debts',
    meta: { ownHeading: true, title: 'Долги' },
    component: () => import('@/pages/DebtsPage.vue'),
  },
  {
    path: '/cars',
    name: 'cars',
    meta: { ownHeading: true, title: 'Машинки' },
    component: () => import('@/pages/CarsPage.vue'),
  },
  {
    path: '/settings',
    name: 'settings',
    meta: { isPublic: true, title: 'Настройки' },
    component: () => import('@/pages/SettingsPage.vue'),
  },
  {
    path: '/profile',
    name: 'profile',
    meta: { title: 'Личный кабинет' },
    component: () => import('@/pages/ProfilePage.vue'),
  },
  {
    path: '/login',
    name: 'login',
    meta: { isPublic: true, ownHeading: true, title: 'Вход' },
    component: () => import('@/pages/LoginPage.vue'),
  },
  {
    path: '/register',
    name: 'register',
    meta: { isPublic: true, ownHeading: true, title: 'Регистрация' },
    component: () => import('@/pages/RegisterPage.vue'),
  },
  {
    path: '/Account/Login',
    redirect: toAccountRedirect('login'),
  },
  {
    path: '/Account/Register',
    redirect: toAccountRedirect('register'),
  },
  {
    path: '/Account/Logout',
    name: 'account-logout',
    meta: { isPublic: true, title: 'Выход' },
    component: () => import('@/pages/AccountLogoutPage.vue'),
  },
  {
    path: '/Account/Lockout',
    name: 'account-lockout',
    meta: { isPublic: true, title: LOGIN_NOTICES[LOCKOUT_NOTICE].title },
    component: () => import('@/pages/AccountNoticePage.vue'),
    props: { notice: LOCKOUT_NOTICE },
  },
  {
    path: '/Account/AccessDenied',
    name: 'account-access-denied',
    meta: { isPublic: true, title: LOGIN_NOTICES[ACCESS_DENIED_NOTICE].title },
    component: () => import('@/pages/AccountNoticePage.vue'),
    props: { notice: ACCESS_DENIED_NOTICE },
  },
  {
    path: '/Account/InvalidUser',
    name: 'account-invalid-user',
    meta: { isPublic: true, title: LOGIN_NOTICES[INVALID_USER_NOTICE].title },
    component: () => import('@/pages/AccountNoticePage.vue'),
    props: { notice: INVALID_USER_NOTICE },
  },
  {
    path: '/Account/Callback',
    name: 'account-callback',
    meta: { isPublic: true, title: 'Вход' },
    component: () => import('@/pages/AccountCallbackPage.vue'),
  },
]

const notFoundRoute: RouteRecordRaw = {
  path: '/:pathMatch(.*)*',
  name: 'not-found',
  meta: { isPublic: true, title: 'Страница не найдена' },
  component: () => import('@/pages/NotFoundPage.vue'),
}

export function buildRoutes(developmentRoutes: RouteRecordRaw[]): RouteRecordRaw[] {
  return [...appRoutes, ...developmentRoutes, notFoundRoute]
}
