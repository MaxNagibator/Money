import type { Plugin } from 'vite'

export const PRUNED_PREFIXES: readonly string[] = [
  'mud-avatar',
  'mud-breadcrumb',
  'mud-button-group',
  'mud-carousel',
  'mud-chart',
  'mud-data-grid',
  'mud-datagrid',
  'mud-drop-container',
  'mud-drop-item',
  'mud-drop-zone',
  'mud-fab-menu',
  'mud-file-upload',
  'mud-form',
  'mud-highlight',
  'mud-pagination',
  'mud-radio',
  'mud-rating',
  'mud-splitter',
  'mud-step',
  'mud-swipe',
]

const VENDOR_CSS_SUFFIX = 'shared/mud/vendor/MudBlazor.min.css'

const NESTING_AT_RULES = new Set(['media', 'supports', 'layer', 'container', 'scope'])

const BACKSLASH = '\\'

export function pruneMudCss(css: string): string {
  return pruneBlock(css)
}

export function mudCssPrunePlugin(): Plugin {
  return {
    name: 'money-prune-mud-css',
    enforce: 'pre',
    transform(code, id) {
      if (!id.replaceAll('\\', '/').split('?')[0]?.endsWith(VENDOR_CSS_SUFFIX)) {
        return null
      }

      return { code: pruneMudCss(code), map: null }
    },
  }
}

function pruneBlock(css: string): string {
  let result = ''

  for (const rule of splitRules(css)) {
    result += rule.body === null ? rule.prelude : pruneRule(rule.prelude, rule.body)
  }

  return result
}

function pruneRule(prelude: string, body: string): string {
  const name = atRuleName(prelude)

  if (name !== null) {
    if (!NESTING_AT_RULES.has(name)) {
      return `${prelude}{${body}}`
    }

    const pruned = pruneBlock(body)

    return pruned.trim() === '' ? '' : `${prelude}{${pruned}}`
  }

  const kept = splitSelectors(prelude).filter(selector => !isPruned(selector))

  return kept.length === 0 ? '' : `${kept.join(',')}{${body}}`
}

function atRuleName(prelude: string): string | null {
  const match = /^\s*@([a-z-]+)/i.exec(prelude)

  return match?.[1]?.toLowerCase() ?? null
}

export function isPruned(selector: string): boolean {
  if (selector.includes(':not(')) {
    return false
  }

  const classes = [...selector.matchAll(/\.(mud-[a-z0-9-]+)/gi)].map(match => match[1] ?? '')

  return classes.some(name => PRUNED_PREFIXES.some(prefix => isInGroup(name, prefix)))
}

function isInGroup(name: string, prefix: string): boolean {
  return name === prefix || name.startsWith(`${prefix}-`)
}

interface Rule {
  prelude: string
  body: string | null
}

function skipAtomic(css: string, index: number): number {
  const character = css[index]

  if (character === BACKSLASH) {
    return index + 2
  }

  if (character === '/' && css[index + 1] === '*') {
    const end = css.indexOf('*/', index + 2)

    return end === -1 ? css.length : end + 2
  }

  if (character !== '"' && character !== '\'') {
    return -1
  }

  for (let cursor = index + 1; cursor < css.length; cursor += 1) {
    if (css[cursor] === BACKSLASH) {
      cursor += 1
      continue
    }

    if (css[cursor] === character) {
      return cursor + 1
    }
  }

  return css.length
}

function splitRules(css: string): Rule[] {
  const rules: Rule[] = []
  let prelude = ''
  let index = 0

  while (index < css.length) {
    const character = css[index] ?? ''
    const atomic = skipAtomic(css, index)

    if (atomic >= 0) {
      prelude += css.slice(index, atomic)
      index = atomic
      continue
    }

    if (character === '{') {
      const end = matchBrace(css, index)

      rules.push({ prelude, body: css.slice(index + 1, end) })
      prelude = ''
      index = end + 1
      continue
    }

    if (character === ';' && prelude.trim().startsWith('@')) {
      rules.push({ prelude: `${prelude};`, body: null })
      prelude = ''
      index += 1
      continue
    }

    prelude += character
    index += 1
  }

  if (prelude.trim() !== '') {
    rules.push({ prelude, body: null })
  }

  return rules
}

function matchBrace(css: string, start: number): number {
  let depth = 0

  for (let index = start; index < css.length; index += 1) {
    const atomic = skipAtomic(css, index)

    if (atomic >= 0) {
      index = atomic - 1
      continue
    }

    const character = css[index]

    if (character === '{') {
      depth += 1
      continue
    }

    if (character === '}') {
      depth -= 1

      if (depth === 0) {
        return index
      }
    }
  }

  return css.length
}

function splitSelectors(prelude: string): string[] {
  const selectors: string[] = []
  let current = ''
  let depth = 0

  for (let index = 0; index < prelude.length; index += 1) {
    const atomic = skipAtomic(prelude, index)

    if (atomic >= 0) {
      current += prelude.slice(index, atomic)
      index = atomic - 1
      continue
    }

    const character = prelude[index] ?? ''

    if (character === '(' || character === '[') {
      depth += 1
    }

    if (character === ')' || character === ']') {
      depth -= 1
    }

    if (character === ',' && depth === 0) {
      selectors.push(current)
      current = ''
      continue
    }

    current += character
  }

  if (current.trim() !== '') {
    selectors.push(current)
  }

  return selectors
}
