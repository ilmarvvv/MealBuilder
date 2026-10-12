import { Link } from 'react-router'
import type { Ingredient } from '../api/ingredientApi'
import RecipeIngredientSelect from './RecipeIngredientSelect'

export type RecipeIngredientFormRow = {
  key: string
  ingredientId: string
  grams: string
}

type RecipeIngredientsSectionProps = {
  ingredients: Ingredient[]
  rows: RecipeIngredientFormRow[]
  disabled: boolean
  onChange: (key: string, values: Partial<RecipeIngredientFormRow>) => void
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
          <p className="recipe-form-section__eyebrow">Step 2</p>

          <h3>Ingredients</h3>

          <p>Select each Ingredient and specify its amount.</p>
        </div>

        <Link
          className="recipe-form-section__secondary-link"
          to="/library/ingredients/new"
          target="_blank"
          rel="noreferrer"
        >
          + Create Ingredient
        </Link>
      </header>

      <div className="recipe-form-rows">
        {rows.map((row, index) => {
          const selectedByOtherRows = new Set(
            rows
              .filter((otherRow) => otherRow.key !== row.key)
              .map((otherRow) => Number(otherRow.ingredientId))
              .filter((ingredientId) => Number.isInteger(ingredientId)),
          )

          return (
            <div className="recipe-form-row" key={row.key}>
              <span className="recipe-form-row__number" aria-hidden="true">
                {index + 1}
              </span>

              <RecipeIngredientSelect
                ingredients={ingredients}
                value={row.ingredientId}
                disabled={disabled}
                excludedIds={selectedByOtherRows}
                onChange={(ingredientId) => onChange(row.key, { ingredientId })}
              />

              <div className="recipe-form-field recipe-form-field--grams">
                <div className="recipe-form-field__number-with-unit">
                  <input
                    type="number"
                    min="0.01"
                    max="100000"
                    step="0.01"
                    inputMode="decimal"
                    aria-label={`Amount for Ingredient ${index + 1} in grams`}
                    value={row.grams}
                    disabled={disabled}
                    required
                    onChange={(event) =>
                      onChange(row.key, {
                        grams: event.target.value,
                      })
                    }
                  />

                  <span className="recipe-form-field__unit" aria-hidden="true">
                    g
                  </span>
                </div>
              </div>

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
                  disabled={disabled || index === rows.length - 1}
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
        + Add another ingredient
      </button>
    </section>
  )
}
