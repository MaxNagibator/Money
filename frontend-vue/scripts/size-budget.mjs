import { gzipSync } from 'node:zlib'
import { readdirSync, readFileSync } from 'node:fs'
import { join } from 'node:path'

const ASSETS_DIR = 'dist/assets'

const BUDGETS = [
  { name: 'главный CSS', pattern: /^index-[\w-]+\.css$/, limit: 54_000 },
  { name: 'ядро приложения', pattern: /^index-[\w-]+\.js$/, limit: 79_000 },
]

let files
try {
  files = readdirSync(ASSETS_DIR)
} catch {
  console.error(`Сборки нет: ${ASSETS_DIR} не читается. Запустите pnpm build.`)
  process.exit(1)
}

let exceeded = false

for (const budget of BUDGETS) {
  const matched = files.filter(file => budget.pattern.test(file))

  if (matched.length !== 1) {
    console.error(`${budget.name}: ожидался один файл по ${budget.pattern}, найдено ${matched.length}`)
    exceeded = true
    continue
  }

  const size = gzipSync(readFileSync(join(ASSETS_DIR, matched[0])), { level: 9 }).length
  const share = Math.round((size / budget.limit) * 100)

  console.log(`${budget.name}: ${size} Б gzip из ${budget.limit} (${share}%), ${matched[0]}`)

  if (size > budget.limit) {
    console.error(`${budget.name} вырос за бюджет на ${size - budget.limit} Б`)
    exceeded = true
  }
}

if (exceeded) {
  process.exit(1)
}
