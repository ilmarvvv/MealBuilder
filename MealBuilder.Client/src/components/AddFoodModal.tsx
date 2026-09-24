import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import { dailyPlanApi } from '../api/dailyPlanApi'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi, type Ingredient } from '../api/ingredientApi'
import type { DailyPlan, PreparedRecipeSummary } from '../api/mealPlanningTypes'
import { preparedRecipeApi } from '../api/preparedRecipeApi'
import {
  recipeApi,
  type RecipeNutrition,
  type RecipeSummary,
} from '../api/recipeApi'
import IngredientPlanningFields from './IngredientPlanningFields'
import {
  createIngredientPlanningEntry,
  type IngredientPlanningEntry,
  type IngredientPlanningMode,
} from './ingredientPlanning'
import { useNavigate } from 'react-router'
import DailyNutritionSummary from './DailyNutritionSummary'
import ErrorList from './ErrorList'
import LoadingIndicator from './LoadingIndicator'
import './AddFoodModal.css'

type AddFoodModalProps = {
  date: string
  isOpen: boolean
  onAdded: (dailyPlans: DailyPlan[]) => void
  onClose: () => void
}

type FoodSource = 'ingredients' | 'recipes' | 'preparedRecipes'
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
  const navigate = useNavigate()

  const [step, setStep] = useState<AddFoodStep>('select')
  const [source, setSource] = useState<FoodSource>('ingredients')
  const [ingredients, setIngredients] = useState<Ingredient[]>([])
  const [recipes, setRecipes] = useState<RecipeSummary[]>([])
  const [preparedRecipes, setPreparedRecipes] = useState<
    PreparedRecipeSummary[]
  >([])
  const [searchTerm, setSearchTerm] = useState('')
  const [selection, setSelection] = useState<FoodSelection | null>(null)
  const [amount, setAmount] = useState('')
  const [plannedTime, setPlannedTime] = useState('')
  const [ingredientPlanningMode, setIngredientPlanningMode] =
    useState<IngredientPlanningMode>('single')
  const [ingredientPlanningEntries, setIngredientPlanningEntries] = useState<
    IngredientPlanningEntry[]
  >([])
  const [lastAddedDayCount, setLastAddedDayCount] = useState(1)
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
      setIngredientPlanningMode('single')
      setIngredientPlanningEntries([])
      setLastAddedDayCount(1)
      setLastAddedName(null)
      setErrors([])
      setIsLoadingSources(true)

      try {
        const [loadedIngredients, loadedRecipes, loadedPreparedRecipes] =
          await Promise.all([
            ingredientApi.getAll(),
            recipeApi.getAll(),
            preparedRecipeApi.getAll(),
          ])

        if (isActive) {
          setIngredients(loadedIngredients)
          setRecipes(loadedRecipes)
          setPreparedRecipes(loadedPreparedRecipes)
        }
      } catch (error) {
        if (isActive) {
          setErrors(getApiErrorMessages(error, 'Unable to load food sources.'))
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

  const filteredRecipes = useMemo(
    () =>
      recipes.filter((recipe) =>
        [recipe.name, recipe.description ?? '']
          .join(' ')
          .toLowerCase()
          .includes(normalizedSearchTerm),
      ),
    [recipes, normalizedSearchTerm],
  )

  const numericAmount = Number(amount)

  const nutritionPreview = useMemo(() => {
    if (selection === null) {
      return null
    }

    if (selection.kind === 'ingredient') {
      if (ingredientPlanningMode === 'multiple') {
        return null
      }

      const grams = Number(ingredientPlanningEntries[0]?.grams)

      if (!Number.isFinite(grams) || grams <= 0) {
        return null
      }

      return scaleNutrition(
        getIngredientNutrition(selection.value),
        grams / 100,
      )
    }

    if (!Number.isFinite(numericAmount) || numericAmount <= 0) {
      return null
    }

    return scaleNutrition(selection.value.nutritionPerPortion, numericAmount)
  }, [
    ingredientPlanningEntries,
    ingredientPlanningMode,
    numericAmount,
    selection,
  ])

  function prepareRecipe(recipeId: number) {
    onClose()

    const searchParams = new URLSearchParams({
      date,
      returnTo: 'planner',
    })

    navigate(`/planner/prepare/${recipeId}?${searchParams.toString()}`)
  }

  function selectIngredient(ingredient: Ingredient) {
    setSelection({
      kind: 'ingredient',
      value: ingredient,
    })
    setIngredientPlanningMode('single')
    setIngredientPlanningEntries([createIngredientPlanningEntry(date)])
    setAmount('')
    setPlannedTime('')
    setErrors([])
    setStep('details')
  }

  function selectPreparedRecipe(preparedRecipe: PreparedRecipeSummary) {
    setSelection({
      kind: 'preparedRecipe',
      value: preparedRecipe,
    })
    setIngredientPlanningMode('single')
    setIngredientPlanningEntries([])
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
    setIngredientPlanningMode('single')
    setIngredientPlanningEntries([])
    setErrors([])
  }

  function addAnother() {
    setStep('select')
    setSearchTerm('')
    setSelection(null)
    setAmount('')
    setLastAddedDayCount(1)
    setPlannedTime('')
    setIngredientPlanningMode('single')
    setIngredientPlanningEntries([])
    setLastAddedName(null)
    setErrors([])
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (selection === null) {
      setErrors(['Select food before continuing.'])
      return
    }

    const parsedIngredientEntries = ingredientPlanningEntries.map((entry) => ({
      date: entry.date,
      grams: Number(entry.grams),
      plannedTime: entry.plannedTime === '' ? null : `${entry.plannedTime}:00`,
    }))

    if (selection.kind === 'ingredient') {
      if (
        parsedIngredientEntries.length === 0 ||
        parsedIngredientEntries.some(
          (entry) =>
            entry.date === '' ||
            !Number.isFinite(entry.grams) ||
            entry.grams <= 0 ||
            entry.grams > 100000,
        )
      ) {
        setErrors([
          'Enter a valid date and an amount between 0.01 and 100000 grams for every day.',
        ])
        return
      }

      const uniqueDates = new Set(
        parsedIngredientEntries.map((entry) => entry.date),
      )

      if (uniqueDates.size !== parsedIngredientEntries.length) {
        setErrors(['Each date can only appear once.'])
        return
      }
    } else {
      if (!Number.isFinite(numericAmount) || numericAmount <= 0) {
        setErrors(['Enter an amount greater than zero.'])
        return
      }

      if (numericAmount > selection.value.availablePortions) {
        setErrors([
          `Only ${numberFormatter.format(
            selection.value.availablePortions,
          )} portions are available.`,
        ])
        return
      }
    }

    setIsSubmitting(true)
    setErrors([])

    try {
      let updatedDailyPlans: DailyPlan[]

      if (selection.kind === 'ingredient') {
        if (ingredientPlanningMode === 'multiple') {
          updatedDailyPlans = await dailyPlanApi.addIngredientBatch({
            ingredientId: selection.value.id,
            entries: parsedIngredientEntries,
          })
        } else {
          const entry = parsedIngredientEntries[0]

          const updatedDailyPlan = await dailyPlanApi.addIngredient(
            entry.date,
            {
              ingredientId: selection.value.id,
              grams: entry.grams,
              plannedTime: entry.plannedTime,
            },
          )

          updatedDailyPlans = [updatedDailyPlan]
        }
      } else {
        const apiPlannedTime = plannedTime === '' ? null : `${plannedTime}:00`

        const updatedDailyPlan = await dailyPlanApi.addPreparedRecipe(date, {
          preparedRecipeId: selection.value.id,
          portions: numericAmount,
          plannedTime: apiPlannedTime,
        })

        updatedDailyPlans = [updatedDailyPlan]

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
      setLastAddedDayCount(updatedDailyPlans.length)
      onAdded(updatedDailyPlans)
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
                    aria-pressed={source === 'recipes'}
                    onClick={() => {
                      setSource('recipes')
                      setSearchTerm('')
                    }}
                  >
                    Recipes
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
                      : source === 'recipes'
                        ? 'Recipes'
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
                ) : source === 'recipes' ? (
                  filteredRecipes.length === 0 ? (
                    <div className="add-food-modal__empty">
                      <h3>No Recipes found</h3>
                      <p>Try a different search term or create a Recipe.</p>
                    </div>
                  ) : (
                    <ul className="add-food-modal__results">
                      {filteredRecipes.map((recipe) => (
                        <li key={recipe.id}>
                          <button
                            type="button"
                            onClick={() => {
                              prepareRecipe(recipe.id)
                            }}
                          >
                            <span>
                              <strong>{recipe.name}</strong>
                              <small>
                                {recipe.servings}{' '}
                                {recipe.servings === 1 ? 'serving' : 'servings'}{' '}
                                · Select to prepare
                              </small>
                            </span>

                            <span>
                              {numberFormatter.format(
                                recipe.nutritionPerServing.calories,
                              )}{' '}
                              kcal / serving
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

              {selection.kind === 'ingredient' ? (
                <IngredientPlanningFields
                  selectedDate={date}
                  mode={ingredientPlanningMode}
                  entries={ingredientPlanningEntries}
                  onModeChange={setIngredientPlanningMode}
                  onEntriesChange={setIngredientPlanningEntries}
                />
              ) : (
                <div className="add-food-modal__fields">
                  <label>
                    <span>Portions</span>

                    <input
                      type="number"
                      min="0.01"
                      max={selection.value.availablePortions}
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
              )}

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
                  {isSubmitting
                    ? 'Adding...'
                    : selection.kind === 'ingredient' &&
                        ingredientPlanningMode === 'multiple'
                      ? `Add to ${ingredientPlanningEntries.length} ${
                          ingredientPlanningEntries.length === 1
                            ? 'Day'
                            : 'Days'
                        }`
                      : 'Add to Daily Plan'}
                </button>
              </div>
            </form>
          )}

          {step === 'success' && (
            <div className="add-food-modal__success">
              <p>Added successfully</p>
              <h3>{lastAddedName}</h3>
              <span>
                {lastAddedDayCount === 1
                  ? 'Daily Plan nutrition and items have been updated.'
                  : `Added to ${lastAddedDayCount} days. Weekly planning has been updated.`}
              </span>

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
