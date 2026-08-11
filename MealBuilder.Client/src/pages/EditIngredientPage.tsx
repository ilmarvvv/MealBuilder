import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi } from '../api/ingredientApi'
import type {
  Ingredient,
  IngredientInput,
} from '../api/ingredientApi'
import ErrorList from '../components/ErrorList'
import IngredientForm from '../components/IngredientForm'
import LoadingIndicator from '../components/LoadingIndicator'
import './IngredientFormPage.css'

function toIngredientInput(
  ingredient: Ingredient,
): IngredientInput {
  return {
    name: ingredient.name,
    caloriesPer100g: ingredient.caloriesPer100g,
    proteinPer100g: ingredient.proteinPer100g,
    carbohydratesPer100g:
      ingredient.carbohydratesPer100g,
    sugarsPer100g: ingredient.sugarsPer100g,
    fiberPer100g: ingredient.fiberPer100g,
    fatPer100g: ingredient.fatPer100g,
    saltPer100g: ingredient.saltPer100g,
  }
}

export default function EditIngredientPage() {
  const { ingredientId } = useParams()
  const navigate = useNavigate()
  const parsedIngredientId = Number(ingredientId)

  const [ingredient, setIngredient] =
    useState<Ingredient | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadIngredient() {
      setIsLoading(true)
      setErrors([])
      setIngredient(null)

      if (
        !Number.isInteger(parsedIngredientId) ||
        parsedIngredientId <= 0
      ) {
        setErrors(['Ingredient not found.'])
        setIsLoading(false)
        return
      }

      try {
        const loadedIngredient =
          await ingredientApi.getById(parsedIngredientId)

        if (!isActive) {
          return
        }

        if (loadedIngredient.isBuiltIn) {
          setErrors([
            'Built-in Ingredients cannot be edited.',
          ])
          return
        }

        setIngredient(loadedIngredient)
      } catch (error) {
        if (isActive) {
          setErrors(
            getApiErrorMessages(
              error,
              'Unable to load the Ingredient.',
            ),
          )
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadIngredient()

    return () => {
      isActive = false
    }
  }, [parsedIngredientId])

  async function handleSubmit(input: IngredientInput) {
    if (!ingredient) {
      return
    }

    setIsSubmitting(true)
    setErrors([])

    try {
      const updatedIngredient = await ingredientApi.update(
        ingredient.id,
        input,
      )

      navigate(
        `/library/ingredients/${updatedIngredient.id}`,
        { replace: true },
      )
    } catch (error) {
      setErrors(
        getApiErrorMessages(
          error,
          'Unable to update the Ingredient.',
        ),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return <LoadingIndicator message="Loading Ingredient..." />
  }

  if (!ingredient) {
    return (
      <section className="ingredient-form-page">
        <header className="ingredient-form-page__header">
          <p className="ingredient-form-page__eyebrow">
            Personal Ingredient
          </p>

          <h2>Edit Ingredient</h2>
        </header>

        <ErrorList messages={errors} />

        <Link
          className="ingredient-form__cancel"
          to="/library/ingredients"
        >
          Back to Ingredients
        </Link>
      </section>
    )
  }

  return (
    <section className="ingredient-form-page">
      <header className="ingredient-form-page__header">
        <p className="ingredient-form-page__eyebrow">
          Personal Ingredient
        </p>

        <h2>Edit Ingredient</h2>

        <p>
          Update {ingredient.name} nutrition values per 100 g.
        </p>
      </header>

      <IngredientForm
        initialValues={toIngredientInput(ingredient)}
        errors={errors}
        isSubmitting={isSubmitting}
        submitLabel="Save Changes"
        cancelTo={`/library/ingredients/${ingredient.id}`}
        onSubmit={handleSubmit}
      />
    </section>
  )
}