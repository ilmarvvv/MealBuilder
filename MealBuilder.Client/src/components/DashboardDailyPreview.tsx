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
  const previewItems = dailyPlan.items.slice(0, 4)
  const remainingItemCount = dailyPlan.items.length - previewItems.length

  return (
    <section
      className="dashboard-daily-preview"
      aria-labelledby="dashboard-daily-preview-heading"
    >
      <header className="dashboard-daily-preview__header">
        <div>
          <p>Today</p>
          <h2 id="dashboard-daily-preview-heading">Today&apos;s Plan</h2>
        </div>

        <strong>
          {dailyPlan.items.length}{' '}
          {dailyPlan.items.length === 1 ? 'item' : 'items'}
        </strong>
      </header>

      {dailyPlan.items.length === 0 ? (
        <div className="dashboard-daily-preview__empty">
          <h3>No food planned for today</h3>
          <p>Add your first Ingredient or Prepared Recipe.</p>
        </div>
      ) : (
        <>
          <DailyNutritionSummary
            nutrition={dailyPlan.nutrition}
            calorieTarget={calorieTarget}
            title="Today's Nutrition"
          />

          <ul className="dashboard-daily-preview__items">
            {previewItems.map((item) => (
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

          {remainingItemCount > 0 && (
            <p className="dashboard-daily-preview__remaining">
              + {remainingItemCount} more{' '}
              {remainingItemCount === 1 ? 'item' : 'items'}
            </p>
          )}
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
