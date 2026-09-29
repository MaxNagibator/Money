import { formatByteSize, formatInstant } from '@/platform/format'
import type { InstallState, OfflineSupport, StorageUsage, UpdateCheck } from '@/platform/pwa'

export interface BuildInfo {
  commit: string
  builtAt: string
}

export type CacheClearResult = 'idle' | 'done' | 'failed'

export const OFFLINE_SUPPORT_TEXT: Readonly<Record<OfflineSupport, string>> = {
  disabled: 'Работа без сети включается только в опубликованной версии приложения',
  withdrawn: 'Работа без сети временно выключена, приложение работает только с сетью',
  unsupported: 'Этот браузер не умеет открывать приложение без сети',
  preparing: 'Готовим работу без сети…',
  ready: 'Приложение откроется и без сети и покажет последние загруженные данные',
  failed: 'Работа без сети не включилась. Обновите страницу позже',
}

export const INSTALL_TEXT: Readonly<Record<InstallState, string>> = {
  installed: 'Приложение установлено на это устройство',
  available: 'Приложение можно установить на это устройство и открывать как обычное',
  manual: 'Установить приложение можно из меню браузера: «Установить приложение» или «На экран „Домой“»',
}

const UPDATE_TEXT: Readonly<Record<UpdateCheck, string | null>> = {
  idle: null,
  checking: 'Проверяем обновления…',
  latest: 'Установлена последняя версия',
  downloading: 'Нашлась новая версия, загружаем…',
  offline: 'Нет сети, проверить обновления не получится',
  failed: 'Не удалось проверить обновления. Попробуйте позже',
}

export const NEW_VERSION_TEXT = 'Доступна новая версия'

export const CACHE_CLEAR_TEXT: Readonly<Record<CacheClearResult, string | null>> = {
  idle: null,
  done: 'Сохранённые данные удалены, при следующем открытии они загрузятся заново',
  failed: 'Не удалось удалить сохранённые данные. Попробуйте позже',
}

export function describeBuild(build: BuildInfo): string {
  const builtAt = new Date(build.builtAt)
  const date = Number.isNaN(builtAt.getTime()) ? '' : `от ${formatInstant(builtAt)}`

  return [build.commit.trim(), date].filter(part => part !== '').join(' ')
}

export function describeUpdate(needRefresh: boolean, check: UpdateCheck): string | null {
  return needRefresh ? NEW_VERSION_TEXT : UPDATE_TEXT[check]
}

export function describeNetwork(connected: boolean): string {
  return connected ? 'Связь есть' : 'Нет связи, показаны сохранённые данные'
}

export function describeStorage(usage: StorageUsage | null | undefined): string {
  if (usage === undefined) {
    return 'Считаем занятое место…'
  }

  if (usage === null) {
    return 'Браузер не сообщает, сколько места занято'
  }

  return `Занято ${formatByteSize(usage.usage)} из доступных ${formatByteSize(usage.quota)}`
}
