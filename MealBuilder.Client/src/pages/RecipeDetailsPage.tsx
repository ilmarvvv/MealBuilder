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
  const [
  isDeleteConfirmationOpen,
  setIsDeleteConfirmationOpen,
  ] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [deleteErrors, setDeleteErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadRecipe() {
      setIsLoading(true)
      setErrors([])
      setRecipe(null)

      if (
        !Number.isInteger(parsedRecipeId) ||
        parsedRecipeId <= 0
      ) {
        setErrors(['Recipe not found.'])
        setIsLoading(false)
        return
      }

      try {
        const loadedRecipe =
          await recipeApi.getById(parsedRecipeId)

        if (isActive) {
          setRecipe(loadedRecipe)
        }
      } catch (error) {
        if (isActive) {
          setErrors(
            getApiErrorMessages(
              error,
              'Unable to load the Recipe.',
            ),
          )
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
      getApiErrorMessages(
        error,
        'Unable to delete the Recipe.',
      ),
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
        <Link to="/library/recipes">
          &larr; Back to Recipes
        </Link>

        <ErrorList messages={errors} />
      </section>
    )
  }

  return (
    <article className="recipe-details">
      <Link
        className="recipe-details__back"
        to="/library/recipes"
      >
        &larr; Back to Recipes
      </Link>

      <header className="recipe-details__header">
        <div>
          <p className="recipe-details__eyebrow">
            Personal Recipe
          </p>

          <h2>{recipe.name}</h2>

          <p>
            {recipe.description ?? 'No description'}
          </p>
        </div>

        <div className="recipe-details__actions">
            <span className="recipe-details__servings">
                {recipe.servings}{' '}
                {recipe.servings === 1 ? 'serving' : 'servings'}
            </span>

            <Link
                className="recipe-details__edit"
                to={`/library/recipes/${recipe.id}/edit`}
            >
                Edit Recipe
            </Link>
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
        </div>
      </header>

{isDeleteConfirmationOpen && (
  <section
    id="delete-recipe-confirmation"
    className="recipe-delete-confirmation"
    role="region"
    aria-labelledby="delete-recipe-heading"
  >
    <div>
      <h3 id="delete-recipe-heading">
        Delete Recipe?
      </h3>

      <p>
        This will permanently delete {recipe.name}.
      </p>
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
        {isDeleting
          ? 'Deleting...'
          : 'Delete permanently'}
      </button>
    </div>
  </section>
)}

      <RecipeNutritionSummary
        total={recipe.totalNutrition}
        perServing={recipe.nutritionPerServing}
      />

      <div className="recipe-details__content">
        <section className="recipe-details__section">
          <h3>Ingredients</h3>

          <ol className="recipe-details__ingredients">
            {recipe.ingredients.map((ingredient) => (
              <li key={ingredient.ingredientId}>
                <Link
                  to={`/library/ingredients/${ingredient.ingredientId}`}
                >
                  {ingredient.ingredientName}
                </Link>

                <strong>
                  {numberFormatter.format(ingredient.grams)} g
                </strong>
              </li>
            ))}
          </ol>
        </section>

        <section className="recipe-details__section">
          <h3>Cooking Steps</h3>

          <ol className="recipe-details__steps">
            {recipe.steps.map((step) => (
              <li key={step.id}>
                <span>{step.position}</span>
                <p>{step.instruction}</p>
              </li>
            ))}
          </ol>
        </section>
      </div>
    </article>
  )
}