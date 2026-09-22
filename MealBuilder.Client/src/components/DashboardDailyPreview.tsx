import { Link } from 'react-router'
import {
  DailyPlanItemType,
  type DailyPlan,
  type DailyPlanItem,
} from '../api/mealPlanningTypes'
import DailyNutritionSummary from './DailyNutritionSummary'
import './DashboardDailyPreview.css'

type DashboardDailyPreviewProps = {
  date: string
  dailyPlan: DailyPlan
  calorieTarget: number
}

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

const dateFormatter = new Intl.DateTimeFormat('en', {
  weekday: 'long',
  month: 'long',
  day: 'numeric',
  year: 'numeric',
})

const weekdayFormatter = new Intl.DateTimeFormat('en', {
  weekday: 'long',
})

function getDayLabel(date: string) {
  const selectedDate = new Date(`${date}T00:00:00`)
  const today = new Date()

  today.setHours(0, 0, 0, 0)

  const yesterday = new Date(today)
  yesterday.setDate(yesterday.getDate() - 1)

  const tomorrow = new Date(today)
  tomorrow.setDate(tomorrow.getDate() + 1)

  if (selectedDate.getTime() === today.getTime()) {
    return 'Today'
  }

  if (selectedDate.getTime() === tomorrow.getTime()) {
    return 'Tomorrow'
  }

  if (selectedDate.getTime() === yesterday.getTime()) {
    return 'Yesterday'
  }

  return weekdayFormatter.format(selectedDate)
}

function formatTime(plannedTime: string | null) {
  return plannedTime === null ? 'No time' : plannedTime.slice(0, 5)
}

function formatAmount(item: DailyPlanItem) {
  if (item.itemType === DailyPlanItemType.Ingredient && item.grams !== null) {
    return `${numberFormatter.format(item.grams)} g`
  }

  if (
    item.itemType === DailyPlanItemType.PreparedRecipe &&
    item.portions !== null
  ) {
    const unit = item.portions === 1 ? 'portion' : 'portions'

    return `${numberFormatter.format(item.portions)} ${unit}`
  }

  return 'Amount unavailable'
}

export default function DashboardDailyPreview({
  date,
  dailyPlan,
  calorieTarget,
}: DashboardDailyPreviewProps) {
  const dayLabel = getDayLabel(date)

  return (
    <section
      className="dashboard-daily-preview"
      aria-labelledby="dashboard-daily-preview-heading"
    >
      <header className="dashboard-daily-preview__header">
        <div>
          <h2 id="dashboard-daily-preview-heading">{dayLabel}&apos;s Plan</h2>

          <p>
            <time dateTime={date}>
              {dateFormatter.format(new Date(`${date}T00:00:00`))}
            </time>
          </p>
        </div>

        <strong>
          {dailyPlan.items.length}{' '}
          {dailyPlan.items.length === 1 ? 'item' : 'items'}
        </strong>
      </header>

      {dailyPlan.items.length === 0 ? (
        <div className="dashboard-daily-preview__empty">
          <h3>No food planned for this day</h3>
          <p>Add an Ingredient, prepare a Recipe, or use Available Portions.</p>
        </div>
      ) : (
        <>
          <DailyNutritionSummary
            nutrition={dailyPlan.nutrition}
            calorieTarget={calorieTarget}
            title="Nutrition"
          />

          <ul className="dashboard-daily-preview__items">
            {dailyPlan.items.map((item) => (
              <li key={item.id}>
                <span
                  className={
                    item.plannedTime === null
                      ? 'dashboard-daily-preview__time dashboard-daily-preview__time--unset'
                      : 'dashboard-daily-preview__time'
                  }
                >
                  {formatTime(item.plannedTime)}
                </span>

                <div>
                  <strong>{item.name}</strong>
                  <small>
                    {item.itemType === DailyPlanItemType.Ingredient
                      ? 'Ingredient'
                      : 'Prepared Recipe'}
                  </small>
                </div>

                <strong>{formatAmount(item)}</strong>
              </li>
            ))}
          </ul>
        </>
      )}

      <div className="dashboard-daily-preview__actions">
        <Link
          className="dashboard-daily-preview__add"
          to={`/planner?date=${date}&addFood=true`}
        >
          + Add Food
        </Link>

        <Link
          className="dashboard-daily-preview__open"
          to={`/planner?date=${date}`}
        >
          Open Planner
        </Link>
      </div>
    </section>
  )
}
