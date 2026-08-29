import { useState, type FormEvent } from 'react'
import { dailyPlanApi } from '../api/dailyPlanApi'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import {
  DailyPlanItemType,
  type DailyPlan,
  type DailyPlanItem,
  type MoveDailyPlanItemResult,
} from '../api/mealPlanningTypes'
import ErrorList from './ErrorList'
import './DailyPlanItemActions.css'

type DailyPlanItemActionsProps = {
  dailyPlanId: number
  planDate: string
  item: DailyPlanItem
  onPlanUpdated: (dailyPlan: DailyPlan) => void
  onMoved: (result: MoveDailyPlanItemResult) => void
  onRemoved: (removedItem: DailyPlanItem, updatedDailyPlan: DailyPlan) => void
  onPreparedRecipesChanged: () => void
}

type EditorMode = 'amount' | 'time' | 'move' | null

function getItemAmount(item: DailyPlanItem) {
  if (item.itemType === DailyPlanItemType.Ingredient) {
    return item.grams ?? 0
  }

  return item.portions ?? 0
}

function getAmountLabel(item: DailyPlanItem) {
  return item.itemType === DailyPlanItemType.Ingredient ? 'Grams' : 'Portions'
}

function getNextDate(date: string) {
  const nextDate = new Date(`${date}T00:00:00Z`)
  nextDate.setUTCDate(nextDate.getUTCDate() + 1)

  return nextDate.toISOString().slice(0, 10)
}

export default function DailyPlanItemActions({
  dailyPlanId,
  planDate,
  item,
  onPlanUpdated,
  onMoved,
  onRemoved,
  onPreparedRecipesChanged,
}: DailyPlanItemActionsProps) {
  const [editorMode, setEditorMode] = useState<EditorMode>(null)
  const [amount, setAmount] = useState('')
  const [plannedTime, setPlannedTime] = useState('')
  const [destinationDate, setDestinationDate] = useState('')
  const [moveAmount, setMoveAmount] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  const currentAmount = getItemAmount(item)
  const amountLabel = getAmountLabel(item)

  function openAmountEditor() {
    setAmount(String(currentAmount))
    setErrors([])
    setEditorMode('amount')
  }

  function openTimeEditor() {
    setPlannedTime(item.plannedTime?.slice(0, 5) ?? '')
    setErrors([])
    setEditorMode('time')
  }

  function openMoveEditor() {
    setDestinationDate(getNextDate(planDate))
    setMoveAmount(String(currentAmount))
    setErrors([])
    setEditorMode('move')
  }

  function closeEditor() {
    if (isSubmitting) {
      return
    }

    setEditorMode(null)
    setErrors([])
  }

  async function handleAmountSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const numericAmount = Number(amount)

    if (!Number.isFinite(numericAmount) || numericAmount <= 0) {
      setErrors(['Enter an amount greater than zero.'])
      return
    }

    setIsSubmitting(true)
    setErrors([])

    try {
      const updatedDailyPlan = await dailyPlanApi.changeItemAmount(
        dailyPlanId,
        item.id,
        {
          amount: numericAmount,
        },
      )

      onPlanUpdated(updatedDailyPlan)

      if (item.itemType === DailyPlanItemType.PreparedRecipe) {
        onPreparedRecipesChanged()
      }

      setEditorMode(null)
    } catch (error) {
      setErrors(getApiErrorMessages(error, 'Unable to change the amount.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleTimeSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const apiPlannedTime = plannedTime === '' ? null : `${plannedTime}:00`

    setIsSubmitting(true)
    setErrors([])

    try {
      const updatedDailyPlan = await dailyPlanApi.changeItemTime(
        dailyPlanId,
        item.id,
        {
          plannedTime: apiPlannedTime,
        },
      )

      onPlanUpdated(updatedDailyPlan)
      setEditorMode(null)
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to change the planned time.'),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleMoveSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const numericMoveAmount = Number(moveAmount)

    if (destinationDate === planDate) {
      setErrors(['Choose a different destination date.'])
      return
    }

    if (
      !Number.isFinite(numericMoveAmount) ||
      numericMoveAmount <= 0 ||
      numericMoveAmount > currentAmount
    ) {
      setErrors([`Enter a move amount between 0.01 and ${currentAmount}.`])
      return
    }

    setIsSubmitting(true)
    setErrors([])

    try {
      const result = await dailyPlanApi.moveItem(dailyPlanId, item.id, {
        destinationDate,
        amount: numericMoveAmount,
      })

      onMoved(result)
      setEditorMode(null)
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to move the Daily Plan item.'),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleRemove() {
    setIsSubmitting(true)
    setErrors([])

    try {
      const updatedDailyPlan = await dailyPlanApi.removeItem(
        dailyPlanId,
        item.id,
      )

      onRemoved(item, updatedDailyPlan)

      if (item.itemType === DailyPlanItemType.PreparedRecipe) {
        onPreparedRecipesChanged()
      }
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to remove the Daily Plan item.'),
      )
      setIsSubmitting(false)
    }
  }

  return (
    <div className="daily-plan-item-actions">
      <div className="daily-plan-item-actions__buttons">
        <button
          type="button"
          disabled={isSubmitting}
          onClick={openAmountEditor}
        >
          Change Amount
        </button>

        <button type="button" disabled={isSubmitting} onClick={openTimeEditor}>
          Change Time
        </button>

        <button type="button" disabled={isSubmitting} onClick={openMoveEditor}>
          Move
        </button>

        <button
          className="daily-plan-item-actions__remove"
          type="button"
          disabled={isSubmitting}
          onClick={handleRemove}
        >
          {isSubmitting && editorMode === null ? 'Removing...' : 'Remove'}
        </button>
      </div>

      <ErrorList messages={errors} />

      {editorMode === 'amount' && (
        <form
          className="daily-plan-item-actions__editor"
          onSubmit={handleAmountSubmit}
        >
          <label>
            <span>{amountLabel}</span>

            <input
              type="number"
              min="0.01"
              step="0.01"
              required
              value={amount}
              onChange={(event) => {
                setAmount(event.target.value)
              }}
            />
          </label>

          <div className="daily-plan-item-actions__editor-buttons">
            <button type="button" disabled={isSubmitting} onClick={closeEditor}>
              Cancel
            </button>

            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Saving...' : 'Save Amount'}
            </button>
          </div>
        </form>
      )}

      {editorMode === 'time' && (
        <form
          className="daily-plan-item-actions__editor"
          onSubmit={handleTimeSubmit}
        >
          <label>
            <span>Planned time (optional)</span>

            <input
              type="time"
              value={plannedTime}
              onChange={(event) => {
                setPlannedTime(event.target.value)
              }}
            />
          </label>

          <div className="daily-plan-item-actions__editor-buttons">
            <button type="button" disabled={isSubmitting} onClick={closeEditor}>
              Cancel
            </button>

            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Saving...' : 'Save Time'}
            </button>
          </div>
        </form>
      )}

      {editorMode === 'move' && (
        <form
          className="daily-plan-item-actions__editor daily-plan-item-actions__editor--move"
          onSubmit={handleMoveSubmit}
        >
          <label>
            <span>Destination date</span>

            <input
              type="date"
              required
              value={destinationDate}
              onChange={(event) => {
                setDestinationDate(event.target.value)
              }}
            />
          </label>

          <label>
            <span>{amountLabel} to move</span>

            <input
              type="number"
              min="0.01"
              max={currentAmount}
              step="0.01"
              required
              value={moveAmount}
              onChange={(event) => {
                setMoveAmount(event.target.value)
              }}
            />
          </label>

          <div className="daily-plan-item-actions__editor-buttons">
            <button type="button" disabled={isSubmitting} onClick={closeEditor}>
              Cancel
            </button>

            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Moving...' : 'Move Item'}
            </button>
          </div>
        </form>
      )}
    </div>
  )
}
