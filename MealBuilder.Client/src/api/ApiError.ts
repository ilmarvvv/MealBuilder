export type ApiValidationErrors = Record<string, string[]>

export class ApiError extends Error {
  readonly status: number
  readonly validationErrors: ApiValidationErrors

  constructor(
    message: string,
    status: number,
    validationErrors: ApiValidationErrors = {},
  ) {
    super(message)

    this.name = 'ApiError'
    this.status = status
    this.validationErrors = validationErrors
  }
}
