import { useEffect, useMemo, useState } from 'react'
import { dailyPlanApi } from '../api/dailyPlanApi'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import {
  DailyPlanItemType,
  type DailyPlan,
  type DailyPlanItem,
} from '../api/mealPlanningTypes'
import DailyNutritionSummary from './DailyNutritionSummary'
import ErrorList from './ErrorList'
import LoadingIndicator from './LoadingIndicator'
import './DailyPlanSection.css'

type DailyPlanSectionProps = {
  date: string
}

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

const dateFormatter = new Intl.DateTimeFormat('en', {
  day: 'numeric',
  month: 'long',
  weekday: 'long',
  year: 'numeric',
  timeZone: 'UTC',
})

function formatDate(date: string) {
  return dateFormatter.format(new Date(`${date}T00:00:00Z`))
}

function formatTime(plannedTime: string | null) {
  return plannedTime === null ? 'No time' : plannedTime.slice(0, 5)
}

function getItemTypeLabel(itemType: DailyPlanItemType) {
  return itemType === DailyPlanItemType.Ingredient
    ? 'Ingredient'
    : 'Prepared Recipe'
}

function formatItemAmount(item: DailyPlanItem) {
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

export default function DailyPlanSection({ date }: DailyPlanSectionProps) {
  const [dailyPlan, setDailyPlan] = useState<DailyPlan | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadDailyPlan() {
      setIsLoading(true)
      setDailyPlan(null)
      setErrors([])

      try {
        const loadedDailyPlan = await dailyPlanApi.getByDate(date)

        if (isActive) {
          setDailyPlan(loadedDailyPlan)
        }
      } catch (error) {
        if (isActive) {
          setErrors(
            getApiErrorMessages(error, 'Unable to load the Daily Plan.'),
          )
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadDailyPlan()

    return () => {
      isActive = false
    }
  }, [date])

  const sortedItems = useMemo(() => {
    if (dailyPlan === null) {
      return []
    }

    return [...dailyPlan.items].sort((leftItem, rightItem) => {
      if (leftItem.plannedTime === null && rightItem.plannedTime === null) {
        return leftItem.id - rightItem.id
      }

      if (leftItem.plannedTime === null) {
        return 1
      }

      if (rightItem.plannedTime === null) {
        return -1
      }

      const timeComparison = leftItem.plannedTime.localeCompare(
        rightItem.plannedTime,
      )

      return timeComparison !== 0 ? timeComparison : leftItem.id - rightItem.id
    })
  }, [dailyPlan])

  return (
    <section
      className="daily-plan-section"
      aria-labelledby="daily-plan-heading"
    >
      <header className="daily-plan-section__header">
        <div>
          <p className="daily-plan-section__eyebrow">Selected day</p>

          <h2 id="daily-plan-heading">{formatDate(date)}</h2>
        </div>

        {!isLoading && dailyPlan !== null && (
          <strong className="daily-plan-section__count">
            {sortedItems.length} {sortedItems.length === 1 ? 'item' : 'items'}
          </strong>
        )}
      </header>

      <ErrorList messages={errors} />

      {isLoading ? (
        <LoadingIndicator message="Loading Daily Plan..." />
      ) : (
        dailyPlan !== null &&
        (sortedItems.length === 0 ? (
          <div className="daily-plan-section__empty">
            <h3>No food added for this day</h3>

            <p>
              Add an Ingredient or available Prepared Recipe portions when you
              are ready.
            </p>
          </div>
        ) : (
          <>
            <DailyNutritionSummary
              nutrition={dailyPlan.nutrition}
              title="Daily Nutrition"
            />

            <ol className="daily-plan-items">
              {sortedItems.map((item) => (
                <li className="daily-plan-item" key={item.id}>
                  <div
                    className={
                      item.plannedTime === null
                        ? 'daily-plan-item__time daily-plan-item__time--unset'
                        : 'daily-plan-item__time'
                    }
                  >
                    {formatTime(item.plannedTime)}
                  </div>

                  <div className="daily-plan-item__content">
                    <header>
                      <div>
                        <p>{getItemTypeLabel(item.itemType)}</p>
                        <h3>{item.name}</h3>
                      </div>

                      <strong>{formatItemAmount(item)}</strong>
                    </header>

                    <dl className="daily-plan-item__nutrition">
                      <div>
                        <dt>Calories</dt>
                        <dd>
                          {numberFormatter.format(item.nutrition.calories)} kcal
                        </dd>
                      </div>

                      <div>
                        <dt>Protein</dt>
                        <dd>
                          {numberFormatter.format(item.nutrition.protein)} g
                        </dd>
                      </div>

                      <div>
                        <dt>Carbohydrates</dt>
                        <dd>
                          {numberFormatter.format(item.nutrition.carbohydrates)}{' '}
                          g
                        </dd>
                      </div>

                      <div>
                        <dt>Fat</dt>
                        <dd>{numberFormatter.format(item.nutrition.fat)} g</dd>
                      </div>
                    </dl>
                  </div>
                </li>
              ))}
            </ol>
          </>
        ))
      )}
    </section>
  )
}
