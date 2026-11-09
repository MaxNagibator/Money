import openapiTS, { astToString } from 'openapi-typescript'

export const SCHEMA_URL = process.env.API_SCHEMA_URL
  ?? 'https://localhost:7124/swagger/v1/swagger.json'

export const OUTPUT_PATH = new URL('../src/api/generated/schema.d.ts', import.meta.url)

export async function generate() {
  const url = new URL(SCHEMA_URL)

  if (url.protocol === 'https:' && (url.hostname === 'localhost' || url.hostname === '127.0.0.1')) {
    process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0'
  }

  const ast = await openapiTS(url)
  return `${astToString(ast).trimEnd()}\n`
}
