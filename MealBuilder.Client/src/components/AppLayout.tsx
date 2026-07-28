import { useState } from 'react'
import { Link, Outlet } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { useAuth } from '../auth/useAuth'
import ErrorList from './ErrorList'
import LoadingIndicator from './LoadingIndicator'

export default function AppLayout() {
  const { user, isLoading, logout } = useAuth()
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  async function handleLogout() {
    setIsLoggingOut(true)
    setErrors([])

    try {
      await logout()
    } catch (error) {
      setErrors(
        getApiErrorMessages(
          error,
          'Unable to log out. Please try again.',
        ),
      )
    } finally {
      setIsLoggingOut(false)
    }
  }

  return (
    <>
      <header>
        <nav aria-label="Main navigation">
          <Link to="/">MealBuilder</Link>

          {isLoading ? (
            <LoadingIndicator message="Loading user..." />
          ) : user ? (
            <>
              <span>{user.email}</span>
              <button
                type="button"
                disabled={isLoggingOut}
                onClick={handleLogout}
              >
                {isLoggingOut ? 'Logging out...' : 'Logout'}
              </button>
            </>
          ) : (
            <>
              <Link to="/login">Login</Link>
              <Link to="/register">Register</Link>
            </>
          )}
        </nav>

        <ErrorList messages={errors} />
      </header>

      <main>
        <Outlet />
      </main>
    </>
  )
}