import type { RecipeNutrition } from '../api/recipeApi'
import './DailyNutritionSummary.css'

type DailyNutritionSummaryProps = {
  nutrition: RecipeNutrition
  calorieTarget?: number
  title?: string
}

type CalorieStatusKind =
  | 'below'
  | 'within'
  | 'slightly-above'
  | 'noticeably-above'
  | 'significantly-above'
  | 'far-above'

type CalorieStatus = {
  kind: CalorieStatusKind
  message: string
  progressPercent: number
  overflowStartPercent: number
  overflowPercent: number
}

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

const wholeNumberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 0,
})

function formatNumber(value: number) {
  return numberFormatter.format(value)
}

function formatWholeNumber(value: number) {
  return wholeNumberFormatter.format(value)
}

function calculatePercentage(value: number, target: number) {
  return Math.min(Math.max((value / target) * 100, 0), 100)
}

function getCalorieStatus(
  calories: number,
  calorieTarget: number,
): CalorieStatus {
  const difference = calories - calorieTarget
  const progressPercent = calculatePercentage(calories, calorieTarget)
  const acceptableRangeStartPercent = calculatePercentage(
    Math.max(calorieTarget - 500, 0),
    calorieTarget,
  )

  if (difference < -500) {
    return {
      kind: 'below',
      message: `Day in progress · ~${formatWholeNumber(
        Math.abs(difference),
      )} kcal remaining`,
      progressPercent,
      overflowStartPercent: 0,
      overflowPercent: 0,
    }
  }

  if (difference <= 250) {
    let detail = 'On target'

    if (difference < 0) {
      detail = `${formatWholeNumber(Math.abs(difference))} kcal remaining`
    } else if (difference > 0) {
      detail = `+${formatWholeNumber(difference)} kcal`
    }

    return {
      kind: 'within',
      message: `Within target range · ${detail}`,
      progressPercent,
      overflowStartPercent: acceptableRangeStartPercent,
      overflowPercent: Math.max(
        progressPercent - acceptableRangeStartPercent,
        0,
      ),
    }
  }

  const overflowPercent = calculatePercentage(difference, calorieTarget)
  const overflowStartPercent = Math.max(100 - overflowPercent, 0)

  if (difference <= 500) {
    return {
      kind: 'slightly-above',
      message: `Slightly above target · +${formatWholeNumber(difference)} kcal`,
      progressPercent: 100,
      overflowStartPercent,
      overflowPercent,
    }
  }

  if (difference <= 750) {
    return {
      kind: 'noticeably-above',
      message: `Noticeably above target · +${formatWholeNumber(
        difference,
      )} kcal`,
      progressPercent: 100,
      overflowStartPercent,
      overflowPercent,
    }
  }

  if (difference < 1000) {
    return {
      kind: 'significantly-above',
      message: `Significantly above target · +${formatWholeNumber(
        difference,
      )} kcal`,
      progressPercent: 100,
      overflowStartPercent,
      overflowPercent,
    }
  }

  return {
    kind: 'far-above',
    message: `Far above target · +${formatWholeNumber(difference)} kcal`,
    progressPercent: 100,
    overflowStartPercent,
    overflowPercent,
  }
}

export default function DailyNutritionSummary({
  nutrition,
  calorieTarget,
  title = 'Nutrition',
}: DailyNutritionSummaryProps) {
  const validCalorieTarget =
    calorieTarget !== undefined && calorieTarget > 0 ? calorieTarget : undefined

  const calorieStatus =
    validCalorieTarget === undefined
      ? undefined
      : getCalorieStatus(nutrition.calories, validCalorieTarget)

  const sectionClassName =
    calorieStatus === undefined
      ? 'daily-nutrition-summary'
      : `daily-nutrition-summary daily-nutrition-summary--${calorieStatus.kind}`

  return (
    <section className={sectionClassName} aria-label={title}>
      <h3>{title}</h3>

      <div className="daily-nutrition-summary__calories">
        <span>Calories</span>

        <strong>
          <span className="daily-nutrition-summary__calorie-current">
            {formatNumber(nutrition.calories)}
          </span>

          {validCalorieTarget === undefined ? (
            ' kcal'
          ) : (
            <span className="daily-nutrition-summary__calorie-target">
              {' '}
              / {formatNumber(validCalorieTarget)} kcal
            </span>
          )}
        </strong>
      </div>

      {calorieStatus !== undefined && validCalorieTarget !== undefined && (
        <div className="daily-nutrition-summary__progress-group">
          <div
            className="daily-nutrition-summary__progress"
            role="progressbar"
            aria-label="Daily calorie progress"
            aria-valuemin={0}
            aria-valuemax={validCalorieTarget}
            aria-valuenow={Math.min(nutrition.calories, validCalorieTarget)}
            aria-valuetext={`${formatNumber(
              nutrition.calories,
            )} of ${formatNumber(validCalorieTarget)} kcal. ${
              calorieStatus.message
            }`}
          >
            <span
              className="daily-nutrition-summary__progress-base"
              style={{ width: `${calorieStatus.progressPercent}%` }}
            />

            {calorieStatus.overflowPercent > 0 && (
              <span
                className="daily-nutrition-summary__progress-overflow"
                style={{
                  left: `${calorieStatus.overflowStartPercent}%`,
                  width: `${calorieStatus.overflowPercent}%`,
                }}
              />
            )}
          </div>

          <p className="daily-nutrition-summary__status">
            {calorieStatus.message}
          </p>
        </div>
      )}

      <dl className="daily-nutrition-summary__values">
        <div>
          <dt>Protein</dt>
          <dd>{formatNumber(nutrition.protein)} g</dd>
        </div>

        <div>
          <dt>Carbs / Sugars</dt>
          <dd>
            {formatNumber(nutrition.carbohydrates)} /{' '}
            {formatNumber(nutrition.sugars)} g
          </dd>
        </div>

        <div>
          <dt>Fiber</dt>
          <dd>{formatNumber(nutrition.fiber)} g</dd>
        </div>

        <div>
          <dt>Fat</dt>
          <dd>{formatNumber(nutrition.fat)} g</dd>
        </div>

        <div>
          <dt>Salt</dt>
          <dd>{formatNumber(nutrition.salt)} g</dd>
        </div>
      </dl>
    </section>
  )
}
