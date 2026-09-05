import { useEffect, useState } from 'react'
import { getApiErrorMessages } from '../api/getApiErrorMessages'
import { ingredientApi, type Ingredient } from '../api/ingredientApi'

type UseIngredientRefreshOptions = {
  enabled: boolean
  onRefreshed: (ingredients: Ingredient[]) => void
}

export default function useIngredientRefresh({
  enabled,
  onRefreshed,
}: UseIngredientRefreshOptions) {
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    if (!enabled) {
      return
    }

    let isActive = true
    let requestId = 0
    let refreshTimeout: number | undefined

    async function refreshIngredients() {
      const currentRequestId = ++requestId

      try {
        const loadedIngredients = await ingredientApi.getAll()

        if (isActive && currentRequestId === requestId) {
          onRefreshed(loadedIngredients)
          setErrors([])
        }
      } catch (error) {
        if (isActive && currentRequestId === requestId) {
          setErrors(
            getApiErrorMessages(
              error,
              'Unable to refresh Ingredients. Return to this tab to try again.',
            ),
          )
        }
      }
    }

    function scheduleRefresh() {
      window.clearTimeout(refreshTimeout)

      if (document.visibilityState !== 'visible') {
        return
      }

      refreshTimeout = window.setTimeout(() => {
        void refreshIngredients()
      }, 100)
    }

    function handleFocus(event: FocusEvent) {
      if (
        event.target === window ||
        event.target instanceof HTMLSelectElement
      ) {
        scheduleRefresh()
      }
    }

    window.addEventListener('focus', handleFocus, true)
    document.addEventListener('visibilitychange', scheduleRefresh)

    return () => {
      isActive = false
      window.clearTimeout(refreshTimeout)
      window.removeEventListener('focus', handleFocus, true)
      document.removeEventListener('visibilitychange', scheduleRefresh)
    }
  }, [enabled, onRefreshed])

  return errors
}
