import { apiRequest } from './apiClient'
import {
  createPaginationSearchParams,
  type PagedResponse,
  type PaginationQuery,
} from './pagination'

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

export type IngredientOwnershipFilter = 'All' | 'BuiltIn' | 'Mine'

export type IngredientPageQuery = PaginationQuery & {
  ownership?: IngredientOwnershipFilter
}

export const ingredientApi = {
  getAll() {
    return apiRequest<Ingredient[]>('/api/ingredients')
  },

  getPage(query: IngredientPageQuery = {}) {
    const searchParams = createPaginationSearchParams(query)

    searchParams.set('ownership', query.ownership ?? 'All')

    return apiRequest<PagedResponse<Ingredient>>(
      `/api/ingredients/page?${searchParams.toString()}`,
    )
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
