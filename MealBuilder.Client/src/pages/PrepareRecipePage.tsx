import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { recipeApi } from '../api/recipeApi'
import type { Recipe } from '../api/recipeApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import PrepareRecipeForm from '../components/PrepareRecipeForm'
import RecipeNutritionSummary from '../components/RecipeNutritionSummary'
import './PrepareRecipePage.css'

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

export default function PrepareRecipePage() {
  const { recipeId } = useParams()
  const parsedRecipeId = Number(recipeId)
  const navigate = useNavigate()

  const [recipe, setRecipe] = useState<Recipe | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadRecipe() {
      setIsLoading(true)
      setErrors([])
      setRecipe(null)

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
        <Link to="/library/recipes">&larr; Back to Recipes</Link>

        <ErrorList messages={errors} />
      </section>
    )
  }

  const recipeDetailsPath = `/library/recipes/${recipe.id}`

  return (
    <section className="prepare-recipe-page">
      <Link className="prepare-recipe-page__back" to={recipeDetailsPath}>
        &larr; Back to Recipe
      </Link>

      <header className="prepare-recipe-page__header">
        <p className="prepare-recipe-page__eyebrow">Prepared Recipe</p>

        <h1>Prepare {recipe.name}</h1>

        <p>
          Review the Recipe snapshot, choose the number of portions, and decide
          whether to plan them now.
        </p>
      </header>

      <div className="prepare-recipe-page__layout">
        <aside className="prepare-recipe-page__snapshot">
          <div>
            <p className="prepare-recipe-page__eyebrow">Snapshot source</p>

            <h2>{recipe.name}</h2>

            <p>
              These Recipe values will be copied into an immutable Prepared
              Recipe snapshot.
            </p>
          </div>

          <dl className="prepare-recipe-page__facts">
            <div>
              <dt>Servings</dt>
              <dd>{recipe.servings}</dd>
            </div>

            <div>
              <dt>Ingredients</dt>
              <dd>{recipe.ingredients.length}</dd>
            </div>

            <div>
              <dt>Cooking steps</dt>
              <dd>{recipe.steps.length}</dd>
            </div>
          </dl>

          <section>
            <h3>Copied Ingredients</h3>

            <ul className="prepare-recipe-page__ingredients">
              {recipe.ingredients.map((ingredient) => (
                <li key={ingredient.ingredientId}>
                  <span>{ingredient.ingredientName}</span>

                  <strong>{numberFormatter.format(ingredient.grams)} g</strong>
                </li>
              ))}
            </ul>
          </section>
        </aside>

        <PrepareRecipeForm
          recipe={recipe}
          cancelPath={recipeDetailsPath}
          onPrepared={(preparedRecipe) => {
            navigate(`/planner?date=${preparedRecipe.preparedDate}`, {
              replace: true,
            })
          }}
        />
      </div>

      <RecipeNutritionSummary
        total={recipe.totalNutrition}
        perServing={recipe.nutritionPerServing}
      />
    </section>
  )
}
