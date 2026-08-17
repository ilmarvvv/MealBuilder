import { createContext } from 'react'
import type { AuthCredentials, AuthUser } from '../api/authApi'

export type AuthContextValue = {
  user: AuthUser | null
  isLoading: boolean
  refreshUser: () => Promise<void>
  register: (credentials: AuthCredentials) => Promise<void>
  login: (credentials: AuthCredentials) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
)
