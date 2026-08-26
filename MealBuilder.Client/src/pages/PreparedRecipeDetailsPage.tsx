import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { preparedRecipeApi } from '../api/preparedRecipeApi'
import type { PreparedRecipe } from '../api/mealPlanningTypes'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import RecipeNutritionSummary from '../components/RecipeNutritionSummary'
import PreparedRecipeDeleteSection from '../components/PreparedRecipeDeleteSection'
import './PreparedRecipeDetailsPage.css'

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

export default function PreparedRecipeDetailsPage() {
  const { preparedRecipeId } = useParams()
  const parsedPreparedRecipeId = Number(preparedRecipeId)
  const navigate = useNavigate()

  const [preparedRecipe, setPreparedRecipe] = useState<PreparedRecipe | null>(
    null,
  )
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadPreparedRecipe() {
      setIsLoading(true)
      setErrors([])
      setPreparedRecipe(null)

      if (
        !Number.isInteger(parsedPreparedRecipeId) ||
        parsedPreparedRecipeId <= 0
      ) {
        setErrors(['Prepared Recipe not found.'])
        setIsLoading(false)
        return
      }

      try {
        const loadedPreparedRecipe = await preparedRecipeApi.getById(
          parsedPreparedRecipeId,
        )

        if (isActive) {
          setPreparedRecipe(loadedPreparedRecipe)
        }
      } catch (error) {
        if (isActive) {
          setErrors(
            getApiErrorMessages(error, 'Unable to load the Prepared Recipe.'),
          )
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadPreparedRecipe()

    return () => {
      isActive = false
    }
  }, [parsedPreparedRecipeId])

  if (isLoading) {
    return <LoadingIndicator message="Loading Prepared Recipe..." />
  }

  if (errors.length > 0 || !preparedRecipe) {
    return (
      <section className="prepared-recipe-details">
        <Link to="/planner">&larr; Back to Planner</Link>

        <ErrorList messages={errors} />
      </section>
    )
  }

  return (
    <article className="prepared-recipe-details">
      <Link className="prepared-recipe-details__back" to="/planner">
        &larr; Back to Planner
      </Link>

      <header className="prepared-recipe-details__header">
        <div>
          <p className="prepared-recipe-details__eyebrow">Prepared Recipe</p>

          <h1>{preparedRecipe.name}</h1>

          <p>Prepared {formatDate(preparedRecipe.preparedDate)}</p>
        </div>

        {preparedRecipe.sourceRecipeId === null ? (
          <span className="prepared-recipe-details__source-status">
            Original recipe deleted
          </span>
        ) : (
          <Link
            className="prepared-recipe-details__source-link"
            to={`/library/recipes/${preparedRecipe.sourceRecipeId}`}
          >
            View original Recipe
          </Link>
        )}
      </header>

      <section
        className="prepared-recipe-availability"
        aria-labelledby="prepared-recipe-availability-heading"
      >
        <header>
          <div>
            <p className="prepared-recipe-details__eyebrow">
              Portion inventory
            </p>

            <h2 id="prepared-recipe-availability-heading">
              Available Portions
            </h2>
          </div>

          <strong>
            {numberFormatter.format(preparedRecipe.availablePortions)} left
          </strong>
        </header>

        <dl>
          <div>
            <dt>Total portions</dt>
            <dd>{numberFormatter.format(preparedRecipe.totalPortions)}</dd>
          </div>

          <div>
            <dt>Planned portions</dt>
            <dd>{numberFormatter.format(preparedRecipe.allocatedPortions)}</dd>
          </div>

          <div>
            <dt>Available portions</dt>
            <dd>{numberFormatter.format(preparedRecipe.availablePortions)}</dd>
          </div>
        </dl>
      </section>

      <RecipeNutritionSummary
        total={preparedRecipe.totalNutrition}
        perServing={preparedRecipe.nutritionPerPortion}
        eyebrow="Immutable snapshot"
        title="Snapshot Nutrition"
        description="Calculated from the copied Ingredient values stored when this Recipe was prepared."
        totalTitle="Whole Preparation"
        perServingTitle="Per Portion"
      />

      <section className="prepared-recipe-snapshot">
        <header>
          <div>
            <p className="prepared-recipe-details__eyebrow">Immutable copy</p>

            <h2>Snapshot Ingredients</h2>
          </div>

          <span>
            {preparedRecipe.ingredients.length}{' '}
            {preparedRecipe.ingredients.length === 1
              ? 'Ingredient'
              : 'Ingredients'}
          </span>
        </header>

        <ol>
          {preparedRecipe.ingredients.map((ingredient) => (
            <li key={ingredient.id}>
              <span>{ingredient.position}</span>

              <div>
                <strong>{ingredient.name}</strong>

                <small>
                  {numberFormatter.format(ingredient.nutrition.calories)} kcal
                </small>
              </div>

              <strong>{numberFormatter.format(ingredient.grams)} g</strong>
            </li>
          ))}
        </ol>
      </section>
      <PreparedRecipeDeleteSection
        preparedRecipeId={preparedRecipe.id}
        preparedRecipeName={preparedRecipe.name}
        onDeleted={() => {
          navigate('/planner', { replace: true })
        }}
      />
    </article>
  )
}
