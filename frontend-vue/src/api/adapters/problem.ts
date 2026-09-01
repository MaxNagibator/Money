export class ApiError extends Error {
}

export function fail(error: unknown, message: string): void {
  if (error === undefined) {
    return
  }

  throw new ApiError(readProblemMessage(error) ?? message)
}

function readProblemMessage(error: unknown): string | null {
  if (typeof error !== 'object' || error === null) {
    return null
  }

  const problem: Record<string, unknown> = { ...error }
  const detail = typeof problem.detail === 'string' ? problem.detail : null
  const title = typeof problem.title === 'string' ? problem.title : null

  return detail ?? title
}
