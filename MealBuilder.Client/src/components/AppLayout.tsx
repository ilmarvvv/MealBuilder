import { useState } from 'react'
import { Link, Outlet } from 'react-router'
import { useAuth } from '../auth/useAuth'

export default function AppLayout() {
  const { user, isLoading, logout } = useAuth()
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const [logoutError, setLogoutError] = useState<string | null>(null)

  async function handleLogout() {
    setIsLoggingOut(true)
    setLogoutError(null)

    try {
      await logout()
    } catch {
      setLogoutError('Unable to log out. Please try again.')
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
            <span>Loading...</span>
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

        {logoutError && <p role="alert">{logoutError}</p>}
      </header>

      <main>
        <Outlet />
      </main>
    </>
  )
}