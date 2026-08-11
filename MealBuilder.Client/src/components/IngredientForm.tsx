import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router'
import type { IngredientInput } from '../api/ingredientApi'
import ErrorList from './ErrorList'
import './IngredientForm.css'

type IngredientFormProps = {
  initialValues?: IngredientInput
  errors: string[]
  isSubmitting: boolean
  submitLabel: string
  cancelTo: string
  onSubmit: (input: IngredientInput) => Promise<void>
}

type NutritionFieldName = Exclude<
  keyof IngredientInput,
  'name'
>

type IngredientFormValues = {
  name: string
} & Record<NutritionFieldName, string>

type NutritionField = {
  name: NutritionFieldName
  label: string
  unit: string
  max: number
}

const nutritionFields: NutritionField[] = [
  {
    name: 'caloriesPer100g',
    label: 'Calories',
    unit: 'kcal',
    max: 900,
  },
  {
    name: 'proteinPer100g',
    label: 'Protein',
    unit: 'g',
    max: 100,
  },
  {
    name: 'carbohydratesPer100g',
    label: 'Carbohydrates',
    unit: 'g',
    max: 100,
  },
  {
    name: 'sugarsPer100g',
    label: 'Sugars',
    unit: 'g',
    max: 100,
  },
  {
    name: 'fiberPer100g',
    label: 'Fiber',
    unit: 'g',
    max: 100,
  },
  {
    name: 'fatPer100g',
    label: 'Fat',
    unit: 'g',
    max: 100,
  },
  {
    name: 'saltPer100g',
    label: 'Salt',
    unit: 'g',
    max: 100,
  },
]

function createInitialValues(
  input?: IngredientInput,
): IngredientFormValues {
  return {
    name: input?.name ?? '',
    caloriesPer100g: String(input?.caloriesPer100g ?? 0),
    proteinPer100g: String(input?.proteinPer100g ?? 0),
    carbohydratesPer100g: String(
      input?.carbohydratesPer100g ?? 0,
    ),
    sugarsPer100g: String(input?.sugarsPer100g ?? 0),
    fiberPer100g: String(input?.fiberPer100g ?? 0),
    fatPer100g: String(input?.fatPer100g ?? 0),
    saltPer100g: String(input?.saltPer100g ?? 0),
  }
}

export default function IngredientForm({
  initialValues,
  errors,
  isSubmitting,
  submitLabel,
  cancelTo,
  onSubmit,
}: IngredientFormProps) {
  const [values, setValues] = useState(() =>
    createInitialValues(initialValues),
  )
  const [clientErrors, setClientErrors] = useState<string[]>([])

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    const trimmedName = values.name.trim()
    const carbohydrates = Number(
      values.carbohydratesPer100g,
    )
    const sugars = Number(values.sugarsPer100g)
    const validationErrors: string[] = []

    if (trimmedName.length === 0) {
      validationErrors.push('Name is required.')
    }

    if (sugars > carbohydrates) {
      validationErrors.push(
        'Sugars cannot exceed carbohydrates.',
      )
    }

    setClientErrors(validationErrors)

    if (validationErrors.length > 0) {
      return
    }

    await onSubmit({
      name: trimmedName,
      caloriesPer100g: Number(values.caloriesPer100g),
      proteinPer100g: Number(values.proteinPer100g),
      carbohydratesPer100g: carbohydrates,
      sugarsPer100g: sugars,
      fiberPer100g: Number(values.fiberPer100g),
      fatPer100g: Number(values.fatPer100g),
      saltPer100g: Number(values.saltPer100g),
    })
  }

  function updateNutritionValue(
    fieldName: NutritionFieldName,
    value: string,
  ) {
    setValues((currentValues) => ({
      ...currentValues,
      [fieldName]: value,
    }))
  }

  return (
    <form className="ingredient-form" onSubmit={handleSubmit}>
      <section className="ingredient-form__section">
        <header>
          <h3>Details</h3>
          <p>Give the Ingredient a clear name.</p>
        </header>

        <label className="ingredient-form__field">
          <span>Name</span>

          <input
            name="name"
            type="text"
            required
            maxLength={100}
            autoComplete="off"
            value={values.name}
            onChange={(event) =>
              setValues((currentValues) => ({
                ...currentValues,
                name: event.target.value,
              }))
            }
          />
        </label>
      </section>

      <section className="ingredient-form__section">
        <header>
            <h3>Nutrition per 100 g</h3>
          <p>
            Values default to zero and can use up to two decimal
            places.
          </p>
        </header>

        <div className="ingredient-form__nutrition-grid">
          {nutritionFields.map((field) => (
            <label
              className="ingredient-form__field"
              key={field.name}
            >
              <span>{field.label}</span>

              <span className="ingredient-form__number-input">
                <input
                  name={field.name}
                  type="number"
                  required
                  min="0"
                  max={field.max}
                  step="0.01"
                  inputMode="decimal"
                  value={values[field.name]}
                  onChange={(event) =>
                    updateNutritionValue(
                      field.name,
                      event.target.value,
                    )
                  }
                />

                <span>{field.unit}</span>
              </span>
            </label>
          ))}
        </div>
      </section>

      <ErrorList messages={[...clientErrors, ...errors]} />

      <footer className="ingredient-form__actions">
        <Link
          className="ingredient-form__cancel"
          to={cancelTo}
        >
          Cancel
        </Link>

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : submitLabel}
        </button>
      </footer>
    </form>
  )
}