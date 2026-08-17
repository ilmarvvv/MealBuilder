import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router'
import { getApiErrorMessages } from '../../api/getApiErrorMessages'
import { profileApi } from '../../api/profileApi'
import { useAuth } from '../../auth/useAuth'
import ErrorList from '../ErrorList'

type ManualTargetSetupProps = {
  onBack: () => void
}

export default function ManualTargetSetup({ onBack }: ManualTargetSetupProps) {
  const navigate = useNavigate()
  const { refreshUser } = useAuth()
  const [dailyCalorieTarget, setDailyCalorieTarget] = useState('2200')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setErrors([])

    try {
      await profileApi.saveDailyCalorieTarget(Number(dailyCalorieTarget))

      await refreshUser()
      navigate('/', { replace: true })
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to save your daily calorie target.'),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="onboarding-card">
      <p className="onboarding-card__eyebrow">Manual setup</p>

      <h2>Choose your daily target</h2>

      <p className="onboarding-card__description">
        Enter the calorie target you already use. You can calculate or change it
        later in Account.
      </p>

      <form className="onboarding-form" onSubmit={handleSubmit}>
        <label
          className="onboarding-form__field"
          htmlFor="daily-calorie-target"
        >
          <span>Daily calorie target</span>

          <span className="onboarding-form__input-unit">
            <input
              id="daily-calorie-target"
              name="dailyCalorieTarget"
              type="number"
              min="1000"
              max="10000"
              step="1"
              required
              value={dailyCalorieTarget}
              onChange={(event) => setDailyCalorieTarget(event.target.value)}
            />

            <span>kcal</span>
          </span>
        </label>

        <ErrorList messages={errors} />

        <div className="onboarding-actions">
          <button
            className="button-secondary"
            type="button"
            disabled={isSubmitting}
            onClick={onBack}
          >
            Back
          </button>

          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'Save target'}
          </button>
        </div>
      </form>
    </section>
  )
}
