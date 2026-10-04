import { Link } from 'react-router'
import type { WeeklyDay, WeeklySummary } from '../api/mealPlanningTypes'
import { getIsoWeekNumber } from '../utils/isoWeek'
import './DashboardWeeklyPreview.css'

type DashboardWeeklyPreviewProps = {
  weeklySummary: WeeklySummary
  selectedDate: string
  todayDate: string
  onDateSelected: (date: string) => void
}

type DayStatus = 'empty' | 'included' | 'excluded' | 'upcoming'

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

function getDayStatus(day: WeeklyDay, todayDate: string): DayStatus {
  if (!day.hasPlan) {
    return day.date > todayDate ? 'upcoming' : 'empty'
  }

  if (!day.includeInWeeklySummary) {
    return 'excluded'
  }

  return day.date > todayDate ? 'upcoming' : 'included'
}

function getDayStatusLabel(status: DayStatus) {
  if (status === 'included') {
    return 'Included'
  }

  if (status === 'excluded') {
    return 'Excluded'
  }

  if (status === 'upcoming') {
    return 'Upcoming'
  }

  return 'Empty'
}

export default function DashboardWeeklyPreview({
  weeklySummary,
  selectedDate,
  todayDate,
  onDateSelected,
}: DashboardWeeklyPreviewProps) {
  const dateRange = `${dayFormatter.format(
    parseDate(weeklySummary.startDate),
  )} – ${rangeEndFormatter.format(parseDate(weeklySummary.endDate))}`
  const weekNumber = getIsoWeekNumber(weeklySummary.startDate)

  return (
    <section
      className="dashboard-weekly-preview"
      aria-labelledby="dashboard-weekly-preview-heading"
    >
      <header className="dashboard-weekly-preview__header">
        <div>
          <h1 id="dashboard-weekly-preview-heading">Week {weekNumber}</h1>
          <span>{dateRange}</span>
        </div>

        <strong className="dashboard-weekly-preview__included-count">
          {weeklySummary.includedDayCount}{' '}
          {weeklySummary.includedDayCount === 1 ? 'day' : 'days'} counted
        </strong>

        <Link
          className="dashboard-weekly-preview__open"
          to={`/planner?date=${selectedDate}`}
        >
          Open Weekly Planner
        </Link>
      </header>

      <ul className="dashboard-weekly-preview__days">
        {weeklySummary.days.map((day) => {
          const status = getDayStatus(day, todayDate)
          const parsedDate = parseDate(day.date)

          return (
            <li key={day.date}>
              <button
                className="dashboard-weekly-preview__day"
                type="button"
                data-status={status}
                aria-pressed={day.date === selectedDate}
                onClick={() => onDateSelected(day.date)}
              >
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
              </button>
            </li>
          )
        })}
      </ul>
    </section>
  )
}
