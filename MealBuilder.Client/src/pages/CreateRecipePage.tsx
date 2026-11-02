import { useEffect, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi } from '../api/ingredientApi'
import type { Ingredient } from '../api/ingredientApi'
import { recipeApi } from '../api/recipeApi'
import type { Recipe, RecipeInput } from '../api/recipeApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import RecipeForm from '../components/RecipeForm'
import useIngredientRefresh from '../hooks/useIngredientRefresh'
import './RecipeFormPage.css'

export default function CreateRecipePage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const copyFromValue = searchParams.get('copyFrom')
  const sourceRecipeId = Number(copyFromValue)
  const isCopying = copyFromValue !== null

  const [sourceRecipe, setSourceRecipe] = useState<Recipe | null>(null)
  const [ingredients, setIngredients] = useState<Ingredient[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  const ingredientRefreshErrors = useIngredientRefresh({
    enabled:
      !isLoading && !isSubmitting && (!isCopying || sourceRecipe !== null),
    onRefreshed: setIngredients,
  })

  useEffect(() => {
    let isActive = true

    async function loadPage() {
      setIsLoading(true)
      setErrors([])
      setSourceRecipe(null)

      if (
        isCopying &&
        (!Number.isInteger(sourceRecipeId) || sourceRecipeId <= 0)
      ) {
        setErrors(['Recipe not found.'])
        setIsLoading(false)
        return
      }

      try {
        const [loadedIngredients, loadedRecipe] = await Promise.all([
          ingredientApi.getAll(),
          isCopying ? recipeApi.getById(sourceRecipeId) : Promise.resolve(null),
        ])

        if (isActive) {
          setIngredients(loadedIngredients)
          setSourceRecipe(loadedRecipe)
        }
      } catch (error) {
        if (isActive) {
          setErrors(
            getApiErrorMessages(
              error,
              isCopying
                ? 'Unable to load the Recipe to copy.'
                : 'Unable to load Ingredients.',
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
  }, [isCopying, sourceRecipeId])

  async function handleSubmit(input: RecipeInput) {
    setIsSubmitting(true)
    setErrors([])

    try {
      const createdRecipe = await recipeApi.create(input)

      navigate(`/library/recipes/${createdRecipe.id}`, { replace: true })
    } catch (error) {
      setErrors(
        getApiErrorMessages(
          error,
          isCopying
            ? 'Unable to create the Recipe copy.'
            : 'Unable to create the Recipe.',
        ),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return <LoadingIndicator message="Loading Ingredients..." />
  }

  if (isCopying && sourceRecipe === null) {
    return (
      <section className="recipe-form-page">
        <ErrorList messages={errors} />

        <Link to="/library/recipes">Back to Recipes</Link>
      </section>
    )
  }

  return (
    <section className="recipe-form-page">
      <header className="recipe-form-page__header">
        <p className="recipe-form-page__eyebrow">Personal Recipe</p>

        <h2>{isCopying ? 'Copy Recipe' : 'Add Recipe'}</h2>

        <p>
          {isCopying
            ? 'Change at least one detail to create a new Recipe. The original stays unchanged.'
            : 'Add Ingredients, add a Description if you like, and review nutrition.'}
        </p>
      </header>

      <RecipeForm
        key={sourceRecipe?.id ?? 'new'}
        ingredients={ingredients}
        initialValues={sourceRecipe ?? undefined}
        requireChanges={isCopying}
        errors={[...errors, ...ingredientRefreshErrors]}
        isSubmitting={isSubmitting}
        submitLabel={isCopying ? 'Create Copy' : 'Create Recipe'}
        cancelPath={
          sourceRecipe
            ? `/library/recipes/${sourceRecipe.id}`
            : '/library/recipes'
        }
        onSubmit={handleSubmit}
      />
    </section>
  )
}
