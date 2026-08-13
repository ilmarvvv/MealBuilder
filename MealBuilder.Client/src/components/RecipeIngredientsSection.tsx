import { Link } from 'react-router'
import type { Ingredient } from '../api/ingredientApi'

export type RecipeIngredientFormRow = {
  key: string
  ingredientId: string
  grams: string
}

type RecipeIngredientsSectionProps = {
  ingredients: Ingredient[]
  rows: RecipeIngredientFormRow[]
  disabled: boolean
  onChange: (
    key: string,
    values: Partial<RecipeIngredientFormRow>,
  ) => void
  onAdd: () => void
  onRemove: (key: string) => void
  onMove: (key: string, direction: -1 | 1) => void
}

export default function RecipeIngredientsSection({
  ingredients,
  rows,
  disabled,
  onChange,
  onAdd,
  onRemove,
  onMove,
}: RecipeIngredientsSectionProps) {
  return (
    <section className="recipe-form-section">
      <header className="recipe-form-section__header">
        <div>
          <p className="recipe-form-section__eyebrow">
            Step 2
          </p>

          <h3>Ingredients</h3>

          <p>
            Select each Ingredient and enter its weight in grams.
          </p>
        </div>

        <Link
          className="recipe-form-section__secondary-link"
          to="/library/ingredients/new"
          target="_blank"
          rel="noreferrer"
        >
          Create Ingredient
        </Link>
      </header>

      <div className="recipe-form-rows">
        {rows.map((row, index) => {
          const selectedByOtherRows = new Set(
            rows
              .filter((otherRow) => otherRow.key !== row.key)
              .map((otherRow) =>
                Number(otherRow.ingredientId),
              )
              .filter((ingredientId) =>
                Number.isInteger(ingredientId),
              ),
          )

          return (
            <div
              className="recipe-form-row"
              key={row.key}
            >
              <span
                className="recipe-form-row__number"
                aria-hidden="true"
              >
                {index + 1}
              </span>

              <label className="recipe-form-field">
                <span>Ingredient</span>

                <select
                  value={row.ingredientId}
                  disabled={disabled}
                  required
                  onChange={(event) =>
                    onChange(row.key, {
                      ingredientId: event.target.value,
                    })
                  }
                >
                  <option value="">
                    Select an Ingredient
                  </option>

                  {ingredients.map((ingredient) => (
                    <option
                      key={ingredient.id}
                      value={ingredient.id}
                      disabled={selectedByOtherRows.has(
                        ingredient.id,
                      )}
                    >
                      {ingredient.name}
                      {ingredient.isBuiltIn
                        ? ' - Built-in'
                        : ' - Mine'}
                    </option>
                  ))}
                </select>
              </label>

              <label className="recipe-form-field recipe-form-field--grams">
                <span>Grams</span>

                <input
                  type="number"
                  min="0.01"
                  max="100000"
                  step="0.01"
                  inputMode="decimal"
                  value={row.grams}
                  disabled={disabled}
                  required
                  onChange={(event) =>
                    onChange(row.key, {
                      grams: event.target.value,
                    })
                  }
                />
              </label>

              <div className="recipe-form-row__actions">
                <button
                  type="button"
                  aria-label={`Move Ingredient ${index + 1} up`}
                  disabled={disabled || index === 0}
                  onClick={() => onMove(row.key, -1)}
                >
                  &uarr;
                </button>

                <button
                  type="button"
                  aria-label={`Move Ingredient ${index + 1} down`}
                  disabled={
                    disabled || index === rows.length - 1
                  }
                  onClick={() => onMove(row.key, 1)}
                >
                  &darr;
                </button>

                <button
                  type="button"
                  aria-label={`Remove Ingredient ${index + 1}`}
                  disabled={disabled || rows.length === 1}
                  onClick={() => onRemove(row.key)}
                >
                  &times;
                </button>
              </div>
            </div>
          )
        })}
      </div>

      <button
        className="recipe-form-section__add"
        type="button"
        disabled={disabled}
        onClick={onAdd}
      >
        + Add Ingredient
      </button>
    </section>
  )
}