import { apiRequest } from './apiClient'
import type {
  AddDailyPlanIngredientInput,
  AddDailyPlanPreparedRecipeInput,
  DailyPlan,
  DailyPlanInclusionInput,
  DailyPlanItemAmountInput,
  DailyPlanItemTimeInput,
  MoveDailyPlanItemInput,
  MoveDailyPlanItemResult,
  WeeklySummary,
} from './mealPlanningTypes'

export const dailyPlanApi = {
  getByDate(date: string) {
    return apiRequest<DailyPlan>(`/api/daily-plans/${date}`)
  },

  getWeek(startDate: string) {
    return apiRequest<WeeklySummary>(`/api/daily-plans/week/${startDate}`)
  },

  addIngredient(date: string, input: AddDailyPlanIngredientInput) {
    return apiRequest<DailyPlan>(`/api/daily-plans/${date}/ingredients`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    })
  },

  addPreparedRecipe(date: string, input: AddDailyPlanPreparedRecipeInput) {
    return apiRequest<DailyPlan>(`/api/daily-plans/${date}/prepared-recipes`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    })
  },

  setWeeklySummaryInclusion(
    dailyPlanId: number,
    input: DailyPlanInclusionInput,
  ) {
    return apiRequest<DailyPlan>(
      `/api/daily-plans/${dailyPlanId}/weekly-summary`,
      {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(input),
      },
    )
  },

  changeItemAmount(
    dailyPlanId: number,
    itemId: number,
    input: DailyPlanItemAmountInput,
  ) {
    return apiRequest<DailyPlan>(
      `/api/daily-plans/${dailyPlanId}/items/${itemId}/amount`,
      {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(input),
      },
    )
  },

  changeItemTime(
    dailyPlanId: number,
    itemId: number,
    input: DailyPlanItemTimeInput,
  ) {
    return apiRequest<DailyPlan>(
      `/api/daily-plans/${dailyPlanId}/items/${itemId}/time`,
      {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(input),
      },
    )
  },

  moveItem(dailyPlanId: number, itemId: number, input: MoveDailyPlanItemInput) {
    return apiRequest<MoveDailyPlanItemResult>(
      `/api/daily-plans/${dailyPlanId}/items/${itemId}/move`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(input),
      },
    )
  },

  removeItem(dailyPlanId: number, itemId: number) {
    return apiRequest<DailyPlan>(
      `/api/daily-plans/${dailyPlanId}/items/${itemId}`,
      {
        method: 'DELETE',
      },
    )
  },
}
