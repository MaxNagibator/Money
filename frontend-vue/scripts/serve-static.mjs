import { createReadStream, existsSync, statSync } from 'node:fs'
import { createServer } from 'node:http'
import { extname, join, normalize, resolve, sep } from 'node:path'

const root = resolve(process.argv[2] ?? '.')
const port = Number(process.argv[3] ?? 4173)
const fallback = join(root, 'index.html')
const MISSING = Symbol('missing')

const types = new Map([
  ['.html', 'text/html; charset=utf-8'],
  ['.js', 'text/javascript; charset=utf-8'],
  ['.mjs', 'text/javascript; charset=utf-8'],
  ['.css', 'text/css; charset=utf-8'],
  ['.json', 'application/json; charset=utf-8'],
  ['.wasm', 'application/wasm'],
  ['.dat', 'application/octet-stream'],
  ['.blat', 'application/octet-stream'],
  ['.bin', 'application/octet-stream'],
  ['.dll', 'application/octet-stream'],
  ['.pdb', 'application/octet-stream'],
  ['.map', 'application/json; charset=utf-8'],
  ['.png', 'image/png'],
  ['.jpg', 'image/jpeg'],
  ['.svg', 'image/svg+xml'],
  ['.ico', 'image/x-icon'],
  ['.woff', 'font/woff'],
  ['.woff2', 'font/woff2'],
  ['.ttf', 'font/ttf'],
  ['.txt', 'text/plain; charset=utf-8'],
  ['.webmanifest', 'application/manifest+json'],
])

if (!existsSync(fallback)) {
  console.error(`Нет index.html в ${root}, сначала соберите фронт`)
  process.exit(1)
}

createServer((request, response) => {
  const path = decodeURIComponent(new URL(request.url ?? '/', 'http://localhost').pathname)
  const target = resolveTarget(path)

  if (target === null) {
    response.writeHead(403).end()
    return
  }

  if (target === MISSING) {
    response.writeHead(404).end()
    return
  }

  response.writeHead(200, {
    'Content-Type': types.get(extname(target).toLowerCase()) ?? 'application/octet-stream',
    'Cache-Control': 'no-store',
  })

  createReadStream(target).pipe(response)
}).listen(port, '127.0.0.1', () => {
  console.log(`Статика ${root} на http://127.0.0.1:${port}`)
})

function resolveTarget(path) {
  const candidate = join(root, normalize(path))

  if (candidate !== root && !candidate.startsWith(root + sep)) {
    return null
  }

  if (existsSync(candidate) && statSync(candidate).isFile()) {
    return candidate
  }

  return extname(candidate) === '' ? fallback : MISSING
}
