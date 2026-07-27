import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { authApi } from '../api/authApi'
import type { AuthCredentials, AuthUser } from '../api/authApi'
import { AuthContext } from './AuthContext'
import type { AuthContextValue } from './AuthContext'

type AuthProviderProps = {
  children: ReactNode
}

export default function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let isActive = true

    async function loadCurrentUser() {
      try {
        const currentUser = await authApi.getCurrentUser()

        if (isActive) {
          setUser(currentUser)
        }
      } catch {
        if (isActive) {
          setUser(null)
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadCurrentUser()

    return () => {
      isActive = false
    }
  }, [])

  async function register(credentials: AuthCredentials) {
    const registeredUser = await authApi.register(credentials)
    setUser(registeredUser)
  }

  async function login(credentials: AuthCredentials) {
    const authenticatedUser = await authApi.login(credentials)
    setUser(authenticatedUser)
  }

  async function logout() {
    await authApi.logout()
    setUser(null)
  }

  const value: AuthContextValue = {
    user,
    isLoading,
    register,
    login,
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}