import { describe, expect, it } from 'vitest'
import { formatTimeOfDay, normalizeTimeInput, parseTimeInput } from './time-input'

describe('parseTimeInput', () => {
  it.each([
    ['18:00', 1080],
    ['18.00', 1080],
    ['18-00', 1080],
    ['18,00', 1080],
    ['18 00', 1080],
    ['1800', 1080],
    ['800', 480],
    ['8:5', 485],
    ['6', 360],
    ['06', 360],
    ['  23:59  ', 1439],
    ['00:00', 0],
  ])('разбирает %j', (raw, expected) => {
    expect(parseTimeInput(raw)).toBe(expected)
  })

  it.each([
    [''],
    ['   '],
    ['24:00'],
    ['18:60'],
    ['2400'],
    ['18:00:30'],
    ['6:00 PM'],
    ['вечер'],
    ['12345'],
    ['-1:00'],
  ])('отвергает %j', raw => {
    expect(parseTimeInput(raw)).toBeNull()
  })
})

describe('formatTimeOfDay', () => {
  it.each([
    [0, '00:00'],
    [360, '06:00'],
    [485, '08:05'],
    [1080, '18:00'],
    [1439, '23:59'],
  ])('печатает %i как 24-часовое %s', (minutes, expected) => {
    expect(formatTimeOfDay(minutes)).toBe(expected)
  })

  it('не отдаёт AM/PM ни при какой локали браузера', () => {
    const values = Array.from({ length: 24 }, (_, hour) => formatTimeOfDay(hour * 60))

    expect(values.every(value => /^\d{2}:\d{2}$/.test(value))).toBe(true)
    expect(values).toContain('13:00')
  })

  it.each([
    [1440, '00:00'],
    [-60, '23:00'],
    [Number.NaN, '00:00'],
  ])('приводит %j к суткам', (minutes, expected) => {
    expect(formatTimeOfDay(minutes)).toBe(expected)
  })
})

describe('normalizeTimeInput', () => {
  it.each([
    ['1800', '18:00'],
    ['6', '06:00'],
    ['8:5', '08:05'],
  ])('нормализует %j в %s', (raw, expected) => {
    expect(normalizeTimeInput(raw)).toBe(expected)
  })

  it('возвращает null на нераспознанном вводе', () => {
    expect(normalizeTimeInput('6:00 PM')).toBeNull()
  })
})
