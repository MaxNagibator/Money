const MONEY_KEYS: ReadonlySet<string> = new Set(['sum', 'paySum'])

interface SourceContext {
  source?: string
}

type SourceReviver = (key: string, value: unknown, context?: SourceContext) => unknown

const parseWithSource = JSON.parse as unknown as (text: string, reviver: SourceReviver) => unknown

function detectSourceAccess(): boolean {
  let available = false

  parseWithSource('0', (_key, value, context) => {
    available = typeof context?.source === 'string'

    return value
  })

  return available
}

// TODO: без доступа к исходному тексту числа (`context.source` есть только в V8) масштаб копеек и точность свыше 2^53 теряются в промежуточном number – снять, когда доступ появится во всех целевых браузерах либо API начнёт отдавать сумму строкой
const SOURCE_ACCESS = detectSourceAccess()

export function rewriteWireJson(text: string): string | null {
  if (!SOURCE_ACCESS) {
    return null
  }

  let rewritten = false
  let parsed: unknown

  try {
    parsed = parseWithSource(text, (key, value, context) => {
      const source = context?.source

      if (typeof value !== 'number' || source === undefined || !MONEY_KEYS.has(key)) {
        return value
      }

      rewritten = true

      return source
    })
  } catch {
    return null
  }

  return rewritten ? JSON.stringify(parsed) : null
}
