import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { recipeApi } from '../api/recipeApi'
import type { Recipe } from '../api/recipeApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import RecipeNutritionSummary from '../components/RecipeNutritionSummary'
import './RecipeDetailsPage.css'

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

export default function RecipeDetailsPage() {
  const { recipeId } = useParams()
  const parsedRecipeId = Number(recipeId)
  const navigate = useNavigate()

  const [recipe, setRecipe] = useState<Recipe | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])
  const [isDeleteConfirmationOpen, setIsDeleteConfirmationOpen] =
    useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [deleteErrors, setDeleteErrors] = useState<string[]>([])

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

  async function handleDelete() {
    if (!recipe) {
      return
    }

    setIsDeleting(true)
    setDeleteErrors([])

    try {
      await recipeApi.remove(recipe.id)

      navigate('/library/recipes', {
        replace: true,
      })
    } catch (error) {
      setDeleteErrors(
        getApiErrorMessages(error, 'Unable to delete the Recipe.'),
      )
    } finally {
      setIsDeleting(false)
    }
  }

  if (isLoading) {
    return <LoadingIndicator message="Loading Recipe..." />
  }

  if (errors.length > 0 || !recipe) {
    return (
      <section className="recipe-details">
        <Link to="/library/recipes">&larr; Back to Recipes</Link>

        <ErrorList messages={errors} />
      </section>
    )
  }

  return (
    <article className="recipe-details">
      <Link className="recipe-details__back" to="/library/recipes">
        &larr; Back to Recipes
      </Link>

      <header className="recipe-details__header">
        <div className="recipe-details__heading">
          <p className="recipe-details__eyebrow">Personal Recipe</p>
          <h2>{recipe.name}</h2>
          <span className="recipe-details__servings">
            {recipe.servings} {recipe.servings === 1 ? 'serving' : 'servings'}
          </span>
        </div>

        <div className="recipe-details__actions">
          <Link
            className="recipe-details__prepare"
            to={`/planner/prepare/${recipe.id}`}
          >
            Prepare Recipe
          </Link>

          <Link
            className="recipe-details__edit"
            to={`/library/recipes/${recipe.id}/edit`}
          >
            Edit Recipe
          </Link>
        </div>
      </header>

      <div className="recipe-details__content">
        {recipe.description && (
          <section className="recipe-details__section recipe-details__section--description">
            <h3>Description</h3>
            <p className="recipe-details__description">{recipe.description}</p>
          </section>
        )}

        <section className="recipe-details__section recipe-details__section--ingredients">
          <div className="recipe-details__ingredients-heading">
            <h3>Ingredients</h3>
            <span>Nutrition for the amount used</span>
          </div>

          <div
            className="recipe-details__ingredients-rule"
            aria-hidden="true"
          />

          <ol className="recipe-details__ingredients">
            {recipe.ingredients.map((ingredient) => (
              <li key={ingredient.ingredientId}>
                <div className="recipe-details__ingredient-main">
                  <Link to={`/library/ingredients/${ingredient.ingredientId}`}>
                    {ingredient.ingredientName}
                  </Link>

                  <span className="recipe-details__ingredient-summary">
                    {numberFormatter.format(ingredient.grams)} g
                  </span>
                </div>

                <dl className="recipe-details__ingredient-metrics">
                  <div>
                    <dt>Calories</dt>
                    <dd>
                      {numberFormatter.format(ingredient.nutrition.calories)}{' '}
                      kcal
                    </dd>
                  </div>
                  <div>
                    <dt>Protein</dt>
                    <dd>
                      {numberFormatter.format(ingredient.nutrition.protein)} g
                    </dd>
                  </div>
                  <div>
                    <dt>Carbs</dt>
                    <dd>
                      {numberFormatter.format(
                        ingredient.nutrition.carbohydrates,
                      )}{' '}
                      g
                    </dd>
                  </div>
                  <div>
                    <dt>Sugars</dt>
                    <dd>
                      {numberFormatter.format(ingredient.nutrition.sugars)} g
                    </dd>
                  </div>
                  <div>
                    <dt>Fiber</dt>
                    <dd>
                      {numberFormatter.format(ingredient.nutrition.fiber)} g
                    </dd>
                  </div>
                  <div>
                    <dt>Fat</dt>
                    <dd>
                      {numberFormatter.format(ingredient.nutrition.fat)} g
                    </dd>
                  </div>
                  <div>
                    <dt>Salt</dt>
                    <dd>
                      {numberFormatter.format(ingredient.nutrition.salt)} g
                    </dd>
                  </div>
                </dl>
              </li>
            ))}
          </ol>
        </section>
      </div>

      <div className="recipe-details__nutrition">
        <h3>Nutrition</h3>
        <div className="recipe-details__nutrition-rule" aria-hidden="true" />

        <RecipeNutritionSummary
          total={recipe.totalNutrition}
          perServing={recipe.nutritionPerServing}
          perServingFirst
          title="Nutrition"
        />
      </div>

      <button
        className="recipe-details__delete"
        type="button"
        aria-expanded={isDeleteConfirmationOpen}
        aria-controls="delete-recipe-confirmation"
        onClick={() => {
          setDeleteErrors([])
          setIsDeleteConfirmationOpen(true)
        }}
      >
        Delete
      </button>

      {isDeleteConfirmationOpen && (
        <section
          id="delete-recipe-confirmation"
          className="recipe-delete-confirmation"
          role="region"
          aria-labelledby="delete-recipe-heading"
        >
          <div>
            <h3 id="delete-recipe-heading">Delete Recipe?</h3>
            <p>This will permanently delete {recipe.name}.</p>
          </div>

          <ErrorList messages={deleteErrors} />

          <div className="recipe-delete-confirmation__actions">
            <button
              className="recipe-delete-confirmation__cancel"
              type="button"
              disabled={isDeleting}
              onClick={() => {
                setDeleteErrors([])
                setIsDeleteConfirmationOpen(false)
              }}
            >
              Cancel
            </button>

            <button
              className="recipe-delete-confirmation__confirm"
              type="button"
              disabled={isDeleting}
              onClick={handleDelete}
            >
              {isDeleting ? 'Deleting...' : 'Delete permanently'}
            </button>
          </div>
        </section>
      )}
    </article>
  )
}
