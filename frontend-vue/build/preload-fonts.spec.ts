import { describe, expect, it } from 'vitest'
import { collectPreloadTags } from './preload-fonts'

describe('preload шрифтов', () => {
  it('берёт начертания первой отрисовки', () => {
    const tags = collectPreloadTags([
      'assets/roboto-latin-400-normal-BqEyEoaF.woff2',
      'assets/roboto-cyrillic-400-normal-CBPI_iaY.woff2',
      'assets/roboto-latin-500-normal-DjUAdKCd.woff2',
      'assets/roboto-cyrillic-500-normal-C_mMyxQe.woff2',
      'assets/roboto-latin-300-normal-CDTVGZEH.woff2',
      'assets/roboto-latin-700-normal-BZpUvMxY.woff2',
      'assets/index-BULuiZQH.js',
    ])

    expect(tags.map(tag => tag.attrs?.href)).toEqual([
      '/assets/roboto-cyrillic-400-normal-CBPI_iaY.woff2',
      '/assets/roboto-cyrillic-500-normal-C_mMyxQe.woff2',
      '/assets/roboto-latin-400-normal-BqEyEoaF.woff2',
      '/assets/roboto-latin-500-normal-DjUAdKCd.woff2',
    ])
  })

  it('отдаёт теги, пригодные для шрифта с другого origin', () => {
    const [tag] = collectPreloadTags(['assets/roboto-latin-400-normal-BqEyEoaF.woff2'])

    expect(tag?.attrs).toMatchObject({ rel: 'preload', as: 'font', type: 'font/woff2', crossorigin: '' })
    expect(tag?.injectTo).toBe('head-prepend')
  })

  it('молчит, когда шрифтов в сборке нет', () => {
    expect(collectPreloadTags(['assets/index-BULuiZQH.js'])).toEqual([])
  })
})
