import { useEffect, useId, useMemo, useRef, useState } from 'react'
import type { Ingredient } from '../api/ingredientApi'

type RecipeIngredientSelectProps = {
  ingredients: Ingredient[]
  value: string
  disabled: boolean
  excludedIds: Set<number>
  onChange: (value: string) => void
}

const maximumVisibleIngredients = 50

const calorieFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

function formatCalories(value: number) {
  return calorieFormatter.format(value)
}

export default function RecipeIngredientSelect({
  ingredients,
  value,
  disabled,
  excludedIds,
  onChange,
}: RecipeIngredientSelectProps) {
  const id = useId()
  const triggerRef = useRef<HTMLButtonElement>(null)
  const searchInputRef = useRef<HTMLInputElement>(null)

  const [isOpen, setIsOpen] = useState(false)
  const [search, setSearch] = useState('')

  const selectedIngredient = ingredients.find(
    (ingredient) => String(ingredient.id) === value,
  )

  const matchingIngredients = useMemo(() => {
    const normalizedSearch = search.trim().toLocaleLowerCase()

    return ingredients.filter((ingredient) => {
      const isSelected = String(ingredient.id) === value
      const isAvailable = isSelected || !excludedIds.has(ingredient.id)

      return (
        isAvailable &&
        ingredient.name.toLocaleLowerCase().includes(normalizedSearch)
      )
    })
  }, [excludedIds, ingredients, search, value])

  const visibleIngredients = matchingIngredients.slice(
    0,
    maximumVisibleIngredients,
  )

  useEffect(() => {
    if (isOpen) {
      searchInputRef.current?.focus()
    }
  }, [isOpen])

  function openDropdown() {
    setSearch('')
    setIsOpen(true)
  }

  function closeDropdown(restoreFocus = false) {
    setIsOpen(false)
    setSearch('')

    if (restoreFocus) {
      triggerRef.current?.focus()
    }
  }

  function selectIngredient(ingredientId: number) {
    onChange(String(ingredientId))
    closeDropdown(true)
  }

  return (
    <div
      className="recipe-ingredient-select"
      onBlur={(event) => {
        const nextTarget = event.relatedTarget

        if (
          !(nextTarget instanceof Node) ||
          !event.currentTarget.contains(nextTarget)
        ) {
          closeDropdown()
        }
      }}
    >
      <button
        ref={triggerRef}
        className="recipe-ingredient-select__trigger"
        type="button"
        aria-haspopup="listbox"
        aria-expanded={isOpen}
        aria-controls={`${id}-listbox`}
        disabled={disabled}
        onClick={() => {
          if (isOpen) {
            closeDropdown()
          } else {
            openDropdown()
          }
        }}
        onKeyDown={(event) => {
          if (event.key === 'ArrowDown') {
            event.preventDefault()
            openDropdown()
          }
        }}
      >
        <span
          className={
            selectedIngredient
              ? 'recipe-ingredient-select__value'
              : 'recipe-ingredient-select__placeholder'
          }
        >
          {selectedIngredient?.name ?? 'Select an Ingredient'}
        </span>

        <span className="recipe-ingredient-select__chevron" aria-hidden="true">
          ▾
        </span>
      </button>

      {isOpen && (
        <div className="recipe-ingredient-select__dropdown">
          <input
            ref={searchInputRef}
            className="recipe-ingredient-select__search"
            type="search"
            placeholder="Search Ingredients"
            aria-label="Search Ingredients"
            autoComplete="off"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === 'Escape') {
                event.preventDefault()
                closeDropdown(true)
              }

              if (event.key === 'Enter') {
                event.preventDefault()
              }
            }}
          />

          <div
            className="recipe-ingredient-select__options"
            id={`${id}-listbox`}
            role="listbox"
            aria-label="Ingredients"
          >
            {visibleIngredients.map((ingredient) => {
              const isSelected = String(ingredient.id) === value

              return (
                <button
                  className={
                    isSelected
                      ? 'recipe-ingredient-select__option recipe-ingredient-select__option--selected'
                      : 'recipe-ingredient-select__option'
                  }
                  type="button"
                  role="option"
                  aria-selected={isSelected}
                  key={ingredient.id}
                  onClick={() => selectIngredient(ingredient.id)}
                >
                  <span>
                    <strong>{ingredient.name}</strong>

                    <small>
                      {ingredient.isBuiltIn
                        ? 'Built-in Ingredient'
                        : 'Personal Ingredient'}
                    </small>
                  </span>

                  <span className="recipe-ingredient-select__option-details">
                    <strong className="recipe-ingredient-select__calories">
                      {formatCalories(ingredient.caloriesPer100g)} kcal
                    </strong>

                    {isSelected && (
                      <span
                        className="recipe-ingredient-select__check"
                        aria-hidden="true"
                      >
                        ✓
                      </span>
                    )}
                  </span>
                </button>
              )
            })}
          </div>

          {matchingIngredients.length > 0 && (
            <footer className="recipe-ingredient-select__footer">
              <span>Nutrition per 100 g</span>

              <span>
                {matchingIngredients.length === 1
                  ? '1 result'
                  : `${matchingIngredients.length} results`}
              </span>
            </footer>
          )}

          {matchingIngredients.length === 0 && (
            <p className="recipe-ingredient-select__message" role="status">
              No matching Ingredients.
            </p>
          )}

          {matchingIngredients.length > maximumVisibleIngredients && (
            <p className="recipe-ingredient-select__message">
              Type more characters to narrow the results.
            </p>
          )}
        </div>
      )}
    </div>
  )
}
