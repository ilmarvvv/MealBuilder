import type { RecipeNutrition } from './recipeApi'

export const DailyPlanItemType = {
  Ingredient: 1,
  PreparedRecipe: 2,
} as const

export type DailyPlanItemType =
  (typeof DailyPlanItemType)[keyof typeof DailyPlanItemType]

export type PreparedRecipeAllocationInput = {
  date: string
  portions: number
  plannedTime: string | null
}

export type PreparedRecipeAllocation = {
  date: string
  portions: number
}

export type CreatePreparedRecipeInput = {
  recipeId: number
  preparedDate: string
  totalPortions: number
  allocations: PreparedRecipeAllocationInput[]
}

export type PreparedRecipePlanningPreviewInput = {
  recipeId: number
  preparedDate: string
  totalPortions: number
  startDate: string
  plannedDays: number
}

export type PreparedRecipeIngredient = {
  id: number
  name: string
  grams: number
  position: number
  nutrition: RecipeNutrition
}

export type PreparedRecipeSummary = {
  id: number
  sourceRecipeId: number | null
  name: string
  preparedDate: string
  totalPortions: number
  allocatedPortions: number
  availablePortions: number
  nutritionPerPortion: RecipeNutrition
}

export type PreparedRecipe = {
  id: number
  sourceRecipeId: number | null
  name: string
  preparedDate: string
  totalPortions: number
  allocatedPortions: number
  availablePortions: number
  totalNutrition: RecipeNutrition
  nutritionPerPortion: RecipeNutrition
  ingredients: PreparedRecipeIngredient[]
}

export type PreparedRecipeAvailability = {
  preparedRecipeId: number
  totalPortions: number
  allocatedPortions: number
  availablePortions: number
}

export type PreparedRecipeDeletionImpact = {
  preparedRecipeId: number
  name: string
  affectedItemCount: number
  affectedDateCount: number
}

export type DailyPlanItem = {
  id: number
  itemType: DailyPlanItemType
  ingredientId: number | null
  preparedRecipeId: number | null
  name: string
  grams: number | null
  portions: number | null
  plannedTime: string | null
  nutrition: RecipeNutrition
}

export type DailyPlan = {
  id: number | null
  date: string
  includeInWeeklySummary: boolean
  nutrition: RecipeNutrition
  items: DailyPlanItem[]
}

export type AddDailyPlanIngredientInput = {
  ingredientId: number
  grams: number
  plannedTime: string | null
}

export type AddDailyPlanPreparedRecipeInput = {
  preparedRecipeId: number
  portions: number
  plannedTime: string | null
}

export type DailyPlanItemAmountInput = {
  amount: number
}

export type DailyPlanItemTimeInput = {
  plannedTime: string | null
}

export type MoveDailyPlanItemInput = {
  destinationDate: string
  amount: number
}

export type MoveDailyPlanItemResult = {
  sourcePlan: DailyPlan
  destinationPlan: DailyPlan
}

export type DailyPlanInclusionInput = {
  includeInWeeklySummary: boolean
}

export type WeeklyDay = {
  date: string
  dailyPlanId: number | null
  hasPlan: boolean
  includeInWeeklySummary: boolean
  nutrition: RecipeNutrition
}

export type WeeklySummary = {
  startDate: string
  endDate: string
  includedDayCount: number
  totalNutrition: RecipeNutrition
  averageNutrition: RecipeNutrition
  days: WeeklyDay[]
}
