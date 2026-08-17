import { apiRequest } from './apiClient'

export const CalculationSex = {
  Female: 1,
  Male: 2,
} as const

export type CalculationSex =
  (typeof CalculationSex)[keyof typeof CalculationSex]

export const ActivityLevel = {
  LowActive: 1,
  ModeratelyActive: 2,
  Active: 3,
  VeryActive: 4,
} as const

export type ActivityLevel = (typeof ActivityLevel)[keyof typeof ActivityLevel]

export const WeightGoal = {
  LoseWeight: 1,
  MaintainWeight: 2,
  GainWeight: 3,
} as const

export type WeightGoal = (typeof WeightGoal)[keyof typeof WeightGoal]

export type CalorieTargetCalculationInput = {
  birthDate: string
  sexForCalculation: CalculationSex
  heightCm: number
  weightKg: number
  activityLevel: ActivityLevel
  weightGoal: WeightGoal
}

export type CalorieTargetEstimate = {
  age: number
  restingEnergyExpenditure: number
  maintenanceCalories: number
  recommendedDailyCalorieTarget: number
}

export type CalculatedProfileInput = {
  dailyCalorieTarget: number
  calculationInputs: CalorieTargetCalculationInput
}

export type NutritionProfile = {
  dailyCalorieTarget: number
  birthDate: string | null
  sexForCalculation: CalculationSex | null
  heightCm: number | null
  weightKg: number | null
  activityLevel: ActivityLevel | null
  weightGoal: WeightGoal | null
  hasCalculationInputs: boolean
}

export const profileApi = {
  getCurrent() {
    return apiRequest<NutritionProfile>('/api/profile')
  },

  calculateTarget(input: CalorieTargetCalculationInput) {
    return apiRequest<CalorieTargetEstimate>(
      '/api/profile/calorie-target/calculate',
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(input),
      },
    )
  },

  saveDailyCalorieTarget(dailyCalorieTarget: number) {
    return apiRequest<NutritionProfile>('/api/profile/calorie-target', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ dailyCalorieTarget }),
    })
  },

  saveCalculated(input: CalculatedProfileInput) {
    return apiRequest<NutritionProfile>('/api/profile/calculated', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    })
  },

  updateCalculationInputs(input: CalorieTargetCalculationInput) {
    return apiRequest<NutritionProfile>('/api/profile/calculation-inputs', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    })
  },
}
