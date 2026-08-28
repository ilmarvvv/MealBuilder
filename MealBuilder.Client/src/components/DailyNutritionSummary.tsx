import type { RecipeNutrition } from '../api/recipeApi'
import './DailyNutritionSummary.css'

type DailyNutritionSummaryProps = {
  nutrition: RecipeNutrition
  calorieTarget?: number
  title?: string
}

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

function formatNumber(value: number) {
  return numberFormatter.format(value)
}

export default function DailyNutritionSummary({
  nutrition,
  calorieTarget,
  title = 'Nutrition',
}: DailyNutritionSummaryProps) {
  const hasCalorieTarget = calorieTarget !== undefined && calorieTarget > 0

  return (
    <section className="daily-nutrition-summary" aria-label={title}>
      <h3>{title}</h3>

      <div className="daily-nutrition-summary__calories">
        <span>Calories</span>

        <strong>
          {formatNumber(nutrition.calories)}
          {hasCalorieTarget && ` / ${formatNumber(calorieTarget)}`} kcal
        </strong>
      </div>

      {hasCalorieTarget && (
        <progress
          aria-label="Daily calorie progress"
          value={Math.min(nutrition.calories, calorieTarget)}
          max={calorieTarget}
        />
      )}

      <dl className="daily-nutrition-summary__values">
        <div>
          <dt>Protein</dt>
          <dd>{formatNumber(nutrition.protein)} g</dd>
        </div>

        <div>
          <dt>Carbohydrates / Sugars</dt>
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
