import { useState } from 'react'
import { useNavigate } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi } from '../api/ingredientApi'
import type { IngredientInput } from '../api/ingredientApi'
import IngredientForm from '../components/IngredientForm'
import './IngredientFormPage.css'

export default function CreateIngredientPage() {
  const navigate = useNavigate()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  async function handleSubmit(input: IngredientInput) {
    setIsSubmitting(true)
    setErrors([])

    try {
      const createdIngredient =
        await ingredientApi.create(input)

      navigate(
        `/library/ingredients/${createdIngredient.id}`,
        { replace: true },
      )
    } catch (error) {
      setErrors(
        getApiErrorMessages(
          error,
          'Unable to create the Ingredient.',
        ),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="ingredient-form-page">
      <header className="ingredient-form-page__header">
        <p className="ingredient-form-page__eyebrow">
          Personal Ingredient
        </p>

        <h2>Add Ingredient</h2>

        <p>
          Add nutrition values for 100 g of the Ingredient.
        </p>
      </header>

      <IngredientForm
        errors={errors}
        isSubmitting={isSubmitting}
        submitLabel="Create Ingredient"
        cancelTo="/library/ingredients"
        onSubmit={handleSubmit}
      />
    </section>
  )
}