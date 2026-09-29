import { CacheableResponsePlugin } from 'workbox-cacheable-response'
import { clientsClaim } from 'workbox-core'
import { ExpirationPlugin } from 'workbox-expiration'
import { cleanupOutdatedCaches, createHandlerBoundToURL, precacheAndRoute, type PrecacheEntry } from 'workbox-precaching'
import { NavigationRoute, registerRoute } from 'workbox-routing'
import { NetworkFirst } from 'workbox-strategies'
import {
  API_CACHE_NAME,
  CONFIG_CACHE_NAME,
  EXPIRATION_DATABASE,
  EXPIRATION_RELEASED,
  RELEASE_EXPIRATION_MESSAGE,
} from '../platform/offline-caches'
import { releasableExpiration } from './expiration'
import { offlineCopy } from './offline-copy'

declare const self: ServiceWorkerGlobalScope & { __WB_MANIFEST: (PrecacheEntry | string)[] }

const API_CACHE_ENTRIES = 300
const API_CACHE_AGE_SECONDS = 30 * 24 * 60 * 60
const NETWORK_TIMEOUT_SECONDS = 15

const expiration = releasableExpiration(
  self.indexedDB,
  EXPIRATION_DATABASE,
  () => new ExpirationPlugin({ maxEntries: API_CACHE_ENTRIES, maxAgeSeconds: API_CACHE_AGE_SECONDS }),
)

self.addEventListener('message', event => {
  const type = messageType(event.data)

  if (type === 'SKIP_WAITING') {
    void self.skipWaiting()
  } else if (type === RELEASE_EXPIRATION_MESSAGE) {
    expiration.release()
    event.ports[0]?.postMessage(EXPIRATION_RELEASED)
  }
})

clientsClaim()
precacheAndRoute(self.__WB_MANIFEST)
cleanupOutdatedCaches()

registerRoute(new NavigationRoute(createHandlerBoundToURL('/index.html'), { denylist: [/^\/api\//] }))

registerRoute(
  ({ url, sameOrigin }) => sameOrigin && url.pathname === '/appsettings.json',
  new NetworkFirst({
    cacheName: CONFIG_CACHE_NAME,
    networkTimeoutSeconds: NETWORK_TIMEOUT_SECONDS,
    plugins: [new CacheableResponsePlugin({ statuses: [200] })],
  }),
  'GET',
)

registerRoute(
  ({ request }) => request.method === 'GET' && request.headers.has('Authorization'),
  new NetworkFirst({
    cacheName: API_CACHE_NAME,
    networkTimeoutSeconds: NETWORK_TIMEOUT_SECONDS,
    plugins: [new CacheableResponsePlugin({ statuses: [200] }), expiration.plugin, offlineCopy],
  }),
  'GET',
)

function messageType(data: unknown): unknown {
  return typeof data === 'object' && data !== null && 'type' in data ? data.type : undefined
}
