import { writeFileSync } from 'node:fs'
import { generate, OUTPUT_PATH, SCHEMA_URL } from './api-schema.mjs'

const contents = await generate()
writeFileSync(OUTPUT_PATH, contents)
console.log(`Типы обновлены из ${SCHEMA_URL}`)
