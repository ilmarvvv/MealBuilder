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
import AddFoodModal from './AddFoodModal'
import DailyPlanItemActions from './DailyPlanItemActions'
import DailyPlanUndoNotice from './DailyPlanUndoNotice'
import './DailyPlanSection.css'

type DailyPlanSectionProps = {
  date: string
  calorieTarget?: number
  onFoodAdded: () => void
  onPlanChanged: () => void
  openAddFoodInitially?: boolean
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

export default function DailyPlanSection({
  date,
  calorieTarget,
  onFoodAdded,
  onPlanChanged,
  openAddFoodInitially = false,
}: DailyPlanSectionProps) {
  const [dailyPlan, setDailyPlan] = useState<DailyPlan | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])
  const [isAddFoodOpen, setIsAddFoodOpen] = useState(openAddFoodInitially)
  const [isUpdatingWeeklyInclusion, setIsUpdatingWeeklyInclusion] =
    useState(false)
  const [removedItem, setRemovedItem] = useState<DailyPlanItem | null>(null)
  const [isUndoing, setIsUndoing] = useState(false)
  const [undoErrors, setUndoErrors] = useState<string[]>([])

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

  useEffect(() => {
    if (removedItem === null || isUndoing) {
      return
    }

    const timeoutId = window.setTimeout(() => {
      setRemovedItem(null)
      setUndoErrors([])
    }, 5000)

    return () => {
      window.clearTimeout(timeoutId)
    }
  }, [isUndoing, removedItem])

  async function handleWeeklySummaryInclusionChange(
    includeInWeeklySummary: boolean,
  ) {
    if (dailyPlan === null || dailyPlan.id === null) {
      return
    }

    setIsUpdatingWeeklyInclusion(true)
    setErrors([])

    try {
      const updatedDailyPlan = await dailyPlanApi.setWeeklySummaryInclusion(
        dailyPlan.id,
        {
          includeInWeeklySummary,
        },
      )

      setDailyPlan(updatedDailyPlan)
      onPlanChanged()
    } catch (error) {
      setErrors(
        getApiErrorMessages(
          error,
          'Unable to update the weekly summary setting.',
        ),
      )
    } finally {
      setIsUpdatingWeeklyInclusion(false)
    }
  }

  async function handleUndo() {
    if (removedItem === null) {
      return
    }

    setIsUndoing(true)
    setUndoErrors([])

    try {
      let restoredDailyPlan: DailyPlan

      if (removedItem.itemType === DailyPlanItemType.Ingredient) {
        if (removedItem.ingredientId === null || removedItem.grams === null) {
          setUndoErrors(['The removed Ingredient cannot be restored.'])
          return
        }

        restoredDailyPlan = await dailyPlanApi.addIngredient(date, {
          ingredientId: removedItem.ingredientId,
          grams: removedItem.grams,
          plannedTime: removedItem.plannedTime,
        })
      } else {
        if (
          removedItem.preparedRecipeId === null ||
          removedItem.portions === null
        ) {
          setUndoErrors(['The removed Prepared Recipe cannot be restored.'])
          return
        }

        restoredDailyPlan = await dailyPlanApi.addPreparedRecipe(date, {
          preparedRecipeId: removedItem.preparedRecipeId,
          portions: removedItem.portions,
          plannedTime: removedItem.plannedTime,
        })
      }

      setDailyPlan(restoredDailyPlan)
      setRemovedItem(null)
      setUndoErrors([])
      onPlanChanged()

      if (removedItem.itemType === DailyPlanItemType.PreparedRecipe) {
        onFoodAdded()
      }
    } catch (error) {
      setUndoErrors(
        getApiErrorMessages(error, 'Unable to restore the removed item.'),
      )
    } finally {
      setIsUndoing(false)
    }
  }

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

        <div className="daily-plan-section__actions">
          {!isLoading && dailyPlan !== null && (
            <strong className="daily-plan-section__count">
              {sortedItems.length} {sortedItems.length === 1 ? 'item' : 'items'}
            </strong>
          )}

          <button
            className="daily-plan-section__add"
            type="button"
            onClick={() => {
              setIsAddFoodOpen(true)
            }}
          >
            + Add Food
          </button>
        </div>
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
              Add an Ingredient, prepare a Recipe, or assign Available Portions
              to this day.
            </p>
          </div>
        ) : (
          <>
            <label className="daily-plan-weekly-inclusion">
              <input
                type="checkbox"
                checked={dailyPlan.includeInWeeklySummary}
                disabled={isUpdatingWeeklyInclusion}
                onChange={(event) => {
                  void handleWeeklySummaryInclusionChange(event.target.checked)
                }}
              />

              <span>
                <strong>Include this day in weekly summary</strong>

                <small>
                  {dailyPlan.includeInWeeklySummary
                    ? 'This day contributes to weekly totals and averages.'
                    : 'Excluded from weekly summary.'}
                </small>
              </span>
            </label>
            <DailyNutritionSummary
              nutrition={dailyPlan.nutrition}
              calorieTarget={calorieTarget}
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
                        <dt>Carbohydrates / Sugars</dt>
                        <dd>
                          {numberFormatter.format(item.nutrition.carbohydrates)}{' '}
                          / {numberFormatter.format(item.nutrition.sugars)} g
                        </dd>
                      </div>

                      <div>
                        <dt>Fiber</dt>
                        <dd>
                          {numberFormatter.format(item.nutrition.fiber)} g
                        </dd>
                      </div>

                      <div>
                        <dt>Fat</dt>
                        <dd>{numberFormatter.format(item.nutrition.fat)} g</dd>
                      </div>

                      <div>
                        <dt>Salt</dt>
                        <dd>{numberFormatter.format(item.nutrition.salt)} g</dd>
                      </div>
                    </dl>
                    {dailyPlan.id !== null && (
                      <DailyPlanItemActions
                        dailyPlanId={dailyPlan.id}
                        planDate={date}
                        item={item}
                        onPlanUpdated={(updatedDailyPlan) => {
                          setDailyPlan(updatedDailyPlan)
                          setErrors([])
                          onPlanChanged()
                        }}
                        onMoved={(result) => {
                          setDailyPlan(result.sourcePlan)
                          setErrors([])
                          onPlanChanged()
                        }}
                        onRemoved={(removedDailyPlanItem, updatedDailyPlan) => {
                          setDailyPlan(updatedDailyPlan)
                          setRemovedItem(removedDailyPlanItem)
                          setIsUndoing(false)
                          setUndoErrors([])
                          onPlanChanged()
                        }}
                        onPreparedRecipesChanged={onFoodAdded}
                      />
                    )}
                  </div>
                </li>
              ))}
            </ol>
          </>
        ))
      )}

      {removedItem !== null && (
        <DailyPlanUndoNotice
          itemName={removedItem.name}
          isUndoing={isUndoing}
          errors={undoErrors}
          onUndo={handleUndo}
          onDismiss={() => {
            setRemovedItem(null)
            setUndoErrors([])
          }}
        />
      )}

      <AddFoodModal
        date={date}
        isOpen={isAddFoodOpen}
        onAdded={(updatedDailyPlan) => {
          setDailyPlan(updatedDailyPlan)
          setErrors([])
          onFoodAdded()
          onPlanChanged()
        }}
        onClose={() => {
          setIsAddFoodOpen(false)
        }}
      />
    </section>
  )
}
