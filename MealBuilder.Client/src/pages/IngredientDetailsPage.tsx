import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi } from '../api/ingredientApi'
import type { Ingredient } from '../api/ingredientApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import './IngredientDetailsPage.css'

const nutritionNumberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

export default function IngredientDetailsPage() {
  const { ingredientId } = useParams()
  const parsedIngredientId = Number(ingredientId)

  const [ingredient, setIngredient] =
    useState<Ingredient | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadIngredient() {
      setIsLoading(true)
      setErrors([])

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

        if (isActive) {
          setIngredient(loadedIngredient)
        }
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

  if (isLoading) {
    return <LoadingIndicator message="Loading Ingredient..." />
  }

  if (errors.length > 0 || !ingredient) {
    return (
      <section className="ingredient-details">
        <Link
          className="ingredient-details__back"
          to="/library/ingredients"
        >
          ← Back to Ingredients
        </Link>

        <ErrorList messages={errors} />
      </section>
    )
  }

  return (
    <article className="ingredient-details">
      <Link
        className="ingredient-details__back"
        to="/library/ingredients"
      >
        ← Back to Ingredients
      </Link>

      <header className="ingredient-details__header">
        <div>
          <span className="ingredient-details__badge">
            {ingredient.isBuiltIn ? 'Built-in' : 'Mine'}
          </span>

          <h2>{ingredient.name}</h2>

          <p>Nutrition values per 100 g</p>
        </div>
        {!ingredient.isBuiltIn && (
        <div className="ingredient-details__actions">
            <Link
            className="ingredient-details__edit"
            to={`/library/ingredients/${ingredient.id}/edit`}
            >
            Edit Ingredient
            </Link>
        </div>
        )}
      </header>

      <section
        className="ingredient-details__nutrition"
        aria-labelledby="nutrition-heading"
      >
        <h3 id="nutrition-heading">
          Nutrition
        </h3>

        <div className="ingredient-details__calories">
          <span>Calories</span>

          <strong>
            {nutritionNumberFormatter.format(
              ingredient.caloriesPer100g,
            )}
          </strong>

          <span>kcal</span>
        </div>

        <dl className="ingredient-details__values">
          <div>
            <dt>Protein</dt>
            <dd>
              {nutritionNumberFormatter.format(
                ingredient.proteinPer100g,
              )}{' '}
              g
            </dd>
          </div>

          <div>
            <dt>Carbohydrates / Sugars</dt>
            <dd>
              {nutritionNumberFormatter.format(
                ingredient.carbohydratesPer100g,
              )}
              {' / '}
              {nutritionNumberFormatter.format(
                ingredient.sugarsPer100g,
              )}{' '}
              g
            </dd>
          </div>

          <div>
            <dt>Fiber</dt>
            <dd>
              {nutritionNumberFormatter.format(
                ingredient.fiberPer100g,
              )}{' '}
              g
            </dd>
          </div>

          <div>
            <dt>Fat</dt>
            <dd>
              {nutritionNumberFormatter.format(
                ingredient.fatPer100g,
              )}{' '}
              g
            </dd>
          </div>

          <div>
            <dt>Salt</dt>
            <dd>
              {nutritionNumberFormatter.format(
                ingredient.saltPer100g,
              )}{' '}
              g
            </dd>
          </div>
        </dl>
      </section>

      {ingredient.isBuiltIn && (
        <section className="ingredient-details__source">
          <h3>Source</h3>

          <p>
            {ingredient.sourceName ?? 'Unknown source'}
            {' · '}
            {ingredient.sourceVersion ?? 'Unknown version'}
          </p>

          <p>
            Source code:{' '}
            <strong>
              {ingredient.sourceCode ?? 'Unavailable'}
            </strong>
          </p>
        </section>
      )}
    </article>
  )
}