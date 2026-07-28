import { environment } from '../config/environment'
import { ApiError } from './ApiError'
import type { ApiValidationErrors } from './ApiError'

const methodsRequiringCsrfToken = new Set([
  'POST',
  'PUT',
  'PATCH',
  'DELETE',
])

type AntiforgeryTokenResponse = {
  token: string
}

type ProblemDetails = {
  title?: string
  detail?: string
  errors?: ApiValidationErrors
}

async function createApiError(response: Response) {
  const problemDetails = (await response
    .json()
    .catch(() => null)) as ProblemDetails | null

  const message =
    problemDetails?.title ??
    problemDetails?.detail ??
    `API request failed with status ${response.status}.`

  return new ApiError(
    message,
    response.status,
    problemDetails?.errors,
  )
}

async function getCsrfToken() {
  const response = await fetch(
    new URL('/api/security/csrf-token', environment.apiBaseUrl),
    {
      credentials: 'include',
    },
  )

  if (!response.ok) {
    throw await createApiError(response)
  }

  const data = (await response.json()) as AntiforgeryTokenResponse

  return data.token
}

export async function apiRequest<TResponse>(
  path: string,
  options: RequestInit = {},
): Promise<TResponse> {
  const method = (options.method ?? 'GET').toUpperCase()
  const headers = new Headers(options.headers)

  if (methodsRequiringCsrfToken.has(method)) {
    headers.set('X-CSRF-TOKEN', await getCsrfToken())
  }

  const response = await fetch(new URL(path, environment.apiBaseUrl), {
    ...options,
    method,
    headers,
    credentials: 'include',
  })

  if (!response.ok) {
    throw await createApiError(response)
  }

  if (response.status === 204) {
    return undefined as TResponse
  }

  return response.json() as Promise<TResponse>
}
