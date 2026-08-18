import { useState } from 'react'
import {
  Appearance,
  loadAppearance,
  saveAppearance,
} from '../../theme/appearance'
import type { Appearance as AppearanceValue } from '../../theme/appearance'
import './AppearanceSettings.css'

const appearanceOptions: Array<{
  value: AppearanceValue
  label: string
  description: string
}> = [
  {
    value: Appearance.System,
    label: 'System',
    description: 'Follow your device appearance.',
  },
  {
    value: Appearance.Light,
    label: 'Light',
    description: 'Always use the light theme.',
  },
  {
    value: Appearance.Dark,
    label: 'Dark',
    description: 'Always use the dark theme.',
  },
]

export default function AppearanceSettings() {
  const [selectedAppearance, setSelectedAppearance] =
    useState<AppearanceValue>(loadAppearance)

  function handleChange(appearance: AppearanceValue) {
    setSelectedAppearance(appearance)
    saveAppearance(appearance)
  }

  return (
    <section className="account-page__section">
      <header>
        <h2>Appearance</h2>
        <p>Choose how MealBuilder looks in this browser.</p>
      </header>

      <fieldset className="appearance-settings">
        <legend>Theme</legend>

        {appearanceOptions.map((option) => (
          <label
            className={`appearance-settings__option${
              selectedAppearance === option.value
                ? ' appearance-settings__option--selected'
                : ''
            }`}
            key={option.value}
          >
            <input
              type="radio"
              name="appearance"
              value={option.value}
              checked={selectedAppearance === option.value}
              onChange={() => handleChange(option.value)}
            />

            <span>
              <strong>{option.label}</strong>
              <span>{option.description}</span>
            </span>
          </label>
        ))}
      </fieldset>
    </section>
  )
}
