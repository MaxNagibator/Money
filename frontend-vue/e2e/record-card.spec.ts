import type { Locator, Page } from '@playwright/test'
import { expect, test } from './support/test'
import { stubScene } from './support/scene'
import { signInAsStub } from './support/session'

const FIRST_RENDER_TIMEOUT = 30_000

const LINE_ALPHA = 0.5
const HEADER_ALPHA = 0.22

interface CardTone {
  borderWidths: string[]
  borderColor: string
  borderRadius: string
  background: string
  headerBorderWidth: string
  headerBorderColor: string
  headerBackground: string
}

async function openScreen(page: Page, path: string, testId: string): Promise<Locator> {
  await page.goto(path)

  const card = page.getByTestId(testId)

  await expect(card).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })

  return card
}

async function readTone(card: Locator): Promise<CardTone> {
  return card.evaluate(element => {
    const box = getComputedStyle(element)
    const headerElement = element.querySelector('.mud-card-header')
    const header = headerElement === null ? null : getComputedStyle(headerElement)

    return {
      borderWidths: [box.borderTopWidth, box.borderRightWidth, box.borderBottomWidth, box.borderLeftWidth],
      borderColor: box.borderTopColor,
      borderRadius: box.borderTopLeftRadius,
      background: box.backgroundColor,
      headerBorderWidth: header?.borderBottomWidth ?? '',
      headerBorderColor: header?.borderBottomColor ?? '',
      headerBackground: header?.backgroundColor ?? '',
    }
  })
}

async function cssVariable(card: Locator, name: string): Promise<string> {
  const raw = await card.evaluate(
    (element, variable) => getComputedStyle(element).getPropertyValue(variable),
    name,
  )

  return raw.trim()
}

async function roleColor(card: Locator, role: string, alpha: number): Promise<string> {
  const triplet = await cssVariable(card, `--mud-palette-${role}-rgb`)

  return `rgba(${triplet.split(',').map(part => part.trim()).join(', ')}, ${alpha})`
}

async function paperBackground(card: Locator): Promise<string> {
  const surface = await cssVariable(card, '--mud-palette-surface')
  const [red, green, blue] = surface.replace(/^rgba?\(|\)$/g, '').split(',').map(part => part.trim())

  return `rgb(${red}, ${green}, ${blue})`
}

async function expectRoleTone(card: Locator, role: string): Promise<void> {
  const tone = await readTone(card)

  expect(tone.borderWidths).toEqual(['1px', '1px', '1px', '1px'])
  expect(tone.borderColor).toBe(await roleColor(card, role, LINE_ALPHA))
  expect(tone.borderRadius).toBe(await cssVariable(card, '--mud-default-borderradius'))
  expect(tone.background).toBe(await paperBackground(card))
  expect(tone.headerBorderWidth).toBe('1px')
  expect(tone.headerBorderColor).toBe(await roleColor(card, role, LINE_ALPHA))
  expect(tone.headerBackground).toBe(await roleColor(card, role, HEADER_ALPHA))
}

test.describe('оформление карточек шаблонов', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
    await stubScene(page)
  })

  test('быстрая операция обводится цветом своего типа, тело остаётся бумагой', async ({ page }) => {
    const expense = await openScreen(page, '/fast-operations', 'fast-operations-card-10')

    await expectRoleTone(expense, 'warning')
    await expectRoleTone(page.getByTestId('fast-operations-card-11'), 'success')
  })

  test('регулярная операция несёт то же оформление', async ({ page }) => {
    const expense = await openScreen(page, '/regular-operations', 'regular-operations-card-1')

    await expectRoleTone(expense, 'warning')
  })
})
