import {
  createIngredientPlanningEntry,
  type IngredientPlanningEntry,
  type IngredientPlanningMode,
} from './ingredientPlanning'
import './IngredientPlanningFields.css'

type IngredientPlanningFieldsProps = {
  selectedDate: string
  mode: IngredientPlanningMode
  entries: IngredientPlanningEntry[]
  onModeChange: (mode: IngredientPlanningMode) => void
  onEntriesChange: (entries: IngredientPlanningEntry[]) => void
}

function addDays(date: string, numberOfDays: number) {
  const result = new Date(`${date}T00:00:00Z`)

  result.setUTCDate(result.getUTCDate() + numberOfDays)

  return result.toISOString().slice(0, 10)
}

export default function IngredientPlanningFields({
  selectedDate,
  mode,
  entries,
  onModeChange,
  onEntriesChange,
}: IngredientPlanningFieldsProps) {
  function changeMode(nextMode: IngredientPlanningMode) {
    if (nextMode === mode) {
      return
    }

    if (nextMode === 'single') {
      const firstEntry =
        entries[0] ?? createIngredientPlanningEntry(selectedDate)

      onEntriesChange([
        {
          ...firstEntry,
          date: selectedDate,
        },
      ])
    } else if (entries.length === 0) {
      onEntriesChange([createIngredientPlanningEntry(selectedDate)])
    }

    onModeChange(nextMode)
  }

  function updateEntry(key: number, values: Partial<IngredientPlanningEntry>) {
    onEntriesChange(
      entries.map((entry) =>
        entry.key === key
          ? {
              ...entry,
              ...values,
            }
          : entry,
      ),
    )
  }

  function addEntry() {
    const lastEntry = entries[entries.length - 1]
    const previousDate = lastEntry?.date || selectedDate
    const previousGrams = lastEntry?.grams ?? '100'
    const previousPlannedTime = lastEntry?.plannedTime ?? ''

    onEntriesChange([
      ...entries,
      createIngredientPlanningEntry(
        addDays(previousDate, 1),
        previousGrams,
        previousPlannedTime,
      ),
    ])
  }

  function removeEntry(key: number) {
    if (entries.length === 1) {
      return
    }

    onEntriesChange(entries.filter((entry) => entry.key !== key))
  }

  const singleEntry = entries[0] ?? createIngredientPlanningEntry(selectedDate)

  return (
    <div className="ingredient-planning">
      <div
        className="ingredient-planning__mode"
        role="group"
        aria-label="Ingredient planning mode"
      >
        <button
          type="button"
          aria-pressed={mode === 'single'}
          onClick={() => {
            changeMode('single')
          }}
        >
          Single day
        </button>

        <button
          type="button"
          aria-pressed={mode === 'multiple'}
          onClick={() => {
            changeMode('multiple')
          }}
        >
          Multiple days
        </button>
      </div>

      {mode === 'single' ? (
        <div className="ingredient-planning__single">
          <label>
            <span>Grams</span>

            <input
              type="number"
              min="0.01"
              max="100000"
              step="0.01"
              required
              value={singleEntry.grams}
              onChange={(event) => {
                updateEntry(singleEntry.key, {
                  grams: event.target.value,
                })
              }}
            />
          </label>

          <label>
            <span>Time (optional)</span>

            <input
              type="time"
              value={singleEntry.plannedTime}
              onChange={(event) => {
                updateEntry(singleEntry.key, {
                  plannedTime: event.target.value,
                })
              }}
            />
          </label>
        </div>
      ) : (
        <div className="ingredient-planning__multiple">
          <div>
            <h3>Plan multiple days</h3>
            <p>Set a separate amount and optional time for each day.</p>
          </div>

          <ul className="ingredient-planning__entries">
            {entries.map((entry) => (
              <li key={entry.key}>
                <label className="ingredient-planning__date">
                  <span>Date</span>

                  <input
                    type="date"
                    required
                    value={entry.date}
                    onChange={(event) => {
                      updateEntry(entry.key, {
                        date: event.target.value,
                      })
                    }}
                  />
                </label>

                <label>
                  <span>Grams</span>

                  <input
                    type="number"
                    min="0.01"
                    max="100000"
                    step="0.01"
                    required
                    value={entry.grams}
                    onChange={(event) => {
                      updateEntry(entry.key, {
                        grams: event.target.value,
                      })
                    }}
                  />
                </label>

                <label>
                  <span>Time (optional)</span>

                  <input
                    type="time"
                    value={entry.plannedTime}
                    onChange={(event) => {
                      updateEntry(entry.key, {
                        plannedTime: event.target.value,
                      })
                    }}
                  />
                </label>

                <button
                  className="ingredient-planning__remove"
                  type="button"
                  disabled={entries.length === 1}
                  aria-label={`Remove ${entry.date}`}
                  onClick={() => {
                    removeEntry(entry.key)
                  }}
                >
                  Remove
                </button>
              </li>
            ))}
          </ul>

          <button
            className="ingredient-planning__add"
            type="button"
            onClick={addEntry}
          >
            + Add Day
          </button>
        </div>
      )}
    </div>
  )
}
