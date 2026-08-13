import type { Ingredient } from '../api/ingredientApi'
import type { RecipeNutrition } from '../api/recipeApi'

export type RecipeIngredientSelection = {
  ingredientId: number | null
  grams: number
}

export type RecipeNutritionSummary = {
  total: RecipeNutrition
  perServing: RecipeNutrition
}

function createZeroNutrition(): RecipeNutrition {
  return {
    calories: 0,
    protein: 0,
    fat: 0,
    carbohydrates: 0,
    sugars: 0,
    fiber: 0,
    salt: 0,
  }
}

function divideNutrition(
  nutrition: RecipeNutrition,
  divisor: number,
): RecipeNutrition {
  return {
    calories: nutrition.calories / divisor,
    protein: nutrition.protein / divisor,
    fat: nutrition.fat / divisor,
    carbohydrates: nutrition.carbohydrates / divisor,
    sugars: nutrition.sugars / divisor,
    fiber: nutrition.fiber / divisor,
    salt: nutrition.salt / divisor,
  }
}

export function calculateRecipeNutrition(
  selections: RecipeIngredientSelection[],
  ingredients: Ingredient[],
  servings: number,
): RecipeNutritionSummary {
  const ingredientsById = new Map(
    ingredients.map((ingredient) => [
      ingredient.id,
      ingredient,
    ]),
  )

  const total = selections.reduce<RecipeNutrition>(
    (nutrition, selection) => {
      if (
        selection.ingredientId === null ||
        !Number.isFinite(selection.grams) ||
        selection.grams <= 0
      ) {
        return nutrition
      }

      const ingredient = ingredientsById.get(
        selection.ingredientId,
      )

      if (!ingredient) {
        return nutrition
      }

      const multiplier = selection.grams / 100

      return {
        calories:
          nutrition.calories +
          ingredient.caloriesPer100g * multiplier,
        protein:
          nutrition.protein +
          ingredient.proteinPer100g * multiplier,
        fat:
          nutrition.fat +
          ingredient.fatPer100g * multiplier,
        carbohydrates:
          nutrition.carbohydrates +
          ingredient.carbohydratesPer100g * multiplier,
        sugars:
          nutrition.sugars +
          ingredient.sugarsPer100g * multiplier,
        fiber:
          nutrition.fiber +
          ingredient.fiberPer100g * multiplier,
        salt:
          nutrition.salt +
          ingredient.saltPer100g * multiplier,
      }
    },
    createZeroNutrition(),
  )

  const hasValidServings =
    Number.isFinite(servings) && servings > 0

  return {
    total,
    perServing: hasValidServings
      ? divideNutrition(total, servings)
      : createZeroNutrition(),
  }
}