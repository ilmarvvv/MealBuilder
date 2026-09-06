import { useId, useState } from 'react'
import type { Ingredient } from '../api/ingredientApi'

type RecipeIngredientSelectProps = {
  ingredients: Ingredient[]
  value: string
  disabled: boolean
  excludedIds: Set<number>
  onChange: (value: string) => void
}

export default function RecipeIngredientSelect({
  ingredients,
  value,
  disabled,
  excludedIds,
  onChange,
}: RecipeIngredientSelectProps) {
  const id = useId()
  const [search, setSearch] = useState('')

  const normalizedSearch = search.trim().toLocaleLowerCase()

  const matchingIngredients = ingredients.filter((ingredient) =>
    ingredient.name.toLocaleLowerCase().includes(normalizedSearch),
  )

  const visibleIngredients = ingredients.filter(
    (ingredient) =>
      String(ingredient.id) === value ||
      ingredient.name.toLocaleLowerCase().includes(normalizedSearch),
  )

  return (
    <div className="recipe-form-field">
      <label htmlFor={`${id}-search`}>Search Ingredients</label>

      <input
        id={`${id}-search`}
        type="search"
        placeholder="Type an Ingredient name"
        autoComplete="off"
        value={search}
        disabled={disabled}
        onChange={(event) => setSearch(event.target.value)}
        onKeyDown={(event) => {
          if (event.key === 'Enter') {
            event.preventDefault()
          }
        }}
      />

      <label htmlFor={`${id}-select`}>Ingredient</label>

      <select
        id={`${id}-select`}
        value={value}
        disabled={disabled}
        required
        onChange={(event) => {
          onChange(event.target.value)
          setSearch('')
        }}
      >
        <option value="">Select an Ingredient</option>

        {visibleIngredients.map((ingredient) => (
          <option
            key={ingredient.id}
            value={ingredient.id}
            disabled={excludedIds.has(ingredient.id)}
          >
            {ingredient.name}
            {ingredient.isBuiltIn ? ' - Built-in' : ' - Mine'}
          </option>
        ))}
      </select>

      <span role="status">
        {normalizedSearch && matchingIngredients.length === 0
          ? value
            ? 'No matching Ingredients. Your current selection is kept.'
            : 'No matching Ingredients.'
          : ''}
      </span>
    </div>
  )
}
