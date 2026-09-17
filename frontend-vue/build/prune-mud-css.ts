import { readdirSync, readFileSync } from 'node:fs'
import { join } from 'node:path'
import type { Plugin } from 'vite'

const UTILITY_CLASS = String.raw`(?:ma|mt|mb|ml|mr|mx|my|ms|me|pa|pt|pb|pl|pr|px|py|ps|pe|gap)-(?:xs-|sm-|md-|lg-|xl-)?n?\d+`

const UTILITY_IN_SOURCE = new RegExp(String.raw`\b${UTILITY_CLASS}\b`, 'g')

const UTILITY_SELECTOR = new RegExp(String.raw`^\.(${UTILITY_CLASS})$`)

const SOURCE_EXTENSIONS = ['.vue', '.ts']

export const PRUNED_PREFIXES: readonly string[] = [
  'mud-avatar',
  'mud-breadcrumb',
  'mud-button-group',
  'mud-carousel',
  'mud-chart',
  'mud-charts',
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

export function collectUtilityClasses(sources: Iterable<string>): Set<string> {
  const used = new Set<string>()

  for (const source of sources) {
    for (const match of source.matchAll(UTILITY_IN_SOURCE)) {
      used.add(match[0])
    }
  }

  return used
}

export function readSourceFiles(root: string): string[] {
  const entries = readdirSync(root, { recursive: true, withFileTypes: true })

  return entries
    .filter(entry => entry.isFile() && SOURCE_EXTENSIONS.some(extension => entry.name.endsWith(extension)))
    .map(entry => readFileSync(join(entry.parentPath, entry.name), 'utf8'))
}

export function pruneMudCss(css: string, usedUtilities: ReadonlySet<string> | null = null): string {
  return pruneBlock(css, usedUtilities)
}

export function mudCssPrunePlugin(): Plugin {
  let usedUtilities: ReadonlySet<string> | null = null
  let root = 'src'

  return {
    name: 'money-prune-mud-css',
    enforce: 'pre',
    configResolved(config) {
      root = join(config.root, 'src')
    },
    buildStart() {
      usedUtilities = collectUtilityClasses(readSourceFiles(root))
    },
    transform(code, id) {
      if (!id.replaceAll('\\', '/').split('?')[0]?.endsWith(VENDOR_CSS_SUFFIX)) {
        return null
      }

      return { code: pruneMudCss(code, usedUtilities), map: null }
    },
  }
}

function pruneBlock(css: string, usedUtilities: ReadonlySet<string> | null): string {
  let result = ''

  for (const rule of splitRules(css)) {
    result += rule.body === null ? rule.prelude : pruneRule(rule.prelude, rule.body, usedUtilities)
  }

  return result
}

function pruneRule(prelude: string, body: string, usedUtilities: ReadonlySet<string> | null): string {
  const name = atRuleName(prelude)

  if (name !== null) {
    if (!NESTING_AT_RULES.has(name)) {
      return `${prelude}{${body}}`
    }

    const pruned = pruneBlock(body, usedUtilities)

    return pruned.trim() === '' ? '' : `${prelude}{${pruned}}`
  }

  const kept = splitSelectors(prelude).filter(selector => !isPruned(selector) && !isUnusedUtility(selector, usedUtilities))

  return kept.length === 0 ? '' : `${kept.join(',')}{${body}}`
}

export function isUnusedUtility(selector: string, usedUtilities: ReadonlySet<string> | null): boolean {
  if (usedUtilities === null) {
    return false
  }

  const match = UTILITY_SELECTOR.exec(selector.trim())

  return match !== null && !usedUtilities.has(match[1] ?? '')
}

function atRuleName(prelude: string): string | null {
  const match = /^\s*@([a-z-]+)/i.exec(prelude)

  return match?.[1]?.toLowerCase() ?? null
}

export function isPruned(selector: string): boolean {
  const classes = [...stripNegations(selector).matchAll(/\.(mud-[a-z0-9-]+)/gi)].map(match => match[1] ?? '')

  return classes.some(name => PRUNED_PREFIXES.some(prefix => isInGroup(name, prefix)))
}

function stripNegations(selector: string): string {
  let result = ''
  let index = 0

  while (index < selector.length) {
    const start = selector.indexOf(':not(', index)

    if (start === -1) {
      return result + selector.slice(index)
    }

    result += selector.slice(index, start)
    index = skipParens(selector, start + ':not('.length - 1)
  }

  return result
}

function skipParens(selector: string, open: number): number {
  let depth = 0

  for (let index = open; index < selector.length; index += 1) {
    const character = selector[index]

    if (character === '(') {
      depth += 1
    }

    if (character === ')') {
      depth -= 1

      if (depth === 0) {
        return index + 1
      }
    }
  }

  return selector.length
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
