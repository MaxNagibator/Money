export const MIN_SKELETON_COUNT = 3
export const MAX_SKELETON_COUNT = 8
export const MAX_SKELETON_ROWS = 2
export const GRID_MD_WIDTH = 960
export const GRID_LG_WIDTH = 1280
export const GRID_SKELETON_HEIGHT = 200
export const GRID_CARD_GAP = 12
export const GRID_CARD_ESTIMATE = GRID_SKELETON_HEIGHT + GRID_CARD_GAP

export function resolveSkeletonCount(viewportHeight: number, cardHeight: number): number {
  if (!Number.isFinite(viewportHeight) || viewportHeight <= 0 || cardHeight <= 0) {
    return MIN_SKELETON_COUNT
  }

  const fitting = Math.ceil(viewportHeight / cardHeight)

  return Math.min(MAX_SKELETON_COUNT, Math.max(MIN_SKELETON_COUNT, fitting))
}

export function resolveGridColumns(viewportWidth: number): number {
  if (!Number.isFinite(viewportWidth) || viewportWidth < GRID_MD_WIDTH) {
    return 1
  }

  return viewportWidth < GRID_LG_WIDTH ? 3 : 4
}

export function resolveGridSkeletonCount(viewportWidth: number, viewportHeight: number, cardHeight: number): number {
  const columns = resolveGridColumns(viewportWidth)
  const fitting = Number.isFinite(viewportHeight) && viewportHeight > 0 && cardHeight > 0
    ? Math.ceil(viewportHeight / cardHeight)
    : MAX_SKELETON_ROWS
  const rows = Math.min(MAX_SKELETON_ROWS, Math.max(1, fitting))

  return columns * rows
}
