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
  eyebrow: string
  title: string
  nutrition: RecipeNutrition
}

const numberFormatter = new Intl.NumberFormat('en', {
  maximumFractionDigits: 2,
})

function formatNumber(value: number) {
  return numberFormatter.format(value)
}

function NutritionPanel({ eyebrow, title, nutrition }: NutritionPanelProps) {
  return (
    <article className="recipe-nutrition-panel">
      <header>
        <p className="recipe-nutrition-panel__eyebrow">{eyebrow}</p>
        <h3>{title}</h3>
      </header>

      <div className="recipe-nutrition-panel__calories">
        <span>Calories</span>

        <p>
          <strong>{formatNumber(nutrition.calories)}</strong>
          <span>kcal</span>
        </p>
      </div>

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
  eyebrow = 'Recipe nutrition',
  title = 'Nutrition Summary',
  description = 'Calculated from the selected Ingredients.',
  totalTitle = 'Whole Recipe',
  perServingTitle = 'Per Serving',
}: RecipeNutritionSummaryProps) {
  return (
    <section
      className="recipe-nutrition-summary"
      aria-label={title}
      aria-describedby="recipe-nutrition-description"
    >
      <p
        className="recipe-nutrition-summary__description"
        id="recipe-nutrition-description"
      >
        {description}
      </p>

      <NutritionPanel
        eyebrow={eyebrow}
        title={totalTitle}
        nutrition={total}
      />

      <NutritionPanel
        eyebrow={eyebrow}
        title={perServingTitle}
        nutrition={perServing}
      />
    </section>
  )
}
