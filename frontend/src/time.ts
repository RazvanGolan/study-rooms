// The booking limits from the API's appsettings.json. The API checks them again; these only shape the grid.
export const OPENS = 8 * 60
export const CLOSES = 20 * 60
export const STEP = 30
export const MIN_MINUTES = 30
export const MAX_MINUTES = 180
export const DAYS_AHEAD = 7
export const CANCEL_CUTOFF_HOURS = 2
export const SLOTS = (CLOSES - OPENS) / STEP

const ZONE = 'Europe/Bucharest'

// "2026-10-20" for today, in the faculty's time zone, wherever the browser is.
export function today(): string {
  return new Intl.DateTimeFormat('en-CA', { timeZone: ZONE, year: 'numeric', month: '2-digit', day: '2-digit' }).format(
    new Date(),
  )
}

// Minutes since midnight in the faculty's time zone.
export function nowMinutes(): number {
  const parts = new Intl.DateTimeFormat('en-GB', { timeZone: ZONE, hour: '2-digit', minute: '2-digit', hourCycle: 'h23' })
    .formatToParts(new Date())
  const hour = Number(parts.find((p) => p.type === 'hour')?.value ?? 0)
  const minute = Number(parts.find((p) => p.type === 'minute')?.value ?? 0)
  return hour * 60 + minute
}

export function addDays(date: string, days: number): string {
  const d = new Date(`${date}T12:00:00Z`)
  d.setUTCDate(d.getUTCDate() + days)
  return d.toISOString().slice(0, 10)
}

// "10:30:00" or "10:30" to 630
export function toMinutes(time: string): number {
  const [h, m] = time.split(':')
  return Number(h) * 60 + Number(m)
}

// 630 to "10:30"
export function toTime(minutes: number): string {
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`
}

export function shortTime(time: string): string {
  return toTime(toMinutes(time))
}

export function duration(minutes: number): string {
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  if (h === 0) return `${m} min`
  if (m === 0) return h === 1 ? '1 hour' : `${h} hours`
  return `${h}.5 hours`
}

export function weekday(date: string, style: 'short' | 'long' = 'short'): string {
  return new Date(`${date}T12:00:00Z`).toLocaleDateString('en-GB', { weekday: style, timeZone: 'UTC' })
}

export function dayOfMonth(date: string): number {
  return Number(date.slice(8, 10))
}

export function monthYear(date: string): string {
  return new Date(`${date}T12:00:00Z`).toLocaleDateString('en-GB', { month: 'long', year: 'numeric', timeZone: 'UTC' })
}

export function longDate(date: string): string {
  return new Date(`${date}T12:00:00Z`).toLocaleDateString('en-GB', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    timeZone: 'UTC',
  })
}

// Minutes from now until the booking starts; negative once it has started.
export function minutesUntil(date: string, start: string): number {
  const dayDiff = Math.round(
    (new Date(`${date}T12:00:00Z`).getTime() - new Date(`${today()}T12:00:00Z`).getTime()) / 86_400_000,
  )
  return dayDiff * 24 * 60 + toMinutes(start) - nowMinutes()
}
