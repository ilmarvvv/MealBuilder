import { ApiError } from './ApiError'

export function getApiErrorMessages(
  error: unknown,
  fallbackMessage: string,
) {
  if (!(error instanceof ApiError)) {
    return [fallbackMessage]
  }

  const validationMessages = Object.values(
    error.validationErrors,
  ).flat()

  if (validationMessages.length > 0) {
    return [...new Set(validationMessages)]
  }

  return [error.message]
}