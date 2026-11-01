import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { recipeApi } from '../api/recipeApi'
import type { Recipe, RecipeNutrition } from '../api/recipeApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import PrepareRecipeForm from '../components/PrepareRecipeForm'
import RecipeNutritionSummary from '../components/RecipeNutritionSummary'
import './PrepareRecipePage.css'

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

function nutritionPerPortion(
  total: RecipeNutrition,
  portions: number,
): RecipeNutrition {
  return {
    calories: total.calories / portions,
    protein: total.protein / portions,
    fat: total.fat / portions,
    carbohydrates: total.carbohydrates / portions,
    sugars: total.sugars / portions,
    fiber: total.fiber / portions,
    salt: total.salt / portions,
  }
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

export default function PrepareRecipePage() {
  const { recipeId } = useParams()
  const parsedRecipeId = Number(recipeId)
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const requestedPlannerDate = searchParams.get('date')
  const openedFromPlanner = searchParams.get('returnTo') === 'planner'
  const plannerDate =
    openedFromPlanner && isValidDateValue(requestedPlannerDate)
      ? requestedPlannerDate
      : null
  const plannerPath =
    plannerDate === null ? null : `/planner?date=${plannerDate}`

  const [recipe, setRecipe] = useState<Recipe | null>(null)
  const [totalPortions, setTotalPortions] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadRecipe() {
      setIsLoading(true)
      setErrors([])
      setRecipe(null)
      setTotalPortions(null)

      if (!Number.isInteger(parsedRecipeId) || parsedRecipeId <= 0) {
        setErrors(['Recipe not found.'])
        setIsLoading(false)
        return
      }

      try {
        const loadedRecipe = await recipeApi.getById(parsedRecipeId)

        if (isActive) {
          setRecipe(loadedRecipe)
        }
      } catch (error) {
        if (isActive) {
          setErrors(getApiErrorMessages(error, 'Unable to load the Recipe.'))
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadRecipe()

    return () => {
      isActive = false
    }
  }, [parsedRecipeId])

  if (isLoading) {
    return <LoadingIndicator message="Loading Recipe..." />
  }

  if (errors.length > 0 || !recipe) {
    return (
      <section className="prepare-recipe-page">
        <Link to={plannerPath ?? '/library/recipes'}>
          &larr; {plannerPath === null ? 'Back to Recipes' : 'Back to Planner'}
        </Link>

        <ErrorList messages={errors} />
      </section>
    )
  }

  const recipeDetailsPath = `/library/recipes/${recipe.id}`
  const returnPath = plannerPath ?? recipeDetailsPath
  const returnLabel =
    plannerPath === null ? 'Back to Recipe' : 'Back to Planner'
  const portionCount = Number(totalPortions ?? recipe.servings)
  const perPortionNutrition =
    Number.isFinite(portionCount) && portionCount > 0
      ? nutritionPerPortion(recipe.totalNutrition, portionCount)
      : null

  return (
    <section className="prepare-recipe-page">
      <Link className="prepare-recipe-page__back" to={returnPath}>
        &larr; {returnLabel}
      </Link>

      <header className="prepare-recipe-page__header">
        <h1>Prepare recipe</h1>
      </header>

      <div className="prepare-recipe-page__layout">
        <div className="prepare-recipe-page__sidebar">
          <aside className="prepare-recipe-page__snapshot">
            <div>
              <p className="prepare-recipe-page__eyebrow">Recipe snapshot</p>

              <h2>{recipe.name}</h2>
            </div>

            <p className="prepare-recipe-page__snapshot-meta">
              {recipe.servings} recipe servings · {recipe.ingredients.length}{' '}
              ingredients
            </p>

            <section>
              <h3>Ingredients</h3>

              <ul className="prepare-recipe-page__ingredients">
                {recipe.ingredients.map((ingredient) => (
                  <li key={ingredient.ingredientId}>
                    <span>{ingredient.ingredientName}</span>

                    <strong>
                      {numberFormatter.format(ingredient.grams)} g
                    </strong>
                  </li>
                ))}
              </ul>
            </section>
          </aside>

          <section className="prepare-recipe-page__nutrition">
            <h2>Nutrition</h2>

            {perPortionNutrition === null ? (
              <p>
                Enter a valid total portion count to see nutrition per portion.
              </p>
            ) : (
              <RecipeNutritionSummary
                total={recipe.totalNutrition}
                perServing={perPortionNutrition}
                title="Nutrition"
                description="Whole recipe values and nutrition per prepared portion."
                perServingTitle="Per Portion"
                splitCarbsAndSugars
              />
            )}
          </section>
        </div>

        <PrepareRecipeForm
          recipe={recipe}
          initialDate={plannerDate ?? undefined}
          cancelPath={returnPath}
          onTotalPortionsChange={setTotalPortions}
          onPrepared={(preparedRecipe) => {
            navigate(`/planner?date=${preparedRecipe.preparedDate}`, {
              replace: true,
            })
          }}
        />
      </div>
    </section>
  )
}
