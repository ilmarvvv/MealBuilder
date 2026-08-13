import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi } from '../api/ingredientApi'
import type { Ingredient } from '../api/ingredientApi'
import { recipeApi } from '../api/recipeApi'
import type { RecipeInput } from '../api/recipeApi'
import LoadingIndicator from '../components/LoadingIndicator'
import RecipeForm from '../components/RecipeForm'
import './RecipeFormPage.css'

export default function CreateRecipePage() {
  const navigate = useNavigate()

  const [ingredients, setIngredients] = useState<Ingredient[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadIngredients() {
      try {
        const loadedIngredients = await ingredientApi.getAll()

        if (isActive) {
          setIngredients(loadedIngredients)
        }
      } catch (error) {
        if (isActive) {
          setErrors(getApiErrorMessages(error, 'Unable to load Ingredients.'))
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadIngredients()

    return () => {
      isActive = false
    }
  }, [])

  async function handleSubmit(input: RecipeInput) {
    setIsSubmitting(true)
    setErrors([])

    try {
      const createdRecipe = await recipeApi.create(input)

      navigate(`/library/recipes/${createdRecipe.id}`, { replace: true })
    } catch (error) {
      setErrors(getApiErrorMessages(error, 'Unable to create the Recipe.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return <LoadingIndicator message="Loading Ingredients..." />
  }

  return (
    <section className="recipe-form-page">
      <header className="recipe-form-page__header">
        <p className="recipe-form-page__eyebrow">Personal Recipe</p>

        <h2>Add Recipe</h2>

        <p>
          Combine Ingredients, add Cooking Steps, and review the calculated
          nutrition values.
        </p>
      </header>

      <RecipeForm
        ingredients={ingredients}
        errors={errors}
        isSubmitting={isSubmitting}
        submitLabel="Create Recipe"
        cancelPath="/library/recipes"
        onSubmit={handleSubmit}
      />
    </section>
  )
}
