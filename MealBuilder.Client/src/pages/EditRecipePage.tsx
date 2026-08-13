import { useEffect, useState } from 'react'
import {
  Link,
  useNavigate,
  useParams,
} from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi } from '../api/ingredientApi'
import type { Ingredient } from '../api/ingredientApi'
import { recipeApi } from '../api/recipeApi'
import type {
  Recipe,
  RecipeInput,
} from '../api/recipeApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import RecipeForm from '../components/RecipeForm'
import './RecipeFormPage.css'

function toRecipeInput(recipe: Recipe): RecipeInput {
  return {
    name: recipe.name,
    description: recipe.description,
    servings: recipe.servings,
    ingredients: recipe.ingredients.map((ingredient) => ({
      ingredientId: ingredient.ingredientId,
      grams: ingredient.grams,
    })),
    steps: recipe.steps.map((step) => ({
      instruction: step.instruction,
    })),
  }
}

export default function EditRecipePage() {
  const { recipeId } = useParams()
  const navigate = useNavigate()
  const parsedRecipeId = Number(recipeId)

  const [recipe, setRecipe] = useState<Recipe | null>(null)
  const [ingredients, setIngredients] = useState<Ingredient[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadPage() {
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
        const [loadedRecipe, loadedIngredients] =
          await Promise.all([
            recipeApi.getById(parsedRecipeId),
            ingredientApi.getAll(),
          ])

        if (isActive) {
          setRecipe(loadedRecipe)
          setIngredients(loadedIngredients)
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

    void loadPage()

    return () => {
      isActive = false
    }
  }, [parsedRecipeId])

  async function handleSubmit(input: RecipeInput) {
    if (!recipe) {
      return
    }

    setIsSubmitting(true)
    setErrors([])

    try {
      const updatedRecipe = await recipeApi.update(
        recipe.id,
        input,
      )

      navigate(
        `/library/recipes/${updatedRecipe.id}`,
        { replace: true },
      )
    } catch (error) {
      setErrors(
        getApiErrorMessages(
          error,
          'Unable to update the Recipe.',
        ),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return <LoadingIndicator message="Loading Recipe..." />
  }

  if (!recipe) {
    return (
      <section className="recipe-form-page">
        <header className="recipe-form-page__header">
          <p className="recipe-form-page__eyebrow">
            Personal Recipe
          </p>

          <h2>Edit Recipe</h2>
        </header>

        <ErrorList messages={errors} />

        <Link
          className="recipe-form__cancel"
          to="/library/recipes"
        >
          Back to Recipes
        </Link>
      </section>
    )
  }

  return (
    <section className="recipe-form-page">
      <header className="recipe-form-page__header">
        <p className="recipe-form-page__eyebrow">
          Personal Recipe
        </p>

        <h2>Edit Recipe</h2>

        <p>
          Update {recipe.name}, its Ingredients, and Cooking
          Steps.
        </p>
      </header>

      <RecipeForm
        ingredients={ingredients}
        initialValues={toRecipeInput(recipe)}
        errors={errors}
        isSubmitting={isSubmitting}
        submitLabel="Save Changes"
        cancelPath={`/library/recipes/${recipe.id}`}
        onSubmit={handleSubmit}
      />
    </section>
  )
}