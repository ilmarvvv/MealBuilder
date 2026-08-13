import { apiRequest } from './apiClient'

export type RecipeNutrition = {
  calories: number
  protein: number
  fat: number
  carbohydrates: number
  sugars: number
  fiber: number
  salt: number
}

export type RecipeIngredient = {
  ingredientId: number
  ingredientName: string
  grams: number
  position: number
}

export type RecipeStep = {
  id: number
  instruction: string
  position: number
}

export type RecipeSummary = {
  id: number
  name: string
  description: string | null
  servings: number
  ingredientCount: number
  nutritionPerServing: RecipeNutrition
}

export type Recipe = {
  id: number
  name: string
  description: string | null
  servings: number
  totalNutrition: RecipeNutrition
  nutritionPerServing: RecipeNutrition
  ingredients: RecipeIngredient[]
  steps: RecipeStep[]
}

export type RecipeIngredientInput = {
  ingredientId: number
  grams: number
}

export type RecipeStepInput = {
  instruction: string
}

export type RecipeInput = {
  name: string
  description: string | null
  servings: number
  ingredients: RecipeIngredientInput[]
  steps: RecipeStepInput[]
}

export const recipeApi = {
  getAll() {
    return apiRequest<RecipeSummary[]>('/api/recipes')
  },

  getById(id: number) {
    return apiRequest<Recipe>(`/api/recipes/${id}`)
  },

  create(input: RecipeInput) {
    return apiRequest<Recipe>('/api/recipes', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    })
  },

  update(id: number, input: RecipeInput) {
    return apiRequest<Recipe>(`/api/recipes/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    })
  },

  remove(id: number) {
    return apiRequest<void>(`/api/recipes/${id}`, {
      method: 'DELETE',
    })
  },
}