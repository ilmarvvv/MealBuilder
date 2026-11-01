import { useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { preparedRecipeApi } from '../api/preparedRecipeApi'
import type {
  PreparedRecipe,
  PreparedRecipeAllocationInput,
} from '../api/mealPlanningTypes'
import type { Recipe } from '../api/recipeApi'
import ErrorList from './ErrorList'

type PrepareRecipeFormProps = {
  recipe: Recipe
  initialDate?: string
  cancelPath: string
  onTotalPortionsChange: (value: string) => void
  onPrepared: (preparedRecipe: PreparedRecipe) => void
}

function getTodayInputValue() {
  const now = new Date()
  const timezoneOffset = now.getTimezoneOffset() * 60_000

  return new Date(now.getTime() - timezoneOffset).toISOString().slice(0, 10)
}

function toApiPlannedTime(value: string) {
  return value === '' ? null : `${value}:00`
}

const portionFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

function describeDailyPortions(allocations: PreparedRecipeAllocationInput[]) {
  const portions = allocations.map((allocation) => allocation.portions)
  const minimum = Math.min(...portions)
  const maximum = Math.max(...portions)

  if (minimum === maximum) {
    return `${portionFormatter.format(minimum)} portion${
      minimum === 1 ? '' : 's'
    } per day`
  }

  return `${portionFormatter.format(minimum)}–${portionFormatter.format(
    maximum,
  )} portions per day`
}

export default function PrepareRecipeForm({
  recipe,
  initialDate,
  cancelPath,
  onTotalPortionsChange,
  onPrepared,
}: PrepareRecipeFormProps) {
  const defaultDate = initialDate ?? getTodayInputValue()

  const [preparedDate, setPreparedDate] = useState(defaultDate)
  const [totalPortions, setTotalPortions] = useState(String(recipe.servings))
  const [startDate, setStartDate] = useState(defaultDate)
  const [plannedDays, setPlannedDays] = useState(String(recipe.servings))
  const [defaultPlannedTime, setDefaultPlannedTime] = useState('')
  const [allocations, setAllocations] = useState<
    PreparedRecipeAllocationInput[]
  >([])
  const [isPreviewing, setIsPreviewing] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])
  const [previewRetryCount, setPreviewRetryCount] = useState(0)
  const previewRequestId = useRef(0)
  const defaultPlannedTimeRef = useRef('')

  const numericTotalPortions = Number(totalPortions)

  const numericPlannedDays = Number(plannedDays)
  const hasValidPlanningInputs =
    Number.isFinite(numericTotalPortions) &&
    numericTotalPortions > 0 &&
    Number.isInteger(numericPlannedDays) &&
    numericPlannedDays >= 1 &&
    numericPlannedDays <= 365 &&
    preparedDate !== '' &&
    startDate !== '' &&
    startDate >= preparedDate &&
    Math.round(numericTotalPortions * 100) >= numericPlannedDays

  const allocatedPortions = useMemo(
    () =>
      allocations.reduce(
        (total, allocation) => total + Number(allocation.portions),
        0,
      ),
    [allocations],
  )

  const availableAfterPlanning = numericTotalPortions - allocatedPortions

  function clearPreview() {
    previewRequestId.current += 1
    setAllocations([])
    setIsPreviewing(false)
    setErrors([])
  }

  useEffect(() => {
    if (!hasValidPlanningInputs) {
      return
    }

    const requestId = ++previewRequestId.current
    const timeoutId = window.setTimeout(async () => {
      if (requestId !== previewRequestId.current) {
        return
      }

      setIsPreviewing(true)

      try {
        const preview = await preparedRecipeApi.previewPlanning({
          recipeId: recipe.id,
          preparedDate,
          totalPortions: Number(totalPortions),
          startDate,
          plannedDays: Number(plannedDays),
        })

        if (requestId !== previewRequestId.current) {
          return
        }

        const plannedTime = toApiPlannedTime(defaultPlannedTimeRef.current)

        setAllocations(
          preview.map((allocation) => ({
            ...allocation,
            plannedTime,
          })),
        )
      } catch (error) {
        if (requestId === previewRequestId.current) {
          setErrors(
            getApiErrorMessages(error, 'Unable to update portion planning.'),
          )
        }
      } finally {
        if (requestId === previewRequestId.current) {
          setIsPreviewing(false)
        }
      }
    }, 250)

    return () => {
      window.clearTimeout(timeoutId)

      if (previewRequestId.current === requestId) {
        previewRequestId.current += 1
      }
    }
  }, [
    hasValidPlanningInputs,
    recipe.id,
    preparedDate,
    totalPortions,
    startDate,
    plannedDays,
    previewRetryCount,
  ])

  function updateAllocation(
    index: number,
    allocation: PreparedRecipeAllocationInput,
  ) {
    setAllocations((currentAllocations) =>
      currentAllocations.map((currentAllocation, currentIndex) =>
        currentIndex === index ? allocation : currentAllocation,
      ),
    )
  }

  function updateDefaultPlannedTime(value: string) {
    const apiPlannedTime = toApiPlannedTime(value)

    setDefaultPlannedTime(value)
    defaultPlannedTimeRef.current = value
    setAllocations((currentAllocations) =>
      currentAllocations.map((allocation) => ({
        ...allocation,
        plannedTime: apiPlannedTime,
      })),
    )
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!hasValidPlanningInputs || isPreviewing || allocations.length === 0) {
      setErrors(['Wait for a valid portion plan before preparing the Recipe.'])
      return
    }

    if (availableAfterPlanning < -0.001) {
      setErrors(['Planned portions cannot exceed total portions.'])
      return
    }

    setIsSubmitting(true)
    setErrors([])

    try {
      const preparedRecipe = await preparedRecipeApi.create({
        recipeId: recipe.id,
        preparedDate,
        totalPortions: numericTotalPortions,
        allocations,
      })

      onPrepared(preparedRecipe)
    } catch (error) {
      setErrors(getApiErrorMessages(error, 'Unable to prepare the Recipe.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form className="prepare-recipe-form" onSubmit={handleSubmit}>
      <section className="prepare-recipe-form__section">
        <div className="prepare-recipe-form__section-heading">
          <div>
            <p>Preparation</p>
            <h2>Portions and date</h2>
          </div>
        </div>

        <div className="prepare-recipe-form__fields">
          <label>
            <span>Prepared date</span>

            <input
              type="date"
              required
              value={preparedDate}
              onChange={(event) => {
                const nextDate = event.target.value

                setPreparedDate(nextDate)
                setStartDate(nextDate)
                clearPreview()
              }}
            />
          </label>

          <label>
            <span>Total portions</span>

            <input
              type="number"
              min="0.01"
              step="0.01"
              required
              value={totalPortions}
              onChange={(event) => {
                const value = event.target.value

                setTotalPortions(value)
                onTotalPortionsChange(value)
                clearPreview()
              }}
            />
          </label>
        </div>
      </section>

      <section className="prepare-recipe-form__section">
        <div className="prepare-recipe-form__section-heading">
          <div>
            <p>Planning</p>
            <h2>Split across days</h2>
          </div>
        </div>

        <div className="prepare-recipe-form__fields prepare-recipe-form__fields--planning">
          <label>
            <span>Start date</span>

            <input
              type="date"
              min={preparedDate}
              required
              value={startDate}
              onChange={(event) => {
                setStartDate(event.target.value)
                clearPreview()
              }}
            />
          </label>

          <label>
            <span>Number of days</span>

            <input
              type="number"
              min="1"
              max="365"
              step="1"
              required
              value={plannedDays}
              onChange={(event) => {
                setPlannedDays(event.target.value)
                clearPreview()
              }}
            />
          </label>

          <label>
            <span>Default time (optional)</span>

            <input
              type="time"
              value={defaultPlannedTime}
              onChange={(event) => {
                updateDefaultPlannedTime(event.target.value)
              }}
            />
          </label>
        </div>

        {!hasValidPlanningInputs && (
          <ErrorList
            messages={[
              'Enter valid portions and dates. Each day needs at least 0.01 portion.',
            ]}
          />
        )}

        {hasValidPlanningInputs &&
          allocations.length === 0 &&
          errors.length === 0 && (
            <p role="status">
              {isPreviewing ? 'Updating plan...' : 'Calculating plan...'}
            </p>
          )}

        {hasValidPlanningInputs &&
          allocations.length === 0 &&
          errors.length > 0 && (
            <button
              type="button"
              onClick={() => {
                setErrors([])
                setPreviewRetryCount((count) => count + 1)
              }}
            >
              Retry planning
            </button>
          )}

        {allocations.length > 0 && (
          <div
            className="prepare-recipe-form__planning-total"
            aria-live="polite"
          >
            <span>
              {allocations.length} days · {describeDailyPortions(allocations)}
            </span>

            <strong>
              {Math.max(0, availableAfterPlanning).toFixed(2)} available
            </strong>
          </div>
        )}
      </section>

      {allocations.length > 0 && (
        <section className="prepare-recipe-form__section">
          <div className="prepare-recipe-form__section-heading">
            <div>
              <p>Review</p>
              <h2>Planned portions</h2>
            </div>

            <small>{allocations.length} days</small>
          </div>

          <div className="prepare-recipe-form__allocations">
            <div
              className="prepare-recipe-form__allocation-head"
              aria-hidden="true"
            >
              <span>Day</span>
              <span>Date</span>
              <span>Portions</span>
              <span>Time</span>
            </div>
            {allocations.map((allocation, index) => (
              <div className="prepare-recipe-form__allocation" key={index}>
                <strong className="prepare-recipe-form__allocation-day">
                  Day {index + 1}
                </strong>

                <label>
                  <span>Date</span>

                  <input
                    type="date"
                    min={preparedDate}
                    required
                    value={allocation.date}
                    onChange={(event) =>
                      updateAllocation(index, {
                        ...allocation,
                        date: event.target.value,
                      })
                    }
                  />
                </label>

                <label>
                  <span>Portions</span>

                  <input
                    type="number"
                    min="0.01"
                    step="0.01"
                    required
                    value={allocation.portions}
                    onChange={(event) =>
                      updateAllocation(index, {
                        ...allocation,
                        portions: Number(event.target.value),
                      })
                    }
                  />
                </label>

                <label>
                  <span>Time (optional)</span>

                  <input
                    type="time"
                    value={allocation.plannedTime?.slice(0, 5) ?? ''}
                    onChange={(event) =>
                      updateAllocation(index, {
                        ...allocation,
                        plannedTime: toApiPlannedTime(event.target.value),
                      })
                    }
                  />
                </label>
              </div>
            ))}
          </div>

          <div className="prepare-recipe-form__planning-total">
            <span>Planned: {allocatedPortions.toFixed(2)}</span>

            <strong>
              Available after planning:{' '}
              {Math.max(0, availableAfterPlanning).toFixed(2)}
            </strong>
          </div>
        </section>
      )}

      <ErrorList messages={errors} />

      <div className="prepare-recipe-form__actions">
        <Link to={cancelPath}>Cancel</Link>

        <button
          type="submit"
          disabled={
            isSubmitting ||
            isPreviewing ||
            !hasValidPlanningInputs ||
            allocations.length === 0 ||
            availableAfterPlanning < -0.001
          }
        >
          {isSubmitting ? 'Preparing...' : 'Prepare Recipe'}
        </button>
      </div>
    </form>
  )
}
