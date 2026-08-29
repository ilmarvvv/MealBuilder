import ErrorList from './ErrorList'
import './DailyPlanUndoNotice.css'

type DailyPlanUndoNoticeProps = {
  itemName: string
  isUndoing: boolean
  errors: string[]
  onUndo: () => void
  onDismiss: () => void
}

export default function DailyPlanUndoNotice({
  itemName,
  isUndoing,
  errors,
  onUndo,
  onDismiss,
}: DailyPlanUndoNoticeProps) {
  return (
    <section className="daily-plan-undo" role="status" aria-live="polite">
      <div className="daily-plan-undo__message">
        <p>Item removed</p>
        <strong>{itemName}</strong>
        <span>Undo is available for 5 seconds.</span>
      </div>

      <ErrorList messages={errors} />

      <div className="daily-plan-undo__actions">
        <button type="button" disabled={isUndoing} onClick={onDismiss}>
          Dismiss
        </button>

        <button type="button" disabled={isUndoing} onClick={onUndo}>
          {isUndoing ? 'Restoring...' : 'Undo'}
        </button>
      </div>
    </section>
  )
}
