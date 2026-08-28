import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import { dailyPlanApi } from '../api/dailyPlanApi'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi, type Ingredient } from '../api/ingredientApi'
import type { DailyPlan, PreparedRecipeSummary } from '../api/mealPlanningTypes'
import { preparedRecipeApi } from '../api/preparedRecipeApi'
import type { RecipeNutrition } from '../api/recipeApi'
import DailyNutritionSummary from './DailyNutritionSummary'
import ErrorList from './ErrorList'
import LoadingIndicator from './LoadingIndicator'
import './AddFoodModal.css'

type AddFoodModalProps = {
  date: string
  isOpen: boolean
  onAdded: (dailyPlan: DailyPlan) => void
  onClose: () => void
}

type FoodSource = 'ingredients' | 'preparedRecipes'
type AddFoodStep = 'select' | 'details' | 'success'

type FoodSelection =
  | {
      kind: 'ingredient'
      value: Ingredient
    }
  | {
      kind: 'preparedRecipe'
      value: PreparedRecipeSummary
    }

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

const dateFormatter = new Intl.DateTimeFormat('en', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
  timeZone: 'UTC',
})

function formatDate(date: string) {
  return dateFormatter.format(new Date(`${date}T00:00:00Z`))
}

function getIngredientNutrition(ingredient: Ingredient): RecipeNutrition {
  return {
    calories: ingredient.caloriesPer100g,
    protein: ingredient.proteinPer100g,
    fat: ingredient.fatPer100g,
    carbohydrates: ingredient.carbohydratesPer100g,
    sugars: ingredient.sugarsPer100g,
    fiber: ingredient.fiberPer100g,
    salt: ingredient.saltPer100g,
  }
}

function scaleNutrition(
  nutrition: RecipeNutrition,
  multiplier: number,
): RecipeNutrition {
  return {
    calories: nutrition.calories * multiplier,
    protein: nutrition.protein * multiplier,
    fat: nutrition.fat * multiplier,
    carbohydrates: nutrition.carbohydrates * multiplier,
    sugars: nutrition.sugars * multiplier,
    fiber: nutrition.fiber * multiplier,
    salt: nutrition.salt * multiplier,
  }
}

export default function AddFoodModal({
  date,
  isOpen,
  onAdded,
  onClose,
}: AddFoodModalProps) {
  const dialogRef = useRef<HTMLDialogElement>(null)

  const [step, setStep] = useState<AddFoodStep>('select')
  const [source, setSource] = useState<FoodSource>('ingredients')
  const [ingredients, setIngredients] = useState<Ingredient[]>([])
  const [preparedRecipes, setPreparedRecipes] = useState<
    PreparedRecipeSummary[]
  >([])
  const [searchTerm, setSearchTerm] = useState('')
  const [selection, setSelection] = useState<FoodSelection | null>(null)
  const [amount, setAmount] = useState('')
  const [plannedTime, setPlannedTime] = useState('')
  const [lastAddedName, setLastAddedName] = useState<string | null>(null)
  const [isLoadingSources, setIsLoadingSources] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    const dialog = dialogRef.current

    if (dialog === null) {
      return
    }

    if (isOpen && !dialog.open) {
      dialog.showModal()
    }

    if (!isOpen && dialog.open) {
      dialog.close()
    }
  }, [isOpen])

  useEffect(() => {
    if (!isOpen) {
      return
    }

    let isActive = true

    async function loadFoodSources() {
      setStep('select')
      setSource('ingredients')
      setSearchTerm('')
      setSelection(null)
      setAmount('')
      setPlannedTime('')
      setLastAddedName(null)
      setErrors([])
      setIsLoadingSources(true)

      try {
        const [loadedIngredients, loadedPreparedRecipes] = await Promise.all([
          ingredientApi.getAll(),
          preparedRecipeApi.getAll(),
        ])

        if (isActive) {
          setIngredients(loadedIngredients)
          setPreparedRecipes(loadedPreparedRecipes)
        }
      } catch (error) {
        if (isActive) {
          setErrors(
            getApiErrorMessages(
              error,
              'Unable to load Ingredients and Available Portions.',
            ),
          )
        }
      } finally {
        if (isActive) {
          setIsLoadingSources(false)
        }
      }
    }

    void loadFoodSources()

    return () => {
      isActive = false
    }
  }, [isOpen])

  const normalizedSearchTerm = searchTerm.trim().toLowerCase()

  const filteredIngredients = useMemo(
    () =>
      ingredients.filter((ingredient) =>
        ingredient.name.toLowerCase().includes(normalizedSearchTerm),
      ),
    [ingredients, normalizedSearchTerm],
  )

  const filteredPreparedRecipes = useMemo(
    () =>
      preparedRecipes.filter(
        (preparedRecipe) =>
          preparedRecipe.availablePortions > 0 &&
          preparedRecipe.name.toLowerCase().includes(normalizedSearchTerm),
      ),
    [preparedRecipes, normalizedSearchTerm],
  )

  const numericAmount = Number(amount)

  const nutritionPreview = useMemo(() => {
    if (
      selection === null ||
      !Number.isFinite(numericAmount) ||
      numericAmount <= 0
    ) {
      return null
    }

    if (selection.kind === 'ingredient') {
      return scaleNutrition(
        getIngredientNutrition(selection.value),
        numericAmount / 100,
      )
    }

    return scaleNutrition(selection.value.nutritionPerPortion, numericAmount)
  }, [numericAmount, selection])

  function selectIngredient(ingredient: Ingredient) {
    setSelection({
      kind: 'ingredient',
      value: ingredient,
    })
    setAmount('100')
    setPlannedTime('')
    setErrors([])
    setStep('details')
  }

  function selectPreparedRecipe(preparedRecipe: PreparedRecipeSummary) {
    setSelection({
      kind: 'preparedRecipe',
      value: preparedRecipe,
    })
    setAmount('1')
    setPlannedTime('')
    setErrors([])
    setStep('details')
  }

  function returnToSelection() {
    setStep('select')
    setSelection(null)
    setAmount('')
    setPlannedTime('')
    setErrors([])
  }

  function addAnother() {
    setStep('select')
    setSearchTerm('')
    setSelection(null)
    setAmount('')
    setPlannedTime('')
    setLastAddedName(null)
    setErrors([])
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (
      selection === null ||
      !Number.isFinite(numericAmount) ||
      numericAmount <= 0
    ) {
      setErrors(['Enter an amount greater than zero.'])
      return
    }

    if (
      selection.kind === 'preparedRecipe' &&
      numericAmount > selection.value.availablePortions
    ) {
      setErrors([
        `Only ${numberFormatter.format(
          selection.value.availablePortions,
        )} portions are available.`,
      ])
      return
    }

    const apiPlannedTime = plannedTime === '' ? null : `${plannedTime}:00`

    setIsSubmitting(true)
    setErrors([])

    try {
      const updatedDailyPlan =
        selection.kind === 'ingredient'
          ? await dailyPlanApi.addIngredient(date, {
              ingredientId: selection.value.id,
              grams: numericAmount,
              plannedTime: apiPlannedTime,
            })
          : await dailyPlanApi.addPreparedRecipe(date, {
              preparedRecipeId: selection.value.id,
              portions: numericAmount,
              plannedTime: apiPlannedTime,
            })

      if (selection.kind === 'preparedRecipe') {
        const selectedPreparedRecipe = selection.value

        setPreparedRecipes((currentPreparedRecipes) =>
          currentPreparedRecipes.map((preparedRecipe) =>
            preparedRecipe.id === selectedPreparedRecipe.id
              ? {
                  ...preparedRecipe,
                  allocatedPortions:
                    preparedRecipe.allocatedPortions + numericAmount,
                  availablePortions: Math.max(
                    0,
                    preparedRecipe.availablePortions - numericAmount,
                  ),
                }
              : preparedRecipe,
          ),
        )
      }

      setLastAddedName(selection.value.name)
      onAdded(updatedDailyPlan)
      setStep('success')
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to add food to the Daily Plan.'),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  const selectedName = selection?.value.name ?? ''

  return (
    <dialog
      ref={dialogRef}
      className="add-food-modal"
      aria-labelledby="add-food-modal-title"
      onCancel={(event) => {
        event.preventDefault()
        onClose()
      }}
    >
      <div className="add-food-modal__layout">
        <header className="add-food-modal__header">
          <div>
            <p>Add to Daily Plan</p>
            <h2 id="add-food-modal-title">Add Food</h2>
            <span>{formatDate(date)}</span>
          </div>

          <button
            className="add-food-modal__close"
            type="button"
            aria-label="Close Add Food"
            onClick={onClose}
          >
            ×
          </button>
        </header>

        <div className="add-food-modal__body">
          <ErrorList messages={errors} />

          {step === 'select' &&
            (isLoadingSources ? (
              <LoadingIndicator message="Loading food sources..." />
            ) : (
              <>
                <div
                  className="add-food-modal__tabs"
                  role="group"
                  aria-label="Food source"
                >
                  <button
                    type="button"
                    aria-pressed={source === 'ingredients'}
                    onClick={() => {
                      setSource('ingredients')
                      setSearchTerm('')
                    }}
                  >
                    Ingredients
                  </button>

                  <button
                    type="button"
                    aria-pressed={source === 'preparedRecipes'}
                    onClick={() => {
                      setSource('preparedRecipes')
                      setSearchTerm('')
                    }}
                  >
                    Available Portions
                  </button>
                </div>

                <label className="add-food-modal__search">
                  <span>
                    Search{' '}
                    {source === 'ingredients'
                      ? 'Ingredients'
                      : 'Available Portions'}
                  </span>

                  <input
                    type="search"
                    value={searchTerm}
                    placeholder="Enter a name"
                    autoFocus
                    onChange={(event) => {
                      setSearchTerm(event.target.value)
                    }}
                  />
                </label>

                {source === 'ingredients' ? (
                  filteredIngredients.length === 0 ? (
                    <div className="add-food-modal__empty">
                      <h3>No Ingredients found</h3>
                      <p>Try a different search term.</p>
                    </div>
                  ) : (
                    <ul className="add-food-modal__results">
                      {filteredIngredients.map((ingredient) => (
                        <li key={ingredient.id}>
                          <button
                            type="button"
                            onClick={() => {
                              selectIngredient(ingredient)
                            }}
                          >
                            <span>
                              <strong>{ingredient.name}</strong>
                              <small>
                                {ingredient.isBuiltIn
                                  ? 'Built-in Ingredient'
                                  : 'Personal Ingredient'}
                              </small>
                            </span>

                            <span>
                              {numberFormatter.format(
                                ingredient.caloriesPer100g,
                              )}{' '}
                              kcal / 100 g
                            </span>
                          </button>
                        </li>
                      ))}
                    </ul>
                  )
                ) : filteredPreparedRecipes.length === 0 ? (
                  <div className="add-food-modal__empty">
                    <h3>No Available Portions</h3>
                    <p>Prepare a Recipe or return planned portions first.</p>
                  </div>
                ) : (
                  <ul className="add-food-modal__results">
                    {filteredPreparedRecipes.map((preparedRecipe) => (
                      <li key={preparedRecipe.id}>
                        <button
                          type="button"
                          onClick={() => {
                            selectPreparedRecipe(preparedRecipe)
                          }}
                        >
                          <span>
                            <strong>{preparedRecipe.name}</strong>
                            <small>Prepared Recipe</small>
                          </span>

                          <span>
                            {numberFormatter.format(
                              preparedRecipe.availablePortions,
                            )}{' '}
                            portions available
                          </span>
                        </button>
                      </li>
                    ))}
                  </ul>
                )}
              </>
            ))}

          {step === 'details' && selection !== null && (
            <form className="add-food-modal__form" onSubmit={handleSubmit}>
              <div className="add-food-modal__selection">
                <span>
                  {selection.kind === 'ingredient'
                    ? 'Ingredient'
                    : 'Prepared Recipe'}
                </span>
                <strong>{selectedName}</strong>

                {selection.kind === 'preparedRecipe' && (
                  <small>
                    {numberFormatter.format(selection.value.availablePortions)}{' '}
                    portions available
                  </small>
                )}
              </div>

              <div className="add-food-modal__fields">
                <label>
                  <span>
                    {selection.kind === 'ingredient' ? 'Grams' : 'Portions'}
                  </span>

                  <input
                    type="number"
                    min="0.01"
                    max={
                      selection.kind === 'preparedRecipe'
                        ? selection.value.availablePortions
                        : undefined
                    }
                    step="0.01"
                    required
                    value={amount}
                    onChange={(event) => {
                      setAmount(event.target.value)
                    }}
                  />
                </label>

                <label>
                  <span>Time (optional)</span>

                  <input
                    type="time"
                    value={plannedTime}
                    onChange={(event) => {
                      setPlannedTime(event.target.value)
                    }}
                  />
                </label>
              </div>

              {nutritionPreview !== null && (
                <DailyNutritionSummary
                  nutrition={nutritionPreview}
                  title="Nutrition Preview"
                />
              )}

              <div className="add-food-modal__actions">
                <button
                  type="button"
                  disabled={isSubmitting}
                  onClick={returnToSelection}
                >
                  Back
                </button>

                <button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? 'Adding...' : 'Add to Daily Plan'}
                </button>
              </div>
            </form>
          )}

          {step === 'success' && (
            <div className="add-food-modal__success">
              <p>Added successfully</p>
              <h3>{lastAddedName}</h3>
              <span>Daily Plan nutrition and items have been updated.</span>

              <div className="add-food-modal__actions">
                <button type="button" onClick={addAnother}>
                  Add Another
                </button>

                <button type="button" onClick={onClose}>
                  Done
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </dialog>
  )
}
