import type { HtmlTagDescriptor, Plugin } from 'vite'

const PRELOADED_FONT = /-(?:400|500)-normal-[\w-]+\.woff2$/

export function collectPreloadTags(fileNames: Iterable<string>): HtmlTagDescriptor[] {
  return [...fileNames]
    .filter(fileName => PRELOADED_FONT.test(fileName))
    .sort()
    .map(fileName => ({
      tag: 'link',
      injectTo: 'head-prepend',
      attrs: {
        rel: 'preload',
        as: 'font',
        type: 'font/woff2',
        href: `/${fileName}`,
        crossorigin: '',
      },
    }))
}

export function preloadFontsPlugin(): Plugin {
  return {
    name: 'money-preload-fonts',
    apply: 'build',
    transformIndexHtml: {
      order: 'post',
      handler(_html, context) {
        return collectPreloadTags(Object.keys(context.bundle ?? {}))
      },
    },
  }
}
