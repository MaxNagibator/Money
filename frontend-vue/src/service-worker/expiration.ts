import type { WorkboxPlugin } from 'workbox-core/types'

export interface ReleasableExpiration {
  plugin: WorkboxPlugin
  release: () => void
}

export function releasableExpiration(factory: IDBFactory, database: string, create: () => WorkboxPlugin): ReleasableExpiration {
  const connections = new Set<IDBDatabase>()
  let current = create()

  const release = (): void => {
    current = create()

    for (const connection of connections) {
      connection.close()
    }

    connections.clear()
  }

  const open = factory.open.bind(factory)

  factory.open = (name: string, version?: number): IDBOpenDBRequest => {
    const request = version === undefined ? open(name) : open(name, version)

    if (name === database) {
      request.addEventListener('success', () => {
        connections.add(request.result)
        request.result.addEventListener('versionchange', release)
      })
    }

    return request
  }

  return {
    plugin: {
      cachedResponseWillBeUsed: param => current.cachedResponseWillBeUsed?.(param) ?? Promise.resolve(param.cachedResponse),
      cacheDidUpdate: param => current.cacheDidUpdate?.(param) ?? Promise.resolve(),
    },
    release,
  }
}
