import { describe, expect, it } from 'vitest'
import { describeBuild, describeStorage, describeUpdate, NEW_VERSION_TEXT } from './app-info'

describe('describeBuild', () => {
  it.each([
    ['хеш и дата', { commit: '59fd1f7', builtAt: '2026-09-29T17:15:00Z' }, '59fd1f7 от 29.09.2026, 20:15'],
    ['сборка вне git', { commit: '', builtAt: '2026-09-29T17:15:00Z' }, 'от 29.09.2026, 20:15'],
    ['испорченная дата', { commit: '59fd1f7', builtAt: 'вчера' }, '59fd1f7'],
    ['ничего не известно', { commit: ' ', builtAt: '' }, ''],
  ])('%s', (_case, build, expected) => {
    expect(describeBuild(build)).toBe(expected)
  })
})

describe('describeUpdate', () => {
  it('готовая к установке версия важнее результата прошлой проверки', () => {
    expect(describeUpdate(true, 'latest')).toBe(NEW_VERSION_TEXT)
  })

  it('до первой проверки ничего не пишет', () => {
    expect(describeUpdate(false, 'idle')).toBeNull()
  })
})

describe('describeStorage', () => {
  it.each([
    ['пока считается', undefined, 'Считаем занятое место…'],
    ['браузер не сообщает', null, 'Браузер не сообщает, сколько места занято'],
    ['известно', { usage: 3_500_000, quota: 2_147_483_648 }, 'Занято 3,3 МБ из доступных 2 ГБ'],
  ])('%s', (_case, usage, expected) => {
    expect(describeStorage(usage)).toBe(expected)
  })
})
