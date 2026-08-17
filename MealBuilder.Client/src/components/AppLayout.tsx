import { useState } from 'react'
import { Link, NavLink, Outlet } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { useAuth } from '../auth/useAuth'
import ErrorList from './ErrorList'
import LoadingIndicator from './LoadingIndicator'
import './AppLayout.css'

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
        getApiErrorMessages(error, 'Unable to log out. Please try again.'),
      )
    } finally {
      setIsLoggingOut(false)
    }
  }

  return (
    <div className="app-shell">
      <header className="app-header">
        <nav className="app-navigation" aria-label="Main navigation">
          <Link className="app-brand" to="/">
            <span className="app-brand__mark">MB</span>
            <span>MealBuilder</span>
          </Link>

          {user?.isOnboardingComplete ? (
            <div className="app-navigation__primary">
              <NavLink className="app-navigation__link" to="/" end>
                Dashboard
              </NavLink>

              <span className="app-navigation__link" aria-disabled="true">
                Planner
              </span>

              <NavLink
                className="app-navigation__link"
                to="/library/ingredients"
              >
                Library
              </NavLink>
            </div>
          ) : (
            <div />
          )}

          <div className="app-navigation__actions">
            {isLoading ? (
              <LoadingIndicator message="Loading user..." />
            ) : user ? (
              <>
                <NavLink className="app-navigation__auth-link" to="/account">
                  Account
                </NavLink>

                <button
                  className="app-navigation__logout"
                  type="button"
                  disabled={isLoggingOut}
                  onClick={handleLogout}
                >
                  {isLoggingOut ? 'Logging out...' : 'Logout'}
                </button>
              </>
            ) : (
              <>
                <NavLink className="app-navigation__auth-link" to="/login">
                  Login
                </NavLink>

                <NavLink
                  className="app-navigation__auth-link app-navigation__auth-link--primary"
                  to="/register"
                >
                  Register
                </NavLink>
              </>
            )}
          </div>
        </nav>

        <div className="app-header__feedback">
          <ErrorList messages={errors} />
        </div>
      </header>

      <main className="app-main">
        <Outlet />
      </main>
    </div>
  )
}
