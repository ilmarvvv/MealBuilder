const millisecondsPerDay = 86_400_000

export function getIsoWeekNumber(dateValue: string) {
  const date = new Date(`${dateValue}T00:00:00Z`)
  const isoDayOfWeek = date.getUTCDay() || 7

  date.setUTCDate(date.getUTCDate() + 4 - isoDayOfWeek)

  const isoYearStart = new Date(Date.UTC(date.getUTCFullYear(), 0, 1))

  return Math.ceil(
    ((date.getTime() - isoYearStart.getTime()) / millisecondsPerDay + 1) / 7,
  )
}
