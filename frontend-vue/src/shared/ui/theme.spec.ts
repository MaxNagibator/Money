import { describe, expect, it } from 'vitest'
import type { AppTheme } from './theme'
import { darkTheme, lightTheme } from './theme'

const MUD_PALETTE_LIGHT = {
  background: '#FFFFFF',
  surface: '#FFFFFF',
  primary: '#594AE2',
  secondary: '#FF4081',
  tertiary: '#1EC8A5',
  info: '#2196F3',
  success: '#00C853',
  warning: '#FF9800',
  error: '#F44336',
  'on-surface': '#424242',
  'on-tertiary': '#FFFFFF',
}

const MUD_PALETTE_DARK = {
  background: '#32333D',
  surface: '#373740',
  primary: '#776BE7',
  secondary: '#FF4081',
  tertiary: '#1EC8A5',
  info: '#3299FF',
  success: '#0BBA83',
  warning: '#FFA800',
  error: '#F64E62',
  'on-surface': '#FFFFFF',
  'on-tertiary': '#FFFFFF',
}

const MUD_TEXT_LIGHT = {
  high: '#424242',
  medium: '#757575',
  disabled: '#9E9E9E',
}

const MUD_TEXT_DARK = {
  high: 'rgba(255,255,255,0.7)',
  medium: 'rgba(255,255,255,0.56)',
  disabled: 'rgba(255,255,255,0.2)',
}

describe('палитра темы', () => {
  it('повторяет палитру MudBlazor в обеих темах', () => {
    expect(lightTheme.colors).toMatchObject(MUD_PALETTE_LIGHT)
    expect(darkTheme.colors).toMatchObject(MUD_PALETTE_DARK)

    for (const value of [...Object.values(lightTheme.colors ?? {}), ...Object.values(darkTheme.colors ?? {})]) {
      expect(value).toMatch(/^#[0-9A-F]{6}$/)
    }
  })

  it('объявляет одинаковый набор ролей в светлой и тёмной теме', () => {
    const lightRoles = Object.keys(lightTheme.colors ?? {}).sort()
    const darkRoles = Object.keys(darkTheme.colors ?? {}).sort()

    expect(lightRoles).toEqual(darkRoles)
    expect(lightRoles).toContain('tertiary')
  })

  it('даёт текстовую лестницу MudBlazor: основной, второстепенный и выключенный текст', () => {
    expect(textLadder(lightTheme, '#FFFFFF')).toEqual(MUD_TEXT_LIGHT)
    expect(textLadder(darkTheme, '#32333D')).toEqual(MUD_TEXT_DARK)
  })
})

function textLadder(theme: AppTheme, background: string): Record<string, string> {
  const onSurface = theme.colors?.['on-surface']
  const color = typeof onSurface === 'string' ? onSurface : ''
  const variables = theme.variables ?? {}

  return {
    high: blend(color, Number(variables['high-emphasis-opacity']), background),
    medium: blend(color, Number(variables['medium-emphasis-opacity']), background),
    disabled: blend(color, Number(variables['disabled-opacity']), background),
  }
}

function blend(color: string, opacity: number, background: string): string {
  if (color === '#FFFFFF') {
    return `rgba(255,255,255,${opacity})`
  }

  const channels = [1, 3, 5].map(offset => {
    const front = Number.parseInt(color.slice(offset, offset + 2), 16)
    const back = Number.parseInt(background.slice(offset, offset + 2), 16)

    return Math.round(back + (front - back) * opacity)
  })

  return `#${channels.map(channel => channel.toString(16).padStart(2, '0')).join('').toUpperCase()}`
}
