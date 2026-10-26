import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router'
import type { Ingredient } from '../api/ingredientApi'
import type { RecipeIngredientInput, RecipeInput } from '../api/recipeApi'
import { calculateRecipeNutrition } from '../utils/recipeNutrition'
import ErrorList from './ErrorList'
import RecipeIngredientsSection from './RecipeIngredientsSection'
import type { RecipeIngredientFormRow } from './RecipeIngredientsSection'
import RecipeNutritionSummary from './RecipeNutritionSummary'
import './RecipeForm.css'

type RecipeFormProps = {
  ingredients: Ingredient[]
  initialValues?: RecipeInput
  errors?: string[]
  isSubmitting: boolean
  submitLabel: string
  cancelPath: string
  onSubmit: (input: RecipeInput) => Promise<void>
}

let nextRowKey = 0

function createRowKey(prefix: string) {
  nextRowKey += 1
  return `${prefix}-${nextRowKey}`
}

function createIngredientRows(
  initialValues?: RecipeInput,
): RecipeIngredientFormRow[] {
  if (!initialValues || initialValues.ingredients.length === 0) {
    return [
      {
        key: createRowKey('ingredient'),
        ingredientId: '',
        grams: '',
      },
    ]
  }

  return initialValues.ingredients.map((ingredient) => ({
    key: createRowKey('ingredient'),
    ingredientId: String(ingredient.ingredientId),
    grams: String(ingredient.grams),
  }))
}

function moveRow<T extends { key: string }>(
  rows: T[],
  key: string,
  direction: -1 | 1,
) {
  const currentIndex = rows.findIndex((row) => row.key === key)
  const nextIndex = currentIndex + direction

  if (currentIndex < 0 || nextIndex < 0 || nextIndex >= rows.length) {
    return rows
  }

  const reorderedRows = [...rows]

  ;[reorderedRows[currentIndex], reorderedRows[nextIndex]] = [
    reorderedRows[nextIndex],
    reorderedRows[currentIndex],
  ]

  return reorderedRows
}

export default function RecipeForm({
  ingredients,
  initialValues,
  errors = [],
  isSubmitting,
  submitLabel,
  cancelPath,
  onSubmit,
}: RecipeFormProps) {
  const [name, setName] = useState(initialValues?.name ?? '')
  const [description, setDescription] = useState(
    initialValues?.description ?? '',
  )
  const [servings, setServings] = useState(String(initialValues?.servings ?? 1))
  const [ingredientRows, setIngredientRows] = useState(() =>
    createIngredientRows(initialValues),
  )
  const [validationErrors, setValidationErrors] = useState<string[]>([])

  const nutrition = useMemo(
    () =>
      calculateRecipeNutrition(
        ingredientRows.map((row) => ({
          ingredientId:
            row.ingredientId === '' ? null : Number(row.ingredientId),
          grams: Number(row.grams),
        })),
        ingredients,
        Number(servings),
      ),
    [ingredientRows, ingredients, servings],
  )

  function updateIngredientRow(
    key: string,
    values: Partial<RecipeIngredientFormRow>,
  ) {
    setIngredientRows((currentRows) =>
      currentRows.map((row) =>
        row.key === key
          ? {
              ...row,
              ...values,
            }
          : row,
      ),
    )
  }

  function buildInput(): RecipeInput | null {
    const nextErrors: string[] = []
    const normalizedName = name.trim()
    const normalizedDescription = description.trim()
    const parsedServings = Number(servings)

    if (normalizedName.length === 0) {
      nextErrors.push('Recipe name is required.')
    } else if (normalizedName.length > 100) {
      nextErrors.push('Recipe name cannot exceed 100 characters.')
    }

    if (normalizedDescription.length > 2000) {
      nextErrors.push('Recipe description cannot exceed 2000 characters.')
    }

    if (
      !Number.isInteger(parsedServings) ||
      parsedServings < 1 ||
      parsedServings > 100
    ) {
      nextErrors.push('Servings must be a whole number between 1 and 100.')
    }

    const recipeIngredients: RecipeIngredientInput[] = []
    const selectedIngredientIds = new Set<number>()

    for (const row of ingredientRows) {
      const ingredientId = Number(row.ingredientId)
      const grams = Number(row.grams)

      if (!Number.isInteger(ingredientId) || ingredientId <= 0) {
        nextErrors.push('Every Ingredient row must select an Ingredient.')
        continue
      }

      if (!Number.isFinite(grams) || grams <= 0 || grams > 100000) {
        nextErrors.push(
          'Ingredient grams must be greater than 0 and no more than 100000.',
        )
        continue
      }

      if (selectedIngredientIds.has(ingredientId)) {
        nextErrors.push('An Ingredient can only be added once.')
        continue
      }

      selectedIngredientIds.add(ingredientId)

      recipeIngredients.push({
        ingredientId,
        grams,
      })
    }

    setValidationErrors([...new Set(nextErrors)])

    if (nextErrors.length > 0) {
      return null
    }

    return {
      name: normalizedName,
      description:
        normalizedDescription.length === 0 ? null : normalizedDescription,
      servings: parsedServings,
      ingredients: recipeIngredients,
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const input = buildInput()

    if (!input) {
      return
    }

    await onSubmit(input)
  }

  return (
    <form
      className="recipe-form"
      noValidate
      onSubmit={(event) => void handleSubmit(event)}
    >
      <ErrorList messages={[...validationErrors, ...errors]} />

      <section className="recipe-form-section">
        <header className="recipe-form-section__header">
          <div>
            <p className="recipe-form-section__eyebrow">Step 1</p>

            <h3>Details</h3>

            <p>
              Give the Recipe a clear name and define its number of servings.
            </p>
          </div>
        </header>

        <div className="recipe-form-details">
          <label className="recipe-form-field">
            <span>Name</span>

            <input
              type="text"
              maxLength={100}
              value={name}
              disabled={isSubmitting}
              required
              onChange={(event) => setName(event.target.value)}
            />
          </label>

          <label className="recipe-form-field">
            <span className="recipe-form-field__label">
              <span>Description</span>
              <small>Optional</small>
            </span>

            <textarea
              rows={8}
              maxLength={2000}
              placeholder="A short description or preparation notes..."
              value={description}
              disabled={isSubmitting}
              onChange={(event) => setDescription(event.target.value)}
            />

            <small>{description.length} / 2000</small>
          </label>

          <label className="recipe-form-field recipe-form-field--servings">
            <span>Servings</span>

            <input
              type="number"
              min="1"
              max="100"
              step="1"
              inputMode="numeric"
              value={servings}
              disabled={isSubmitting}
              required
              onChange={(event) => setServings(event.target.value)}
            />
          </label>
        </div>
      </section>

      <RecipeIngredientsSection
        ingredients={ingredients}
        rows={ingredientRows}
        disabled={isSubmitting}
        onChange={updateIngredientRow}
        onAdd={() =>
          setIngredientRows((currentRows) => [
            ...currentRows,
            {
              key: createRowKey('ingredient'),
              ingredientId: '',
              grams: '',
            },
          ])
        }
        onRemove={(key) =>
          setIngredientRows((currentRows) =>
            currentRows.length === 1
              ? currentRows
              : currentRows.filter((row) => row.key !== key),
          )
        }
        onMove={(key, direction) =>
          setIngredientRows((currentRows) =>
            moveRow(currentRows, key, direction),
          )
        }
      />

      <RecipeNutritionSummary
        total={nutrition.total}
        perServing={nutrition.perServing}
      />

      <footer className="recipe-form__actions">
        <Link className="recipe-form__cancel" to={cancelPath}>
          Cancel
        </Link>

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : submitLabel}
        </button>
      </footer>
    </form>
  )
}
