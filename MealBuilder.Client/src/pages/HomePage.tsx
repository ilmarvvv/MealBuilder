import { useEffect, useState } from 'react'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { profileApi } from '../api/profileApi'
import type { NutritionProfile } from '../api/profileApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import './HomePage.css'

const numberFormatter = new Intl.NumberFormat('en')

export default function HomePage() {
  const [profile, setProfile] = useState<NutritionProfile | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

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
            getApiErrorMessages(error, 'Unable to load your nutrition target.'),
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
    return <LoadingIndicator message="Loading Dashboard..." />
  }

  return (
    <section className="dashboard-page">
      <header className="dashboard-page__header">
        <p className="dashboard-page__eyebrow">Dashboard</p>
        <h1>Today</h1>
        <p>Your daily nutrition overview.</p>
      </header>

      <ErrorList messages={errors} />

      {profile && (
        <section
          className="dashboard-nutrition"
          aria-labelledby="dashboard-nutrition-title"
        >
          <h2 id="dashboard-nutrition-title">Today&apos;s Nutrition</h2>

          <div className="dashboard-nutrition__calories">
            <span>Calories</span>

            <strong>
              0 / {numberFormatter.format(profile.dailyCalorieTarget)} kcal
            </strong>
          </div>

          <progress
            aria-label="Daily calorie progress"
            value={0}
            max={profile.dailyCalorieTarget}
          />

          <p className="dashboard-nutrition__empty">
            Calories from your daily plan will appear here.
          </p>
        </section>
      )}
    </section>
  )
}
