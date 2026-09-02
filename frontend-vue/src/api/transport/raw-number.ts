const FORM_CONTENT_TYPE = 'application/x-www-form-urlencoded'
const RAW_LITERAL = Symbol('rawNumber')
const LITERAL_PATTERN = /^-?(?:0|[1-9]\d*)(?:\.\d+)?$/
const PLACEHOLDER_PREFIX = 'raw-number:'

interface RawNumber {
  [RAW_LITERAL]: string
}

function isRawNumber(value: unknown): value is RawNumber {
  return typeof value === 'object' && value !== null && RAW_LITERAL in value
}

function substitute(json: string, placeholder: string, literal: string): string {
  const quoted = `"${placeholder}"`
  const at = json.indexOf(quoted)

  if (at !== json.lastIndexOf(quoted)) {
    throw new Error('Подстановка числа совпала с содержимым запроса')
  }

  return json.slice(0, at) + literal + json.slice(at + quoted.length)
}

export function rawNumber(literal: string): number {
  if (!LITERAL_PATTERN.test(literal)) {
    throw new RangeError('Сырое число задаётся десятичным литералом без экспоненты')
  }

  return { [RAW_LITERAL]: literal } as unknown as number
}

export function serializeBody(body: unknown, headers?: Headers): unknown {
  if (body instanceof FormData) {
    return body
  }

  if (headers?.get('Content-Type') === FORM_CONTENT_TYPE) {
    return new URLSearchParams(body as Record<string, string>).toString()
  }

  const literals = new Map<string, string>()

  const json: string | undefined = JSON.stringify(body, (_key, value: unknown) => {
    if (!isRawNumber(value)) {
      return value
    }

    const placeholder = `${PLACEHOLDER_PREFIX}${crypto.randomUUID()}`

    literals.set(placeholder, value[RAW_LITERAL])

    return placeholder
  })

  if (json === undefined) {
    return undefined
  }

  let serialized = json

  for (const [placeholder, literal] of literals) {
    serialized = substitute(serialized, placeholder, literal)
  }

  return serialized
}
