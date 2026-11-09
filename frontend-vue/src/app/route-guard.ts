import type { RouteLocationRaw, RouteMeta } from 'vue-router'
import { START_LOCATION } from 'vue-router'
import { readSafeReturnUrl, SIGNED_IN_RETURN_URL } from '@/domain/return-url'
import type { SessionChange } from '@/domain/session'

export interface GuardTarget {
  fullPath: string
  meta: RouteMeta
}

export interface GuardSource {
  fullPath: string
}

export function resolveNavigation(to: GuardTarget, from: GuardSource, isAuthenticated: boolean): RouteLocationRaw | true {
  if (!isAuthenticated) {
    return to.meta.isPublic === true ? true : { name: 'login', query: { returnUrl: to.fullPath } }
  }

  return to.fullPath === '/' && from === START_LOCATION ? SIGNED_IN_RETURN_URL : true
}

export function resolveSessionExit(change: SessionChange, current: GuardTarget): RouteLocationRaw | null {
  if (change !== 'expired' || current.meta.isPublic === true) {
    return null
  }

  return { name: 'login', query: { returnUrl: readSafeReturnUrl(current.fullPath) } }
}
