import { useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import type { PreparedRecipeSummary } from '../api/mealPlanningTypes'
import { preparedRecipeApi } from '../api/preparedRecipeApi'
import DailyPlanSection from '../components/DailyPlanSection'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import './PlannerPage.css'

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

const dateFormatter = new Intl.DateTimeFormat('en', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
  timeZone: 'UTC',
})

function formatDate(date: string) {
  return dateFormatter.format(new Date(`${date}T00:00:00Z`))
}

function getTodayDateValue() {
  const today = new Date()
  const year = today.getFullYear()
  const month = String(today.getMonth() + 1).padStart(2, '0')
  const day = String(today.getDate()).padStart(2, '0')

  return `${year}-${month}-${day}`
}

function isValidDateValue(value: string | null): value is string {
  if (value === null || !/^\d{4}-\d{2}-\d{2}$/.test(value)) {
    return false
  }

  const parsedDate = new Date(`${value}T00:00:00Z`)

  return (
    !Number.isNaN(parsedDate.getTime()) &&
    parsedDate.toISOString().slice(0, 10) === value
  )
}

export default function PlannerPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const requestedDate = searchParams.get('date')
  const selectedDate = isValidDateValue(requestedDate)
    ? requestedDate
    : getTodayDateValue()
  const [preparedRecipes, setPreparedRecipes] = useState<
    PreparedRecipeSummary[]
  >([])
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])
  const [showAllPreparedRecipes, setShowAllPreparedRecipes] = useState(false)

  useEffect(() => {
    let isActive = true

    async function loadPreparedRecipes() {
      setIsLoading(true)
      setErrors([])

      try {
        const loadedPreparedRecipes = await preparedRecipeApi.getAll()

        if (isActive) {
          setPreparedRecipes(loadedPreparedRecipes)
        }
      } catch (error) {
        if (isActive) {
          setErrors(
            getApiErrorMessages(error, 'Unable to load Available Portions.'),
          )
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadPreparedRecipes()

    return () => {
      isActive = false
    }
  }, [])

  const availablePreparedRecipes = useMemo(
    () =>
      preparedRecipes.filter(
        (preparedRecipe) => preparedRecipe.availablePortions > 0,
      ),
    [preparedRecipes],
  )

  const displayedPreparedRecipes = showAllPreparedRecipes
    ? preparedRecipes
    : availablePreparedRecipes

  return (
    <section className="planner-page">
      <header className="planner-page__header">
        <div>
          <p className="planner-page__eyebrow">Meal planning</p>

          <h1>Planner</h1>

          <p>Select a day, plan food, and manage your prepared portions.</p>
        </div>

        <label className="planner-page__date-picker">
          <span>Selected date</span>

          <input
            type="date"
            value={selectedDate}
            onChange={(event) => {
              setSearchParams({ date: event.target.value })
            }}
          />
        </label>
      </header>

      <DailyPlanSection date={selectedDate} />

      <section
        className="available-portions"
        aria-labelledby="available-portions-heading"
      >
        <header className="available-portions__header">
          <div>
            <p className="available-portions__eyebrow">Prepared Recipes</p>

            <h2 id="available-portions-heading">
              {showAllPreparedRecipes
                ? 'All Prepared Recipes'
                : 'Available Portions'}
            </h2>

            <p>
              {showAllPreparedRecipes
                ? 'Review every preparation, including fully planned portions.'
                : 'Prepared portions that have not been assigned to a day yet.'}
            </p>
          </div>

          {!isLoading && (
            <div className="available-portions__controls">
              <button
                type="button"
                onClick={() =>
                  setShowAllPreparedRecipes((currentValue) => !currentValue)
                }
              >
                {showAllPreparedRecipes ? 'Show available' : 'Show all'}
              </button>

              <strong className="available-portions__count">
                {displayedPreparedRecipes.length}
              </strong>
            </div>
          )}
        </header>

        <ErrorList messages={errors} />

        {isLoading ? (
          <LoadingIndicator message="Loading Available Portions..." />
        ) : displayedPreparedRecipes.length === 0 ? (
          <div className="available-portions__empty">
            <h3>
              {showAllPreparedRecipes
                ? 'No Prepared Recipes'
                : 'No available portions'}
            </h3>

            <p>
              {showAllPreparedRecipes
                ? 'Prepare a Recipe to create your first preparation.'
                : 'Prepare a Recipe or reduce a planned amount to return portions here.'}
            </p>
          </div>
        ) : (
          <ul className="available-portions__list">
            {displayedPreparedRecipes.map((preparedRecipe) => (
              <li className="available-portions__item" key={preparedRecipe.id}>
                <div>
                  <h3>{preparedRecipe.name}</h3>

                  <p>Prepared {formatDate(preparedRecipe.preparedDate)}</p>
                </div>

                <div className="available-portions__amount">
                  <strong>
                    {numberFormatter.format(preparedRecipe.availablePortions)}
                  </strong>

                  <span>
                    of {numberFormatter.format(preparedRecipe.totalPortions)}{' '}
                    portions left
                  </span>
                </div>

                <dl className="available-portions__nutrition">
                  <div>
                    <dt>Calories</dt>
                    <dd>
                      {numberFormatter.format(
                        preparedRecipe.nutritionPerPortion.calories,
                      )}{' '}
                      kcal
                    </dd>
                  </div>

                  <div>
                    <dt>Protein</dt>
                    <dd>
                      {numberFormatter.format(
                        preparedRecipe.nutritionPerPortion.protein,
                      )}{' '}
                      g
                    </dd>
                  </div>

                  <div>
                    <dt>Carbs</dt>
                    <dd>
                      {numberFormatter.format(
                        preparedRecipe.nutritionPerPortion.carbohydrates,
                      )}{' '}
                      g
                    </dd>
                  </div>
                </dl>
                <Link
                  className="available-portions__details"
                  to={`/planner/prepared-recipes/${preparedRecipe.id}`}
                >
                  View Prepared Recipe
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>
    </section>
  )
}
