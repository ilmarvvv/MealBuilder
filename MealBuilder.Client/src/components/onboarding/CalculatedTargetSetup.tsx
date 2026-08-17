import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router'
import { getApiErrorMessages } from '../../api/getApiErrorMessages'
import {
  ActivityLevel,
  CalculationSex,
  profileApi,
  WeightGoal,
} from '../../api/profileApi'
import type {
  CalorieTargetCalculationInput,
  CalorieTargetEstimate,
} from '../../api/profileApi'
import { useAuth } from '../../auth/useAuth'
import ErrorList from '../ErrorList'

type CalculatedTargetSetupProps = {
  onBack: () => void
}

type SetupStep = 1 | 2 | 3

export default function CalculatedTargetSetup({
  onBack,
}: CalculatedTargetSetupProps) {
  const navigate = useNavigate()
  const { refreshUser } = useAuth()

  const [step, setStep] = useState<SetupStep>(1)
  const [birthDate, setBirthDate] = useState('')
  const [sexForCalculation, setSexForCalculation] = useState<CalculationSex>(
    CalculationSex.Female,
  )
  const [heightCm, setHeightCm] = useState('')
  const [weightKg, setWeightKg] = useState('')
  const [activityLevel, setActivityLevel] = useState<ActivityLevel>(
    ActivityLevel.ModeratelyActive,
  )
  const [weightGoal, setWeightGoal] = useState<WeightGoal>(
    WeightGoal.MaintainWeight,
  )
  const [estimate, setEstimate] = useState<CalorieTargetEstimate | null>(null)
  const [dailyCalorieTarget, setDailyCalorieTarget] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<string[]>([])

  function createCalculationInput(): CalorieTargetCalculationInput {
    return {
      birthDate,
      sexForCalculation,
      heightCm: Number(heightCm),
      weightKg: Number(weightKg),
      activityLevel,
      weightGoal,
    }
  }

  function handleBodyInformationSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrors([])
    setStep(2)
  }

  async function handleCalculationSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setErrors([])

    try {
      const calculatedEstimate = await profileApi.calculateTarget(
        createCalculationInput(),
      )

      setEstimate(calculatedEstimate)
      setDailyCalorieTarget(
        String(calculatedEstimate.recommendedDailyCalorieTarget),
      )
      setStep(3)
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to calculate your calorie target.'),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleConfirmationSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setErrors([])

    try {
      await profileApi.saveCalculated({
        dailyCalorieTarget: Number(dailyCalorieTarget),
        calculationInputs: createCalculationInput(),
      })

      await refreshUser()
      navigate('/', { replace: true })
    } catch (error) {
      setErrors(
        getApiErrorMessages(error, 'Unable to save your nutrition profile.'),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="onboarding-card">
      <p className="onboarding-card__eyebrow">
        Calculated setup · Step {step} of 3
      </p>

      {step === 1 && (
        <>
          <h2>Body information</h2>

          <p className="onboarding-card__description">
            These values are used only to estimate your daily calorie target.
          </p>

          <form
            className="onboarding-form"
            onSubmit={handleBodyInformationSubmit}
          >
            <div className="onboarding-form__grid">
              <label className="onboarding-form__field" htmlFor="birth-date">
                <span>Birth date</span>

                <input
                  id="birth-date"
                  name="birthDate"
                  type="date"
                  required
                  value={birthDate}
                  onChange={(event) => setBirthDate(event.target.value)}
                />
              </label>

              <label
                className="onboarding-form__field"
                htmlFor="calculation-sex"
              >
                <span>Sex for calculation</span>

                <select
                  id="calculation-sex"
                  name="sexForCalculation"
                  value={sexForCalculation}
                  onChange={(event) =>
                    setSexForCalculation(
                      Number(event.target.value) as CalculationSex,
                    )
                  }
                >
                  <option value={CalculationSex.Female}>Female</option>
                  <option value={CalculationSex.Male}>Male</option>
                </select>
              </label>

              <label className="onboarding-form__field" htmlFor="height-cm">
                <span>Height</span>

                <span className="onboarding-form__input-unit">
                  <input
                    id="height-cm"
                    name="heightCm"
                    type="number"
                    min="100"
                    max="250"
                    step="0.1"
                    required
                    value={heightCm}
                    onChange={(event) => setHeightCm(event.target.value)}
                  />

                  <span>cm</span>
                </span>
              </label>

              <label className="onboarding-form__field" htmlFor="weight-kg">
                <span>Weight</span>

                <span className="onboarding-form__input-unit">
                  <input
                    id="weight-kg"
                    name="weightKg"
                    type="number"
                    min="30"
                    max="400"
                    step="0.1"
                    required
                    value={weightKg}
                    onChange={(event) => setWeightKg(event.target.value)}
                  />

                  <span>kg</span>
                </span>
              </label>
            </div>

            <ErrorList messages={errors} />

            <div className="onboarding-actions">
              <button
                className="button-secondary"
                type="button"
                onClick={onBack}
              >
                Back
              </button>

              <button type="submit">Continue</button>
            </div>
          </form>
        </>
      )}

      {step === 2 && (
        <>
          <h2>Activity and goal</h2>

          <p className="onboarding-card__description">
            Choose the options that best describe your usual activity and
            current goal.
          </p>

          <form className="onboarding-form" onSubmit={handleCalculationSubmit}>
            <label className="onboarding-form__field" htmlFor="activity-level">
              <span>Activity level</span>

              <select
                id="activity-level"
                name="activityLevel"
                value={activityLevel}
                onChange={(event) =>
                  setActivityLevel(Number(event.target.value) as ActivityLevel)
                }
              >
                <option value={ActivityLevel.LowActive}>Low active</option>
                <option value={ActivityLevel.ModeratelyActive}>
                  Moderately active
                </option>
                <option value={ActivityLevel.Active}>Active</option>
                <option value={ActivityLevel.VeryActive}>Very active</option>
              </select>
            </label>

            <label className="onboarding-form__field" htmlFor="weight-goal">
              <span>Weight goal</span>

              <select
                id="weight-goal"
                name="weightGoal"
                value={weightGoal}
                onChange={(event) =>
                  setWeightGoal(Number(event.target.value) as WeightGoal)
                }
              >
                <option value={WeightGoal.LoseWeight}>Lose weight</option>
                <option value={WeightGoal.MaintainWeight}>
                  Maintain weight
                </option>
                <option value={WeightGoal.GainWeight}>Gain weight</option>
              </select>
            </label>

            <ErrorList messages={errors} />

            <div className="onboarding-actions">
              <button
                className="button-secondary"
                type="button"
                disabled={isSubmitting}
                onClick={() => setStep(1)}
              >
                Back
              </button>

              <button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Calculating...' : 'Calculate target'}
              </button>
            </div>
          </form>
        </>
      )}

      {step === 3 && estimate && (
        <>
          <h2>Daily target</h2>

          <p className="onboarding-card__description">
            Review the estimate and confirm the target you want to use.
          </p>

          <div className="onboarding-estimate">
            <div>
              <span>Resting energy</span>
              <strong>{estimate.restingEnergyExpenditure} kcal</strong>
            </div>

            <div>
              <span>Maintenance</span>
              <strong>{estimate.maintenanceCalories} kcal</strong>
            </div>

            <div>
              <span>Recommendation</span>
              <strong>{estimate.recommendedDailyCalorieTarget} kcal</strong>
            </div>
          </div>

          <form className="onboarding-form" onSubmit={handleConfirmationSubmit}>
            <label
              className="onboarding-form__field"
              htmlFor="calculated-daily-target"
            >
              <span>Confirmed daily target</span>

              <span className="onboarding-form__input-unit">
                <input
                  id="calculated-daily-target"
                  name="dailyCalorieTarget"
                  type="number"
                  min="1000"
                  max="10000"
                  step="1"
                  required
                  value={dailyCalorieTarget}
                  onChange={(event) =>
                    setDailyCalorieTarget(event.target.value)
                  }
                />

                <span>kcal</span>
              </span>
            </label>

            <p className="onboarding-disclaimer">
              This result is an estimate, not medical advice.
            </p>

            <ErrorList messages={errors} />

            <div className="onboarding-actions">
              <button
                className="button-secondary"
                type="button"
                disabled={isSubmitting}
                onClick={() => setStep(2)}
              >
                Back
              </button>

              <button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : 'Confirm target'}
              </button>
            </div>
          </form>
        </>
      )}
    </section>
  )
}
