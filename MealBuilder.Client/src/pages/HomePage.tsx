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

export default function HomePage() {
  const [{ todayDate, weekStartDate }] = useState(getDashboardDates)
  const [selectedDate, setSelectedDate] = useState(todayDate)
  const [profile, setProfile] = useState<NutritionProfile | null>(null)
  const [dailyPlan, setDailyPlan] = useState<DailyPlan | null>(null)
  const [weeklySummary, setWeeklySummary] = useState<WeeklySummary | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])
  const [isDailyPlanLoading, setIsDailyPlanLoading] = useState(true)
  const [dailyPlanErrors, setDailyPlanErrors] = useState<string[]>([])

  useEffect(() => {
    let isCancelled = false

    async function loadDashboard() {
      try {
        const [loadedProfile, loadedWeeklySummary] = await Promise.all([
          profileApi.getCurrent(),
          dailyPlanApi.getWeek(weekStartDate, todayDate),
        ])

        if (!isCancelled) {
          setProfile(loadedProfile)
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

  useEffect(() => {
    let isCancelled = false

    async function loadDailyPlan() {
      setIsDailyPlanLoading(true)
      setDailyPlanErrors([])

      try {
        const loadedDailyPlan = await dailyPlanApi.getByDate(selectedDate)

        if (!isCancelled) {
          setDailyPlan(loadedDailyPlan)
        }
      } catch (error) {
        if (!isCancelled) {
          setDailyPlanErrors(
            getApiErrorMessages(error, 'Unable to load the selected day.'),
          )
        }
      } finally {
        if (!isCancelled) {
          setIsDailyPlanLoading(false)
        }
      }
    }

    void loadDailyPlan()

    return () => {
      isCancelled = true
    }
  }, [selectedDate])

  function selectDate(date: string) {
    if (date === selectedDate) {
      return
    }

    setDailyPlanErrors([])
    setIsDailyPlanLoading(true)
    setSelectedDate(date)
  }

  if (isLoading) {
    return <LoadingIndicator message="Loading Dashboard..." />
  }

  return (
    <section className="dashboard-page">
      <ErrorList messages={errors} />

      {profile && weeklySummary && (
        <>
          <DashboardWeeklyPreview
            weeklySummary={weeklySummary}
            selectedDate={selectedDate}
            todayDate={todayDate}
            onDateSelected={selectDate}
          />

          {dailyPlanErrors.length > 0 ? (
            <ErrorList messages={dailyPlanErrors} />
          ) : isDailyPlanLoading || dailyPlan?.date !== selectedDate ? (
            <LoadingIndicator message="Loading selected day..." />
          ) : (
            <DashboardDailyPreview
              date={selectedDate}
              dailyPlan={dailyPlan}
              calorieTarget={profile.dailyCalorieTarget}
            />
          )}
        </>
      )}
    </section>
  )
}
