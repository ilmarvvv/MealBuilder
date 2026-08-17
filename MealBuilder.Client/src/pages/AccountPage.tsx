import { useEffect, useState } from 'react'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { profileApi } from '../api/profileApi'
import type { NutritionProfile } from '../api/profileApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import DailyCalorieTargetSettings from '../components/profile/DailyCalorieTargetSettings'
import CalculatedTargetSetup from '../components/onboarding/CalculatedTargetSetup'
import './AccountPage.css'

export default function AccountPage() {
  const [profile, setProfile] = useState<NutritionProfile | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])
  const [isRecalculating, setIsRecalculating] = useState(false)

  useEffect(() => {
    let isCancelled = false

    async function loadProfile() {
      try {
        const loadedProfile = await profileApi.getCurrent()

        if (!isCancelled) {
          setProfile(loadedProfile)
        }
      } catch (error) {
        if (!isCancelled) {
          setErrors(
            getApiErrorMessages(
              error,
              'Unable to load your nutrition profile.',
            ),
          )
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false)
        }
      }
    }

    void loadProfile()

    return () => {
      isCancelled = true
    }
  }, [])

  if (isLoading) {
    return <LoadingIndicator message="Loading nutrition profile..." />
  }

  return (
    <section className="account-page">
      <header className="account-page__header">
        <p className="account-page__eyebrow">Account</p>
        <h1>Nutrition profile</h1>
        <p>Review and manage your saved daily calorie target.</p>
      </header>

      <ErrorList messages={errors} />

      {profile && (
        <>
          <DailyCalorieTargetSettings
            profile={profile}
            onProfileChanged={setProfile}
          />

          <section className="account-page__section">
            <header>
              <h2>Recalculate target</h2>
              <p>
                Update your calculation information and review a new estimate.
                Your saved target will not change until you confirm it.
              </p>
            </header>

            {isRecalculating ? (
              <CalculatedTargetSetup
                initialProfile={profile}
                onBack={() => setIsRecalculating(false)}
                onSaved={(updatedProfile) => {
                  setProfile(updatedProfile)
                  setIsRecalculating(false)
                }}
              />
            ) : (
              <button
                className="button-secondary"
                type="button"
                onClick={() => setIsRecalculating(true)}
              >
                Recalculate target
              </button>
            )}
          </section>
        </>
      )}
    </section>
  )
}
