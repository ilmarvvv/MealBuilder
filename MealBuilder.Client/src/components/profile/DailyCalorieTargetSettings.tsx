import { useState } from 'react'
import type { FormEvent } from 'react'
import { getApiErrorMessages } from '../../api/getApiErrorMessages'
import { profileApi } from '../../api/profileApi'
import type { NutritionProfile } from '../../api/profileApi'
import ErrorList from '../ErrorList'

type DailyCalorieTargetSettingsProps = {
  profile: NutritionProfile
  onProfileChanged: (profile: NutritionProfile) => void
}

export default function DailyCalorieTargetSettings({
  profile,
  onProfileChanged,
}: DailyCalorieTargetSettingsProps) {
  const [dailyCalorieTarget, setDailyCalorieTarget] = useState(
    String(profile.dailyCalorieTarget),
  )
  const [isEditing, setIsEditing] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSaving(true)
    setErrors([])

    try {
      const updatedProfile = await profileApi.saveDailyCalorieTarget(
        Number(dailyCalorieTarget),
      )

      onProfileChanged(updatedProfile)
      setDailyCalorieTarget(String(updatedProfile.dailyCalorieTarget))
      setIsEditing(false)
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to update your calorie target.'),
      )
    } finally {
      setIsSaving(false)
    }
  }

  function handleCancel() {
    setDailyCalorieTarget(String(profile.dailyCalorieTarget))
    setErrors([])
    setIsEditing(false)
  }

  return (
    <article className="account-page__card">
      <div className="account-page__target">
        <span>Daily calorie target</span>

        {isEditing ? (
          <form className="account-page__target-form" onSubmit={handleSubmit}>
            <label>
              <input
                type="number"
                min="1000"
                max="10000"
                step="1"
                required
                value={dailyCalorieTarget}
                onChange={(event) => setDailyCalorieTarget(event.target.value)}
              />

              <span>kcal</span>
            </label>

            <div>
              <button
                className="button-secondary"
                type="button"
                disabled={isSaving}
                onClick={handleCancel}
              >
                Cancel
              </button>

              <button type="submit" disabled={isSaving}>
                {isSaving ? 'Saving...' : 'Save target'}
              </button>
            </div>
          </form>
        ) : (
          <strong>{profile.dailyCalorieTarget} kcal</strong>
        )}

        <ErrorList messages={errors} />
      </div>

      <div className="account-page__card-actions">
        <span className="account-page__badge">
          {profile.hasCalculationInputs ? 'Calculated setup' : 'Manual setup'}
        </span>

        {!isEditing && (
          <button
            className="button-secondary"
            type="button"
            onClick={() => setIsEditing(true)}
          >
            Edit target
          </button>
        )}
      </div>
    </article>
  )
}
