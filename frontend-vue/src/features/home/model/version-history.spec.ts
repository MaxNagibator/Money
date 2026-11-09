import { describe, expect, it } from 'vitest'
import { CHANGE_TYPES, compareVersions } from './version-entry'
import { VERSION_HISTORY } from './version-history'

describe('история версий', () => {
  it('версии не повторяются, строки изменений не пустые и известного типа', () => {
    const changes = VERSION_HISTORY.flatMap(entry => entry.changes)

    expect(new Set(VERSION_HISTORY.map(entry => entry.version)).size).toBe(VERSION_HISTORY.length)
    expect(changes.every(change => change.description.trim().length > 0)).toBe(true)
    expect(changes.every(change => CHANGE_TYPES.includes(change.type))).toBe(true)
  })

  it('заканчивается самой первой версией', () => {
    expect(VERSION_HISTORY.at(-1)).toMatchObject({ version: '0.1.7', date: '2017-05-26', changes: [] })
  })

  it('идёт от новой версии к старой, даты при этом не растут', () => {
    for (let index = 1; index < VERSION_HISTORY.length; index += 1) {
      const previous = VERSION_HISTORY[index - 1]!
      const current = VERSION_HISTORY[index]!

      expect({
        pair: `${previous.version} → ${current.version}`,
        order: compareVersions(previous.version, current.version),
      }).toEqual({ pair: `${previous.version} → ${current.version}`, order: 1 })

      expect(previous.date >= current.date).toBe(true)
    }
  })
})
