import { useEffect, useState } from 'react'
import { dailyPlanApi } from '../api/dailyPlanApi'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import type { WeeklyDay, WeeklySummary } from '../api/mealPlanningTypes'
import DailyNutritionSummary from './DailyNutritionSummary'
import ErrorList from './ErrorList'
import LoadingIndicator from './LoadingIndicator'
import './WeeklyPlannerSection.css'

type WeeklyPlannerSectionProps = {
  selectedDate: string
  refreshRevision: number
  onDateSelected: (date: string) => void
}

type DayStatus = 'empty' | 'included' | 'excluded'

const weekRangeFormatter = new Intl.DateTimeFormat('en', {
  month: 'short',
  day: 'numeric',
  timeZone: 'UTC',
})

const weekEndFormatter = new Intl.DateTimeFormat('en', {
  month: 'short',
  day: 'numeric',
  year: 'numeric',
  timeZone: 'UTC',
})

const weekdayFormatter = new Intl.DateTimeFormat('en', {
  weekday: 'short',
  timeZone: 'UTC',
})

const dayFormatter = new Intl.DateTimeFormat('en', {
  month: 'short',
  day: 'numeric',
  timeZone: 'UTC',
})

const calorieFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 0,
})

function parseDate(date: string) {
  return new Date(`${date}T00:00:00Z`)
}

function formatDateValue(date: Date) {
  return date.toISOString().slice(0, 10)
}

function addDays(date: string, numberOfDays: number) {
  const result = parseDate(date)

  result.setUTCDate(result.getUTCDate() + numberOfDays)

  return formatDateValue(result)
}

function getWeekStart(date: string) {
  const selectedDate = parseDate(date)
  const dayOfWeek = selectedDate.getUTCDay()
  const daysSinceMonday = dayOfWeek === 0 ? 6 : dayOfWeek - 1

  selectedDate.setUTCDate(selectedDate.getUTCDate() - daysSinceMonday)

  return formatDateValue(selectedDate)
}

function getDayStatus(day: WeeklyDay): DayStatus {
  if (!day.hasPlan) {
    return 'empty'
  }

  return day.includeInWeeklySummary ? 'included' : 'excluded'
}

function getDayStatusLabel(status: DayStatus) {
  if (status === 'included') {
    return 'Included'
  }

  if (status === 'excluded') {
    return 'Excluded'
  }

  return 'Empty'
}

export default function WeeklyPlannerSection({
  selectedDate,
  refreshRevision,
  onDateSelected,
}: WeeklyPlannerSectionProps) {
  const weekStartDate = getWeekStart(selectedDate)
  const weekEndDate = addDays(weekStartDate, 6)
  const [weeklySummary, setWeeklySummary] = useState<WeeklySummary | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadWeeklySummary() {
      setIsLoading(true)
      setWeeklySummary(null)
      setErrors([])

      try {
        const loadedWeeklySummary = await dailyPlanApi.getWeek(weekStartDate)

        if (isActive) {
          setWeeklySummary(loadedWeeklySummary)
        }
      } catch (error) {
        if (isActive) {
          setErrors(
            getApiErrorMessages(error, 'Unable to load the weekly summary.'),
          )
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadWeeklySummary()

    return () => {
      isActive = false
    }
  }, [refreshRevision, weekStartDate])

  const weekRange = `${weekRangeFormatter.format(
    parseDate(weekStartDate),
  )} – ${weekEndFormatter.format(parseDate(weekEndDate))}`

  return (
    <section
      className="weekly-planner"
      aria-labelledby="weekly-planner-heading"
    >
      <header className="weekly-planner__header">
        <div>
          <p>Weekly details</p>
          <h2 id="weekly-planner-heading">{weekRange}</h2>
        </div>

        <div className="weekly-planner__navigation">
          <button
            type="button"
            aria-label="Open previous week"
            onClick={() => {
              onDateSelected(addDays(selectedDate, -7))
            }}
          >
            ← Previous
          </button>

          <button
            type="button"
            aria-label="Open next week"
            onClick={() => {
              onDateSelected(addDays(selectedDate, 7))
            }}
          >
            Next →
          </button>
        </div>
      </header>

      <ErrorList messages={errors} />

      {isLoading ? (
        <LoadingIndicator message="Loading weekly summary..." />
      ) : (
        weeklySummary !== null && (
          <>
            <div className="weekly-planner__summary-header">
              <strong>
                {weeklySummary.includedDayCount} of 7 days included
              </strong>

              <span>
                Empty and excluded days do not affect weekly calculations.
              </span>
            </div>

            <ul className="weekly-planner__days">
              {weeklySummary.days.map((day) => {
                const status = getDayStatus(day)
                const isSelected = day.date === selectedDate
                const parsedDate = parseDate(day.date)

                return (
                  <li key={day.date}>
                    <button
                      className="weekly-planner__day"
                      type="button"
                      data-status={status}
                      aria-pressed={isSelected}
                      onClick={() => {
                        onDateSelected(day.date)
                      }}
                    >
                      <span className="weekly-planner__day-date">
                        <strong>{weekdayFormatter.format(parsedDate)}</strong>
                        <small>{dayFormatter.format(parsedDate)}</small>
                      </span>

                      <strong className="weekly-planner__day-calories">
                        {day.hasPlan
                          ? `${calorieFormatter.format(
                              day.nutrition.calories,
                            )} kcal`
                          : '—'}
                      </strong>

                      <small className="weekly-planner__day-status">
                        {getDayStatusLabel(status)}
                      </small>
                    </button>
                  </li>
                )
              })}
            </ul>

            {weeklySummary.includedDayCount > 0 ? (
              <div className="weekly-planner__nutrition">
                <DailyNutritionSummary
                  nutrition={weeklySummary.totalNutrition}
                  title="Weekly Total"
                />

                <DailyNutritionSummary
                  nutrition={weeklySummary.averageNutrition}
                  title="Average per Included Day"
                />
              </div>
            ) : (
              <div className="weekly-planner__empty">
                <h3>No included days</h3>
                <p>Add food to a day or include an existing non-empty day.</p>
              </div>
            )}
          </>
        )
      )}
    </section>
  )
}
