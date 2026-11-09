import type { RouteComponent, Router } from 'vue-router'

type LazyRouteComponent = () => Promise<RouteComponent>

const started = new Set<string>()

export function prefetchRoute(router: Router, path: string): void {
  if (started.has(path)) {
    return
  }

  started.add(path)

  const loaders = router.resolve(path).matched
    .map(record => record.components?.default)
    .filter((component): component is LazyRouteComponent => typeof component === 'function')

  if (loaders.length === 0) {
    return
  }

  void Promise.all(loaders.map(load => load())).catch(() => started.delete(path))
}

export function resetPrefetch(): void {
  started.clear()
}
