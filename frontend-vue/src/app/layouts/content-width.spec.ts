import { describe, expect, it } from 'vitest'

const layout = import.meta.glob<string>('./AppLayout.vue', { query: '?raw', import: 'default', eager: true })['./AppLayout.vue']

const sources = {
  ...import.meta.glob<string>('../../pages/*.vue', { query: '?raw', import: 'default', eager: true }),
  ...import.meta.glob<string>('./*.vue', { query: '?raw', import: 'default', eager: true }),
}

describe('общая ширина контента', () => {
  it('контейнер оболочки берёт ширину MaxWidth.Large', () => {
    expect(layout).toMatch(/<MudContainer\s+max-width="lg"/)
  })

  it('контейнер ширины ровно один и стоит в оболочке', () => {
    const withContainer = Object.entries(sources)
      .filter(([, source]) => source.includes('<MudContainer'))
      .map(([path]) => path.split('/').at(-1))

    expect(withContainer).toEqual(['AppLayout.vue'])
  })
})
