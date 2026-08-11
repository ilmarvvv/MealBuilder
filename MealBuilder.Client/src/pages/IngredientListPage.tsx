import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi } from '../api/ingredientApi'
import type { Ingredient } from '../api/ingredientApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import './IngredientListPage.css'

type IngredientFilter = 'all' | 'built-in' | 'mine'

const nutritionNumberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

export default function IngredientListPage() {
  const [ingredients, setIngredients] = useState<Ingredient[]>([])
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedFilter, setSelectedFilter] =
    useState<IngredientFilter>('all')
  const [isLoading, setIsLoading] = useState(true)
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
          setErrors(
            getApiErrorMessages(
              error,
              'Unable to load Ingredients. Please try again.',
            ),
          )
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

  const visibleIngredients = useMemo(() => {
    const normalizedQuery = searchQuery.trim().toLowerCase()

    return ingredients.filter((ingredient) => {
      const matchesSearch = ingredient.name
        .toLowerCase()
        .includes(normalizedQuery)

      const matchesFilter =
        selectedFilter === 'all' ||
        (selectedFilter === 'built-in' &&
          ingredient.isBuiltIn) ||
        (selectedFilter === 'mine' &&
          !ingredient.isBuiltIn)

      return matchesSearch && matchesFilter
    })
  }, [ingredients, searchQuery, selectedFilter])

  if (isLoading) {
    return <LoadingIndicator message="Loading Ingredients..." />
  }

  return (
    <section className="ingredient-list">
      <header className="ingredient-list__header">
        <div>
          <h2>Ingredients</h2>
          <p>
            Nutrition values are shown per 100 g.
          </p>
        </div>
        <Link
            className="ingredient-list__add"
            to="/library/ingredients/new"
            >
            + Add Ingredient
        </Link>
      </header>

      <div className="ingredient-list__controls">
        <label className="ingredient-search">
          <span>Search Ingredients</span>

          <input
            type="search"
            placeholder="Search by name..."
            value={searchQuery}
            onChange={(event) =>
              setSearchQuery(event.target.value)
            }
          />
        </label>

        <div
          className="ingredient-filters"
          role="group"
          aria-label="Filter Ingredients by ownership"
        >
          <button
            className="ingredient-filter"
            type="button"
            aria-pressed={selectedFilter === 'all'}
            onClick={() => setSelectedFilter('all')}
          >
            All
          </button>

          <button
            className="ingredient-filter"
            type="button"
            aria-pressed={selectedFilter === 'built-in'}
            onClick={() => setSelectedFilter('built-in')}
          >
            Built-in
          </button>

          <button
            className="ingredient-filter"
            type="button"
            aria-pressed={selectedFilter === 'mine'}
            onClick={() => setSelectedFilter('mine')}
          >
            Mine
          </button>
        </div>
      </div>

      <ErrorList messages={errors} />

      {errors.length === 0 &&
        (visibleIngredients.length === 0 ? (
          <div className="ingredient-list__empty">
            <h3>No matching Ingredients</h3>
            <p>
              Try another search or ownership filter.
            </p>
          </div>
        ) : (
          <>
            <p className="ingredient-list__result-count">
              {visibleIngredients.length}{' '}
              {visibleIngredients.length === 1
                ? 'Ingredient'
                : 'Ingredients'}
            </p>

            <ul className="ingredient-grid">
              {visibleIngredients.map((ingredient) => (
                <li key={ingredient.id}>
                    <Link
                        className="ingredient-card__link"
                        to={`/library/ingredients/${ingredient.id}`}
                    >
                        <article className="ingredient-card">
                    <header className="ingredient-card__header">
                      <h3>{ingredient.name}</h3>

                      <span className="ingredient-card__badge">
                        {ingredient.isBuiltIn
                          ? 'Built-in'
                          : 'Mine'}
                      </span>
                    </header>

                    <p className="ingredient-card__calories">
                      <strong>
                        {nutritionNumberFormatter.format(
                          ingredient.caloriesPer100g,
                        )}
                      </strong>{' '}
                      kcal
                    </p>

                    <dl className="ingredient-card__macros">
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
                        <dt>Carbohydrates</dt>
                        <dd>
                          {nutritionNumberFormatter.format(
                            ingredient.carbohydratesPer100g,
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
                    </dl>
                  </article>
                  </Link>
                </li>
              ))}
            </ul>
          </>
        ))}
    </section>
  )
}