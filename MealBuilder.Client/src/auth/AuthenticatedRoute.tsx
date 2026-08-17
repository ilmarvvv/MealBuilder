import { Navigate, Outlet } from 'react-router'
import LoadingIndicator from '../components/LoadingIndicator'
import { useAuth } from './useAuth'

type AuthenticatedRouteProps = {
  allowIncompleteOnboarding?: boolean
}

export default function AuthenticatedRoute({
  allowIncompleteOnboarding = false,
}: AuthenticatedRouteProps) {
  const { user, isLoading } = useAuth()

  if (isLoading) {
    return <LoadingIndicator message="Loading account..." />
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (!allowIncompleteOnboarding && !user.isOnboardingComplete) {
    return <Navigate to="/onboarding" replace />
  }

  return <Outlet />
}
