export const Appearance = {
  System: 'system',
  Light: 'light',
  Dark: 'dark',
} as const

export type Appearance = (typeof Appearance)[keyof typeof Appearance]

const appearanceStorageKey = 'mealbuilder.appearance'

function isAppearance(value: string | null): value is Appearance {
  return (
    value === Appearance.System ||
    value === Appearance.Light ||
    value === Appearance.Dark
  )
}

export function loadAppearance(): Appearance {
  try {
    const storedAppearance = window.localStorage.getItem(appearanceStorageKey)

    return isAppearance(storedAppearance) ? storedAppearance : Appearance.Dark
  } catch {
    return Appearance.Dark
  }
}

export function applyAppearance(appearance: Appearance) {
  if (appearance === Appearance.System) {
    document.documentElement.removeAttribute('data-theme')
    return
  }

  document.documentElement.dataset.theme = appearance
}

export function saveAppearance(appearance: Appearance) {
  try {
    window.localStorage.setItem(appearanceStorageKey, appearance)
  } catch {
    // Apply the preference for the current page when storage is unavailable.
  }

  applyAppearance(appearance)
}

export function initializeAppearance() {
  applyAppearance(loadAppearance())
}
