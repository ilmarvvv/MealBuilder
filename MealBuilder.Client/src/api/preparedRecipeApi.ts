import { apiRequest } from './apiClient'
import type {
  CreatePreparedRecipeInput,
  PreparedRecipe,
  PreparedRecipeAllocation,
  PreparedRecipeAvailability,
  PreparedRecipeDeletionImpact,
  PreparedRecipePlanningPreviewInput,
  PreparedRecipeSummary,
} from './mealPlanningTypes'

export const preparedRecipeApi = {
  getAll() {
    return apiRequest<PreparedRecipeSummary[]>('/api/prepared-recipes')
  },

  getById(id: number) {
    return apiRequest<PreparedRecipe>(`/api/prepared-recipes/${id}`)
  },

  getAvailability(id: number) {
    return apiRequest<PreparedRecipeAvailability>(
      `/api/prepared-recipes/${id}/availability`,
    )
  },

  getDeletionImpact(id: number) {
    return apiRequest<PreparedRecipeDeletionImpact>(
      `/api/prepared-recipes/${id}/deletion-impact`,
    )
  },

  previewPlanning(input: PreparedRecipePlanningPreviewInput) {
    return apiRequest<PreparedRecipeAllocation[]>(
      '/api/prepared-recipes/planning-preview',
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(input),
      },
    )
  },

  create(input: CreatePreparedRecipeInput) {
    return apiRequest<PreparedRecipe>('/api/prepared-recipes', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    })
  },

  remove(id: number) {
    return apiRequest<void>(`/api/prepared-recipes/${id}`, {
      method: 'DELETE',
    })
  },
}
