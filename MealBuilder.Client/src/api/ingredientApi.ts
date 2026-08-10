import { apiRequest } from './apiClient'

export type Ingredient = {
  id: number
  name: string
  caloriesPer100g: number
  proteinPer100g: number
  fatPer100g: number
  carbohydratesPer100g: number
  sugarsPer100g: number
  fiberPer100g: number
  saltPer100g: number
  isBuiltIn: boolean
  sourceName: string | null
  sourceCode: string | null
  sourceVersion: string | null
}

export type IngredientInput = {
  name: string
  caloriesPer100g: number
  proteinPer100g: number
  fatPer100g: number
  carbohydratesPer100g: number
  sugarsPer100g: number
  fiberPer100g: number
  saltPer100g: number
}

export const ingredientApi = {
  getAll() {
    return apiRequest<Ingredient[]>('/api/ingredients')
  },

  getById(id: number) {
    return apiRequest<Ingredient>(`/api/ingredients/${id}`)
  },

  create(input: IngredientInput) {
    return apiRequest<Ingredient>('/api/ingredients', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    })
  },

  update(id: number, input: IngredientInput) {
    return apiRequest<Ingredient>(`/api/ingredients/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    })
  },

  remove(id: number) {
    return apiRequest<void>(`/api/ingredients/${id}`, {
      method: 'DELETE',
    })
  },
}