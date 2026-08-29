import { Link } from 'react-router'
import type { WeeklyDay, WeeklySummary } from '../api/mealPlanningTypes'
import DailyNutritionSummary from './DailyNutritionSummary'
import './DashboardWeeklyPreview.css'

type DashboardWeeklyPreviewProps = {
  weeklySummary: WeeklySummary
  calorieTarget: number
}

type DayStatus = 'empty' | 'included' | 'excluded'

const weekdayFormatter = new Intl.DateTimeFormat('en', {
  weekday: 'short',
})

const dayFormatter = new Intl.DateTimeFormat('en', {
  month: 'short',
  day: 'numeric',
})

const rangeEndFormatter = new Intl.DateTimeFormat('en', {
  month: 'short',
  day: 'numeric',
  year: 'numeric',
})

const calorieFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 0,
})

function parseDate(date: string) {
  return new Date(`${date}T00:00:00`)
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

export default function DashboardWeeklyPreview({
  weeklySummary,
  calorieTarget,
}: DashboardWeeklyPreviewProps) {
  const dateRange = `${dayFormatter.format(
    parseDate(weeklySummary.startDate),
  )} – ${rangeEndFormatter.format(parseDate(weeklySummary.endDate))}`

  return (
    <section
      className="dashboard-weekly-preview"
      aria-labelledby="dashboard-weekly-preview-heading"
    >
      <header className="dashboard-weekly-preview__header">
        <div>
          <p>Current Week</p>
          <h2 id="dashboard-weekly-preview-heading">Weekly Preview</h2>
          <span>{dateRange}</span>
        </div>

        <strong>{weeklySummary.includedDayCount} of 7 days included</strong>
      </header>

      <ul className="dashboard-weekly-preview__days">
        {weeklySummary.days.map((day) => {
          const status = getDayStatus(day)
          const parsedDate = parseDate(day.date)

          return (
            <li key={day.date} data-status={status}>
              <time dateTime={day.date}>
                <strong>{weekdayFormatter.format(parsedDate)}</strong>
                <span>{dayFormatter.format(parsedDate)}</span>
              </time>

              <strong>
                {day.hasPlan
                  ? `${calorieFormatter.format(day.nutrition.calories)} kcal`
                  : '—'}
              </strong>

              <small>{getDayStatusLabel(status)}</small>
            </li>
          )
        })}
      </ul>

      {weeklySummary.includedDayCount > 0 ? (
        <DailyNutritionSummary
          nutrition={weeklySummary.averageNutrition}
          calorieTarget={calorieTarget}
          title="Average per Included Day"
        />
      ) : (
        <div className="dashboard-weekly-preview__empty">
          <h3>No included days yet</h3>
          <p>
            Add food to a day or include an existing day in the weekly summary.
          </p>
        </div>
      )}

      <div className="dashboard-weekly-preview__actions">
        <Link to={`/planner?date=${weeklySummary.startDate}`}>
          Open Weekly Planner
        </Link>
      </div>
    </section>
  )
}
