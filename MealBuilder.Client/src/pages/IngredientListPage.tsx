import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import {
  ingredientApi,
  type Ingredient,
  type IngredientOwnershipFilter,
} from '../api/ingredientApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import LoadMoreButton from '../components/LoadMoreButton'
import './IngredientListPage.css'

type IngredientFilter = 'all' | 'built-in' | 'mine'

const pageSize = 24

const ownershipByFilter: Record<IngredientFilter, IngredientOwnershipFilter> = {
  all: 'All',
  'built-in': 'BuiltIn',
  mine: 'Mine',
}

const nutritionNumberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

export default function IngredientListPage() {
  const [ingredients, setIngredients] = useState<Ingredient[]>([])
  const [searchQuery, setSearchQuery] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [selectedFilter, setSelectedFilter] = useState<IngredientFilter>('all')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setCurrentPage(1)
      setDebouncedSearch(searchQuery)
    }, 300)

    return () => {
      window.clearTimeout(timeout)
    }
  }, [searchQuery])

  useEffect(() => {
    let isActive = true

    async function loadIngredients() {
      setIsLoading(true)
      setErrors([])

      if (currentPage === 1) {
        setIngredients([])
        setTotalCount(0)
      }

      try {
        const loadedPage = await ingredientApi.getPage({
          search: debouncedSearch,
          ownership: ownershipByFilter[selectedFilter],
          page: currentPage,
          pageSize,
        })

        if (isActive) {
          setIngredients((currentIngredients) =>
            currentPage === 1
              ? loadedPage.items
              : [...currentIngredients, ...loadedPage.items],
          )
          setTotalCount(loadedPage.totalCount)
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
  }, [currentPage, debouncedSearch, selectedFilter])

  if (isLoading && ingredients.length === 0) {
    return <LoadingIndicator message="Loading Ingredients..." />
  }

  return (
    <section className="ingredient-list">
      <header className="ingredient-list__header">
        <div>
          <h2>Ingredients</h2>
          <p>Nutrition values are shown per 100 g.</p>
        </div>
        <Link className="ingredient-list__add" to="/library/ingredients/new">
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
            onChange={(event) => setSearchQuery(event.target.value)}
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
            onClick={() => {
              setSelectedFilter('all')
              setCurrentPage(1)
            }}
          >
            All
          </button>

          <button
            className="ingredient-filter"
            type="button"
            aria-pressed={selectedFilter === 'built-in'}
            onClick={() => {
              setSelectedFilter('built-in')
              setCurrentPage(1)
            }}
          >
            Built-in
          </button>

          <button
            className="ingredient-filter"
            type="button"
            aria-pressed={selectedFilter === 'mine'}
            onClick={() => {
              setSelectedFilter('mine')
              setCurrentPage(1)
            }}
          >
            Mine
          </button>
        </div>
      </div>

      <ErrorList messages={errors} />

      {errors.length === 0 &&
        (ingredients.length === 0 ? (
          <div className="ingredient-list__empty">
            <h3>No matching Ingredients</h3>
            <p>Try another search or ownership filter.</p>
          </div>
        ) : (
          <>
            <p className="ingredient-list__result-count">
              Showing {ingredients.length} of {totalCount}{' '}
              {totalCount === 1 ? 'Ingredient' : 'Ingredients'}
            </p>

            <ul className="ingredient-grid">
              {ingredients.map((ingredient) => (
                <li key={ingredient.id}>
                  <Link
                    className="ingredient-card__link"
                    to={`/library/ingredients/${ingredient.id}`}
                  >
                    <article className="ingredient-card">
                      <header className="ingredient-card__header">
                        <h3>{ingredient.name}</h3>

                        <span className="ingredient-card__badge">
                          {ingredient.isBuiltIn ? 'Built-in' : 'Mine'}
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
            {ingredients.length < totalCount && (
              <LoadMoreButton
                isLoading={isLoading}
                onClick={() => setCurrentPage((page) => page + 1)}
              />
            )}
          </>
        ))}
    </section>
  )
}
