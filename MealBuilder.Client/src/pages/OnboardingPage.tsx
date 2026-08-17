import { useState } from 'react'
import { Navigate } from 'react-router'
import { useAuth } from '../auth/useAuth'
import CalculatedTargetSetup from '../components/onboarding/CalculatedTargetSetup'
import ManualTargetSetup from '../components/onboarding/ManualTargetSetup'
import LoadingIndicator from '../components/LoadingIndicator'
import './OnboardingPage.css'

type SetupMode = 'choice' | 'calculated' | 'manual'

export default function OnboardingPage() {
  const { user, isLoading } = useAuth()
  const [setupMode, setSetupMode] = useState<SetupMode>('choice')

  if (isLoading) {
    return <LoadingIndicator message="Loading account..." />
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (user.isOnboardingComplete) {
    return <Navigate to="/" replace />
  }

  return (
    <div className="onboarding-page">
      <header className="onboarding-page__header">
        <p className="onboarding-page__eyebrow">Welcome to MealBuilder</p>

        <h1>Set your nutrition target</h1>

        <p>
          Start with a daily calorie target. You can review and change it later
          in Account.
        </p>
      </header>

      {setupMode === 'choice' && (
        <section
          className="onboarding-choice-grid"
          aria-label="Target setup options"
        >
          <button
            className="onboarding-choice onboarding-choice--recommended"
            type="button"
            onClick={() => setSetupMode('calculated')}
          >
            <span className="onboarding-choice__badge">Recommended</span>

            <strong>Calculate my target</strong>

            <span>
              Use body information, activity, and your current weight goal to
              get an estimate.
            </span>
          </button>

          <button
            className="onboarding-choice"
            type="button"
            onClick={() => setSetupMode('manual')}
          >
            <span className="onboarding-choice__badge">Quick setup</span>

            <strong>Set it manually</strong>

            <span>
              Enter a calorie target you already know and start using
              MealBuilder immediately.
            </span>
          </button>
        </section>
      )}

      {setupMode === 'calculated' && (
        <CalculatedTargetSetup onBack={() => setSetupMode('choice')} />
      )}

      {setupMode === 'manual' && (
        <ManualTargetSetup onBack={() => setSetupMode('choice')} />
      )}
    </div>
  )
}
