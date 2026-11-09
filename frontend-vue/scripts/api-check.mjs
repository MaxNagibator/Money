import { readFileSync } from 'node:fs'
import { generate, OUTPUT_PATH, SCHEMA_URL } from './api-schema.mjs'

const generated = await generate()

let committed
try {
  committed = readFileSync(OUTPUT_PATH, 'utf8')
} catch {
  console.error('Сгенерированных типов нет. Запустите pnpm api:gen.')
  process.exit(1)
}

if (committed !== generated) {
  console.error(`Типы разошлись со схемой ${SCHEMA_URL}. Запустите pnpm api:gen и закоммитьте результат.`)
  process.exit(1)
}

console.log('Типы совпадают со схемой API')
