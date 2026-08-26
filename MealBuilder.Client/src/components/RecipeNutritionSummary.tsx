import type { RecipeNutrition } from '../api/recipeApi'
import './RecipeNutritionSummary.css'

type RecipeNutritionSummaryProps = {
  total: RecipeNutrition
  perServing: RecipeNutrition
  eyebrow?: string
  title?: string
  description?: string
  totalTitle?: string
  perServingTitle?: string
}

type NutritionPanelProps = {
  title: string
  nutrition: RecipeNutrition
}

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

function formatNumber(value: number) {
  return numberFormatter.format(value)
}

function NutritionPanel({ title, nutrition }: NutritionPanelProps) {
  return (
    <article className="recipe-nutrition-panel">
      <h4>{title}</h4>

      <p className="recipe-nutrition-panel__calories">
        <strong>{formatNumber(nutrition.calories)}</strong>
        <span>kcal</span>
      </p>

      <dl className="recipe-nutrition-panel__values">
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
    </article>
  )
}

export default function RecipeNutritionSummary({
  total,
  perServing,
  eyebrow = 'Live calculation',
  title = 'Nutrition Summary',
  description = 'Calculated from the selected Ingredients.',
  totalTitle = 'Whole Recipe',
  perServingTitle = 'Per Serving',
}: RecipeNutritionSummaryProps) {
  return (
    <section
      className="recipe-nutrition-summary"
      aria-labelledby="recipe-nutrition-title"
    >
      <header>
        <div>
          <p className="recipe-nutrition-summary__eyebrow">{eyebrow}</p>

          <h3 id="recipe-nutrition-title">{title}</h3>
        </div>

        <p>{description}</p>
      </header>

      <div className="recipe-nutrition-summary__panels">
        <NutritionPanel title={totalTitle} nutrition={total} />

        <NutritionPanel title={perServingTitle} nutrition={perServing} />
      </div>
    </section>
  )
}
