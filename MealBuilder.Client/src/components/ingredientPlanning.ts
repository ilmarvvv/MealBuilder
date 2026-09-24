export type IngredientPlanningMode = 'single' | 'multiple'

export type IngredientPlanningEntry = {
  key: number
  date: string
  grams: string
  plannedTime: string
}

let nextEntryKey = 1

export function createIngredientPlanningEntry(
  date: string,
  grams = '100',
  plannedTime = '',
): IngredientPlanningEntry {
  return {
    key: nextEntryKey++,
    date,
    grams,
    plannedTime,
  }
}
