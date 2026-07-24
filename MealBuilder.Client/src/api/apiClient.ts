import { environment } from '../config/environment'

const methodsRequiringCsrfToken = new Set([
  'POST',
  'PUT',
  'PATCH',
  'DELETE',
])

type AntiforgeryTokenResponse = {
  token: string
}

async function getCsrfToken() {
  const response = await fetch(
    new URL('/api/security/csrf-token', environment.apiBaseUrl),
    {
      credentials: 'include',
    },
  )

  if (!response.ok) {
    throw new Error('Failed to retrieve the CSRF token.')
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
    throw new Error(`API request failed with status ${response.status}.`)
  }

  if (response.status === 204) {
    return undefined as TResponse
  }

  return response.json() as Promise<TResponse>
}