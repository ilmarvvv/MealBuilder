import { useMemo, useState } from 'react'
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
  cancelPath: string
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

export default function PrepareRecipeForm({
  recipe,
  cancelPath,
  onPrepared,
}: PrepareRecipeFormProps) {
  const [preparedDate, setPreparedDate] = useState(getTodayInputValue)
  const [totalPortions, setTotalPortions] = useState(String(recipe.servings))
  const [automaticallyPlan, setAutomaticallyPlan] = useState(true)
  const [startDate, setStartDate] = useState(getTodayInputValue)
  const [plannedDays, setPlannedDays] = useState('1')
  const [defaultPlannedTime, setDefaultPlannedTime] = useState('')
  const [allocations, setAllocations] = useState<
    PreparedRecipeAllocationInput[]
  >([])
  const [isPreviewing, setIsPreviewing] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  const numericTotalPortions = Number(totalPortions)

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
    setAllocations([])
    setErrors([])
  }

  async function handlePreview() {
    const numericPlannedDays = Number(plannedDays)

    if (
      !Number.isFinite(numericTotalPortions) ||
      numericTotalPortions <= 0 ||
      !Number.isInteger(numericPlannedDays) ||
      numericPlannedDays < 1 ||
      numericPlannedDays > 365
    ) {
      setErrors(['Enter valid total portions and planned days.'])
      return
    }

    setIsPreviewing(true)
    setErrors([])

    try {
      const preview = await preparedRecipeApi.previewPlanning({
        recipeId: recipe.id,
        preparedDate,
        totalPortions: numericTotalPortions,
        startDate,
        plannedDays: numericPlannedDays,
      })

      const apiPlannedTime = toApiPlannedTime(defaultPlannedTime)

      setAllocations(
        preview.map((allocation) => ({
          ...allocation,
          plannedTime: apiPlannedTime,
        })),
      )
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to preview portion planning.'),
      )
    } finally {
      setIsPreviewing(false)
    }
  }

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
    setAllocations((currentAllocations) =>
      currentAllocations.map((allocation) => ({
        ...allocation,
        plannedTime: apiPlannedTime,
      })),
    )
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (automaticallyPlan && allocations.length === 0) {
      setErrors(['Preview the portion plan before preparing the Recipe.'])
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
        allocations: automaticallyPlan ? allocations : [],
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

          <span>1</span>
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
                setTotalPortions(event.target.value)
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
            <h2>Choose how to use the portions</h2>
          </div>

          <span>2</span>
        </div>

        <label className="prepare-recipe-form__toggle">
          <input
            type="checkbox"
            checked={automaticallyPlan}
            onChange={(event) => {
              setAutomaticallyPlan(event.target.checked)
              clearPreview()
            }}
          />

          <span>
            <strong>Automatically Plan Portions</strong>
            <small>Preview an even distribution across consecutive days.</small>
          </span>
        </label>

        {automaticallyPlan ? (
          <>
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

            <button
              className="prepare-recipe-form__preview"
              type="button"
              disabled={isPreviewing || isSubmitting}
              onClick={handlePreview}
            >
              {isPreviewing ? 'Creating preview...' : 'Preview planning'}
            </button>
          </>
        ) : (
          <div className="prepare-recipe-form__available-note">
            <strong>
              All {totalPortions || '0'} portions will remain available.
            </strong>

            <p>You can assign them to any valid date later from Planner.</p>
          </div>
        )}
      </section>

      {automaticallyPlan && allocations.length > 0 && (
        <section className="prepare-recipe-form__section">
          <div className="prepare-recipe-form__section-heading">
            <div>
              <p>Review</p>
              <h2>Planned portions</h2>
            </div>

            <span>{allocations.length}</span>
          </div>

          <div className="prepare-recipe-form__allocations">
            {allocations.map((allocation, index) => (
              <div
                className="prepare-recipe-form__allocation"
                key={`${index}-${allocation.date}`}
              >
                <label>
                  <span>Day {index + 1}</span>

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
            !Number.isFinite(numericTotalPortions) ||
            numericTotalPortions <= 0
          }
        >
          {isSubmitting ? 'Preparing...' : 'Prepare Recipe'}
        </button>
      </div>
    </form>
  )
}
