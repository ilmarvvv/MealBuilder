import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { recipeApi } from '../api/recipeApi'
import type { RecipeSummary } from '../api/recipeApi'
import ErrorList from '../components/ErrorList'
import LoadingIndicator from '../components/LoadingIndicator'
import './RecipeListPage.css'

const nutritionNumberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

export default function RecipeListPage() {
  const [recipes, setRecipes] = useState<RecipeSummary[]>([])
  const [searchQuery, setSearchQuery] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let isActive = true

    async function loadRecipes() {
      try {
        const loadedRecipes = await recipeApi.getAll()

        if (isActive) {
          setRecipes(loadedRecipes)
        }
      } catch (error) {
        if (isActive) {
          setErrors(
            getApiErrorMessages(
              error,
              'Unable to load Recipes. Please try again.',
            ),
          )
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadRecipes()

    return () => {
      isActive = false
    }
  }, [])

  const visibleRecipes = useMemo(() => {
    const normalizedQuery = searchQuery.trim().toLowerCase()

    return recipes.filter((recipe) => {
      const searchableText = [
        recipe.name,
        recipe.description ?? '',
      ]
        .join(' ')
        .toLowerCase()

      return searchableText.includes(normalizedQuery)
    })
  }, [recipes, searchQuery])

  if (isLoading) {
    return <LoadingIndicator message="Loading Recipes..." />
  }

  return (
    <section className="recipe-list">
      <header className="recipe-list__header">
        <div>
          <h2>Recipes</h2>
          <p>
            Create reusable Recipes with Ingredients and Cooking Steps.
          </p>
        </div>

        <Link
          className="recipe-list__add"
          to="/library/recipes/new"
        >
          + Add Recipe
        </Link>
      </header>

      <div className="recipe-list__controls">
        <label className="recipe-search">
          <span>Search Recipes</span>

          <input
            type="search"
            placeholder="Search by name or description..."
            value={searchQuery}
            onChange={(event) =>
              setSearchQuery(event.target.value)
            }
          />
        </label>
      </div>

      <ErrorList messages={errors} />

      {errors.length === 0 &&
        (visibleRecipes.length === 0 ? (
          <div className="recipe-list__empty">
            <h3>
              {recipes.length === 0
                ? 'No Recipes yet'
                : 'No matching Recipes'}
            </h3>

            <p>
              {recipes.length === 0
                ? 'Create your first Recipe to reuse it in your meal plan.'
                : 'Try another search.'}
            </p>
          </div>
        ) : (
          <>
            <p className="recipe-list__result-count">
              {visibleRecipes.length}{' '}
              {visibleRecipes.length === 1
                ? 'Recipe'
                : 'Recipes'}
            </p>

            <ul className="recipe-grid">
              {visibleRecipes.map((recipe) => (
                <li key={recipe.id}>
                  <Link
                    className="recipe-card__link"
                    to={`/library/recipes/${recipe.id}`}
                  >
                    <article className="recipe-card">
                      <header className="recipe-card__header">
                        <div>
                          <h3>{recipe.name}</h3>

                          <p>
                            {recipe.description ??
                              'No description'}
                          </p>
                        </div>

                        <span className="recipe-card__badge">
                          {recipe.servings}{' '}
                          {recipe.servings === 1
                            ? 'serving'
                            : 'servings'}
                        </span>
                      </header>

                      <p className="recipe-card__calories">
                        <strong>
                          {nutritionNumberFormatter.format(
                            recipe.nutritionPerServing.calories,
                          )}
                        </strong>{' '}
                        kcal per serving
                      </p>

                      <dl className="recipe-card__macros">
                        <div>
                          <dt>Protein</dt>
                          <dd>
                            {nutritionNumberFormatter.format(
                              recipe.nutritionPerServing.protein,
                            )}{' '}
                            g
                          </dd>
                        </div>

                        <div>
                          <dt>Carbohydrates</dt>
                          <dd>
                            {nutritionNumberFormatter.format(
                              recipe.nutritionPerServing
                                .carbohydrates,
                            )}{' '}
                            g
                          </dd>
                        </div>

                        <div>
                          <dt>Fat</dt>
                          <dd>
                            {nutritionNumberFormatter.format(
                              recipe.nutritionPerServing.fat,
                            )}{' '}
                            g
                          </dd>
                        </div>
                      </dl>

                      <p className="recipe-card__ingredient-count">
                        {recipe.ingredientCount}{' '}
                        {recipe.ingredientCount === 1
                          ? 'Ingredient'
                          : 'Ingredients'}
                      </p>
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