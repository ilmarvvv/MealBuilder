export type RecipeStepFormRow = {
  key: string
  instruction: string
}

type RecipeStepsSectionProps = {
  rows: RecipeStepFormRow[]
  disabled: boolean
  onChange: (
    key: string,
    values: Partial<RecipeStepFormRow>,
  ) => void
  onAdd: () => void
  onRemove: (key: string) => void
  onMove: (key: string, direction: -1 | 1) => void
}

export default function RecipeStepsSection({
  rows,
  disabled,
  onChange,
  onAdd,
  onRemove,
  onMove,
}: RecipeStepsSectionProps) {
  return (
    <section className="recipe-form-section">
      <header className="recipe-form-section__header">
        <div>
          <p className="recipe-form-section__eyebrow">
            Step 3
          </p>

          <h3>Cooking Steps</h3>

          <p>
            Add at least one instruction and arrange the steps
            in cooking order.
          </p>
        </div>
      </header>

      <div className="recipe-form-rows">
        {rows.map((row, index) => (
          <div
            className="recipe-form-row recipe-form-row--step"
            key={row.key}
          >
            <span
              className="recipe-form-row__number"
              aria-hidden="true"
            >
              {index + 1}
            </span>

            <label className="recipe-form-field">
              <span>Instruction</span>

              <textarea
                rows={3}
                maxLength={2000}
                value={row.instruction}
                disabled={disabled}
                required
                onChange={(event) =>
                  onChange(row.key, {
                    instruction: event.target.value,
                  })
                }
              />

              <small>
                {row.instruction.length} / 2000
              </small>
            </label>

            <div className="recipe-form-row__actions">
              <button
                type="button"
                aria-label={`Move Cooking Step ${index + 1} up`}
                disabled={disabled || index === 0}
                onClick={() => onMove(row.key, -1)}
              >
                &uarr;
              </button>

              <button
                type="button"
                aria-label={`Move Cooking Step ${index + 1} down`}
                disabled={
                  disabled || index === rows.length - 1
                }
                onClick={() => onMove(row.key, 1)}
              >
                &darr;
              </button>

              <button
                type="button"
                aria-label={`Remove Cooking Step ${index + 1}`}
                disabled={disabled || rows.length === 1}
                onClick={() => onRemove(row.key)}
              >
                &times;
              </button>
            </div>
          </div>
        ))}
      </div>

      <button
        className="recipe-form-section__add"
        type="button"
        disabled={disabled}
        onClick={onAdd}
      >
        + Add Cooking Step
      </button>
    </section>
  )
}