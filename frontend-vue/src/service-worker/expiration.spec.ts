import { describe, expect, it, vi } from 'vitest'
import type { WorkboxPlugin } from 'workbox-core/types'
import { releasableExpiration } from './expiration'

interface FakeConnection extends EventTarget {
  name: string
  close: ReturnType<typeof vi.fn>
}

function fakeFactory(): { factory: IDBFactory, open: ReturnType<typeof vi.fn>, connections: FakeConnection[] } {
  const connections: FakeConnection[] = []
  const open = vi.fn((name: string) => {
    const connection = Object.assign(new EventTarget(), { name, close: vi.fn() })
    const request = Object.assign(new EventTarget(), { result: connection })

    connections.push(connection)
    queueMicrotask(() => request.dispatchEvent(new Event('success')))

    return request
  })

  return { factory: { open } as unknown as IDBFactory, open, connections }
}

function fakePlugins(): { create: () => WorkboxPlugin, created: WorkboxPlugin[] } {
  const created: WorkboxPlugin[] = []

  return {
    created,
    create: () => {
      const plugin: WorkboxPlugin = {
        cacheDidUpdate: vi.fn(() => Promise.resolve()),
        cachedResponseWillBeUsed: vi.fn(() => Promise.resolve(null)),
      }

      created.push(plugin)

      return plugin
    },
  }
}

async function openBoth(factory: IDBFactory): Promise<void> {
  factory.open('workbox-expiration', 1)
  factory.open('money-other')
  await Promise.resolve()
}

describe('соединение воркера с базой отметок', () => {
  it('открытие проходит в браузер с теми же аргументами', () => {
    const { factory, open } = fakeFactory()

    releasableExpiration(factory, 'workbox-expiration', fakePlugins().create)
    factory.open('workbox-expiration', 1)
    factory.open('money-other')

    expect(open.mock.calls).toEqual([['workbox-expiration', 1], ['money-other']])
  })

  it.each<[string, (release: () => void, connection: FakeConnection) => void]>([
    ['сообщение страницы', release => {
      release()
    }],
    ['удаление базы со страницы', (_release, connection) => {
      connection.dispatchEvent(new Event('versionchange'))
    }],
  ])('%s закрывает соединения с базой отметок и не трогает чужие', async (_trigger, trigger) => {
    const { factory, connections } = fakeFactory()
    const expiration = releasableExpiration(factory, 'workbox-expiration', fakePlugins().create)

    await openBoth(factory)

    const [own, foreign] = connections as [FakeConnection, FakeConnection]

    trigger(expiration.release, own)

    expect(own.close).toHaveBeenCalledTimes(1)
    expect(foreign.close).not.toHaveBeenCalled()
  })

  it('после освобождения отметки ведёт новый плагин, а не тот, что держал закрытое соединение', async () => {
    const { factory } = fakeFactory()
    const { create, created } = fakePlugins()
    const expiration = releasableExpiration(factory, 'workbox-expiration', create)
    const param = {} as Parameters<NonNullable<WorkboxPlugin['cacheDidUpdate']>>[0]

    await expiration.plugin.cacheDidUpdate?.(param)
    expiration.release()
    await expiration.plugin.cacheDidUpdate?.(param)

    const [before, after] = created as [WorkboxPlugin, WorkboxPlugin]

    expect(created).toHaveLength(2)
    expect(before.cacheDidUpdate).toHaveBeenCalledTimes(1)
    expect(after.cacheDidUpdate).toHaveBeenCalledTimes(1)
  })

  it('соединение, открытое после освобождения, тоже закрывается следующим', async () => {
    const { factory, connections } = fakeFactory()
    const expiration = releasableExpiration(factory, 'workbox-expiration', fakePlugins().create)

    await openBoth(factory)
    expiration.release()
    factory.open('workbox-expiration', 1)
    await Promise.resolve()
    connections[2]?.dispatchEvent(new Event('versionchange'))

    expect(connections[0]?.close).toHaveBeenCalledTimes(1)
    expect(connections[2]?.close).toHaveBeenCalledTimes(1)
  })
})
