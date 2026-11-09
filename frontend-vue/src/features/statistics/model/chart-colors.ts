const PALETTE = [
  '#2196F3',
  '#F44336',
  '#4CAF50',
  '#FFC107',
  '#9C27B0',
  '#00BCD4',
  '#009688',
  '#FF5722',
  '#E91E63',
  '#3F51B5',
  '#03A9F4',
  '#CDDC39',
  '#FFEB3B',
  '#FF9800',
  '#673AB7',
  '#795548',
  '#607D8B',
  '#9E9E9E',
] as const

export const CHART_PALETTE: readonly string[] = PALETTE

export function categoryColor(category: { color: string | null }, position: number): string {
  return category.color ?? fallbackColor(position)
}

export function fallbackColor(position: number): string {
  const index = Math.abs(Math.trunc(position)) % PALETTE.length

  return PALETTE[index] ?? PALETTE[0]
}
