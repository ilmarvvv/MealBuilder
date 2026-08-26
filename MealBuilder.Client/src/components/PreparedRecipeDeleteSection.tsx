import { useState } from 'react'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { preparedRecipeApi } from '../api/preparedRecipeApi'
import type { PreparedRecipeDeletionImpact } from '../api/mealPlanningTypes'
import ErrorList from './ErrorList'
import LoadingIndicator from './LoadingIndicator'

type PreparedRecipeDeleteSectionProps = {
  preparedRecipeId: number
  preparedRecipeName: string
  onDeleted: () => void
}

export default function PreparedRecipeDeleteSection({
  preparedRecipeId,
  preparedRecipeName,
  onDeleted,
}: PreparedRecipeDeleteSectionProps) {
  const [isConfirmationOpen, setIsConfirmationOpen] = useState(false)
  const [deletionImpact, setDeletionImpact] =
    useState<PreparedRecipeDeletionImpact | null>(null)
  const [isLoadingImpact, setIsLoadingImpact] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  async function openConfirmation() {
    setIsConfirmationOpen(true)
    setDeletionImpact(null)
    setIsLoadingImpact(true)
    setErrors([])

    try {
      const loadedImpact =
        await preparedRecipeApi.getDeletionImpact(preparedRecipeId)

      setDeletionImpact(loadedImpact)
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to review the deletion impact.'),
      )
    } finally {
      setIsLoadingImpact(false)
    }
  }

  function closeConfirmation() {
    setIsConfirmationOpen(false)
    setDeletionImpact(null)
    setErrors([])
  }

  async function handleDelete() {
    setIsDeleting(true)
    setErrors([])

    try {
      await preparedRecipeApi.remove(preparedRecipeId)
      onDeleted()
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to delete the Prepared Recipe.'),
      )
      setIsDeleting(false)
    }
  }

  if (!isConfirmationOpen) {
    return (
      <section className="prepared-recipe-delete">
        <div>
          <p>Danger zone</p>
          <h2>Delete Prepared Recipe</h2>

          <p>
            Review affected Daily Plans before permanently deleting this
            preparation.
          </p>
        </div>

        <button type="button" onClick={openConfirmation}>
          Delete Prepared Recipe
        </button>
      </section>
    )
  }

  return (
    <section
      className="prepared-recipe-delete prepared-recipe-delete--confirmation"
      role="region"
      aria-labelledby="delete-prepared-recipe-heading"
    >
      <div>
        <p>Permanent deletion</p>

        <h2 id="delete-prepared-recipe-heading">
          Delete {preparedRecipeName}?
        </h2>

        <p>The original Recipe will not be deleted.</p>
      </div>

      <ErrorList messages={errors} />

      {isLoadingImpact ? (
        <LoadingIndicator message="Checking affected plans..." />
      ) : (
        deletionImpact && (
          <div className="prepared-recipe-delete__impact">
            <dl>
              <div>
                <dt>Planned items removed</dt>
                <dd>{deletionImpact.affectedItemCount}</dd>
              </div>

              <div>
                <dt>Dates affected</dt>
                <dd>{deletionImpact.affectedDateCount}</dd>
              </div>
            </dl>

            <p>
              Snapshot Ingredients and every Daily Plan item using this Prepared
              Recipe will be permanently removed. Affected daily and weekly
              nutrition totals will change.
            </p>
          </div>
        )
      )}

      <div className="prepared-recipe-delete__actions">
        <button
          className="prepared-recipe-delete__cancel"
          type="button"
          disabled={isDeleting}
          onClick={closeConfirmation}
        >
          Cancel
        </button>

        <button
          className="prepared-recipe-delete__confirm"
          type="button"
          disabled={isDeleting || isLoadingImpact || deletionImpact === null}
          onClick={handleDelete}
        >
          {isDeleting ? 'Deleting...' : 'Delete permanently'}
        </button>
      </div>
    </section>
  )
}
