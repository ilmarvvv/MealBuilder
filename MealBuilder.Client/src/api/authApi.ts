import { apiRequest } from './apiClient'

export type AuthUser = {
  id: string
  email: string
}

export type AuthCredentials = {
  email: string
  password: string
}

export const authApi = {
  register(credentials: AuthCredentials) {
    return apiRequest<AuthUser>('/api/auth/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    })
  },

  login(credentials: AuthCredentials) {
    return apiRequest<AuthUser>('/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    })
  },

  getCurrentUser() {
    return apiRequest<AuthUser>('/api/auth/me')
  },

  logout() {
    return apiRequest<void>('/api/auth/logout', {
      method: 'POST',
    })
  },
}