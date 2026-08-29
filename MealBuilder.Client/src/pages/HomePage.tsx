import { useEffect, useState } from 'react'
import { dailyPlanApi } from '../api/dailyPlanApi'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import type { DailyPlan, WeeklySummary } from '../api/mealPlanningTypes'
import { profileApi } from '../api/profileApi'
import type { NutritionProfile } from '../api/profileApi'
import DashboardDailyPreview from '../components/DashboardDailyPreview'
import DashboardWeeklyPreview from '../components/DashboardWeeklyPreview'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import './HomePage.css'

const dateFormatter = new Intl.DateTimeFormat('en', {
  weekday: 'long',
  month: 'long',
  day: 'numeric',
  year: 'numeric',
})

function formatLocalDate(date: Date) {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')

  return `${year}-${month}-${day}`
}

function getDashboardDates() {
  const today = new Date()
  const weekStart = new Date(today)
  const daysSinceMonday = today.getDay() === 0 ? 6 : today.getDay() - 1

  weekStart.setDate(today.getDate() - daysSinceMonday)

  return {
    todayDate: formatLocalDate(today),
    weekStartDate: formatLocalDate(weekStart),
  }
}

function parseDate(date: string) {
  return new Date(`${date}T00:00:00`)
}

export default function HomePage() {
  const [{ todayDate, weekStartDate }] = useState(getDashboardDates)
  const [profile, setProfile] = useState<NutritionProfile | null>(null)
  const [dailyPlan, setDailyPlan] = useState<DailyPlan | null>(null)
  const [weeklySummary, setWeeklySummary] = useState<WeeklySummary | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isCancelled = false

    async function loadDashboard() {
      try {
        const [loadedProfile, loadedDailyPlan, loadedWeeklySummary] =
          await Promise.all([
            profileApi.getCurrent(),
            dailyPlanApi.getByDate(todayDate),
            dailyPlanApi.getWeek(weekStartDate),
          ])

        if (!isCancelled) {
          setProfile(loadedProfile)
          setDailyPlan(loadedDailyPlan)
          setWeeklySummary(loadedWeeklySummary)
        }
      } catch (error) {
        if (!isCancelled) {
          setErrors(getApiErrorMessages(error, 'Unable to load the Dashboard.'))
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false)
        }
      }
    }

    void loadDashboard()

    return () => {
      isCancelled = true
    }
  }, [todayDate, weekStartDate])

  if (isLoading) {
    return <LoadingIndicator message="Loading Dashboard..." />
  }

  return (
    <section className="dashboard-page">
      <header className="dashboard-page__header">
        <p className="dashboard-page__eyebrow">Dashboard</p>
        <h1>Today</h1>
        <p>{dateFormatter.format(parseDate(todayDate))}</p>
      </header>

      <ErrorList messages={errors} />

      {profile && dailyPlan && weeklySummary && (
        <>
          <DashboardDailyPreview
            date={todayDate}
            dailyPlan={dailyPlan}
            calorieTarget={profile.dailyCalorieTarget}
          />

          <DashboardWeeklyPreview
            weeklySummary={weeklySummary}
            calorieTarget={profile.dailyCalorieTarget}
          />
        </>
      )}
    </section>
  )
}
