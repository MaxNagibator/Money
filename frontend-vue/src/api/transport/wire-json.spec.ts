import { describe, expect, it, vi } from 'vitest'
import { rewriteWireJson } from '@/api/transport/wire-json'

describe('rewriteWireJson', () => {
  it.each([
    ['100.00'],
    ['100.5'],
    ['100'],
    ['-0.10'],
  ])('сохраняет литерал суммы %j', (literal) => {
    expect(rewriteWireJson(`{"sum":${literal}}`)).toBe(`{"sum":"${literal}"}`)
  })

  it('сохраняет цифры, которые не выживают в JavaScript-числе', () => {
    const literal = '9007199254740993.55'

    expect(rewriteWireJson(`{"sum":${literal}}`)).toBe(`{"sum":"${literal}"}`)
    expect(JSON.stringify(JSON.parse(`{"sum":${literal}}`))).not.toBe(`{"sum":"${literal}"}`)
  })

  it.each([
    ['sum'],
    ['paySum'],
  ])('разворачивает ключ %j на любой глубине', (key) => {
    expect(rewriteWireJson(`{"items":[{"${key}":1.20},{"${key}":0}]}`))
      .toBe(`{"items":[{"${key}":"1.20"},{"${key}":"0"}]}`)
  })

  it('не трогает строку, в тексте которой есть похожее поле', () => {
    const text = '{"comment":"\\"sum\\": 100.00","sum":1.50}'

    expect(rewriteWireJson(text)).toBe('{"comment":"\\"sum\\": 100.00","sum":"1.50"}')
  })

  it('не трогает сумму, пришедшую строкой', () => {
    expect(rewriteWireJson('{"sum":"100.00"}')).toBeNull()
  })

  it.each([
    ['{"id":1,"place":"Кафе"}'],
    ['[]'],
    ['{"summary":100.00}'],
  ])('оставляет без замены ответ %j', (text) => {
    expect(rewriteWireJson(text)).toBeNull()
  })

  it('оставляет без замены неразобранный ответ', () => {
    expect(rewriteWireJson('<html>Шлюз недоступен</html>')).toBeNull()
  })

  it('без доступа к исходному тексту числа ничего не заменяет', async () => {
    const parseWithoutSource = JSON.parse.bind(JSON) as (text: string) => unknown

    vi.spyOn(JSON, 'parse').mockImplementation((text: string) => parseWithoutSource(text))
    vi.resetModules()

    const { rewriteWireJson: withoutSource } = await import('@/api/transport/wire-json')

    expect(withoutSource('{"sum":100.00}')).toBeNull()
  })
})
