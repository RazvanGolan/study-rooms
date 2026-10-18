import { useCallback, useState } from 'react'
import { AlertTriangle } from 'lucide-react'
import { ApiError, type Booking, type JoinPolicy } from './api'

export const POLICY_LABEL: Record<JoinPolicy, string> = {
  Private: 'Private',
  Open: 'Open',
  OnRequest: 'On request',
}

export const POLICY_HINT: Record<JoinPolicy, string> = {
  Private: 'Nobody else can join',
  Open: 'Anyone can join while there is room',
  OnRequest: 'People ask, you approve',
}

// The look of a booking block: what kind it is for the student looking at it.
export type BlockKind = 'mine' | 'requested' | 'open' | 'request' | 'private' | 'block' | 'full'

export function blockKind(booking: Booking): BlockKind {
  if (booking.myStatus === 'Joined') return 'mine'
  if (booking.myStatus === 'Requested') return 'requested'
  if (booking.owner === null) return 'block'
  if (booking.joinPolicy === 'Private') return 'private'
  if (booking.placesLeft === 0) return 'full'
  if (booking.joinPolicy === 'OnRequest') return 'request'
  return 'open'
}

export function ErrorNote({ error, onDismiss }: { error: ApiError | Error | null; onDismiss?: () => void }) {
  if (!error) return null
  const problems = error instanceof ApiError ? error.problems : []
  const status = error instanceof ApiError && error.status > 0 ? error.status : null
  return (
    <div className="error-note" role="alert">
      <AlertTriangle size={18} strokeWidth={2.25} aria-hidden />
      <div>
        <p className="error-title">
          {error.message}
          {status && <span className="error-status">{status}</span>}
        </p>
        {problems.length > 0 && (
          <ul>
            {problems.map((p) => (
              <li key={p}>{p}</li>
            ))}
          </ul>
        )}
      </div>
      {onDismiss && (
        <button className="link-btn" onClick={onDismiss}>
          OK
        </button>
      )}
    </div>
  )
}

// One mark per place in the room: filled for each person in the booking.
export function Seats({ taken, capacity, label = true }: { taken: number; capacity: number; label?: boolean }) {
  const marks = Math.max(capacity, taken)
  const over = taken > capacity
  return (
    <span
      className="seats"
      role="img"
      aria-label={`${taken} of ${capacity} places taken`}
      title={over ? `${taken} people, room now holds ${capacity}` : `${taken} of ${capacity} places taken`}
    >
      {Array.from({ length: Math.min(marks, 16) }, (_, i) => (
        <i key={i} className={i < taken ? (i >= capacity ? 'over' : 'f') : undefined} />
      ))}
      {marks > 16 && <em>+{marks - 16}</em>}
      {label && <span className="seats-text">{Math.max(0, capacity - taken)} left</span>}
    </span>
  )
}

export function Segmented<T extends string>({
  value,
  options,
  onChange,
  label,
}: {
  value: T
  options: { value: T; label: string; hint?: string }[]
  onChange: (value: T) => void
  label: string
}) {
  return (
    <div className="segmented" role="radiogroup" aria-label={label}>
      {options.map((o) => (
        <button
          key={o.value}
          type="button"
          role="radio"
          aria-checked={value === o.value}
          title={o.hint}
          onClick={() => onChange(o.value)}
        >
          {o.label}
        </button>
      ))}
    </div>
  )
}

// Runs one API call at a time and keeps its error, so a button can show "busy" and the reason it failed.
export function useAction() {
  const [busy, setBusy] = useState<string | null>(null)
  const [error, setError] = useState<ApiError | Error | null>(null)

  const run = useCallback(async <T,>(name: string, action: () => Promise<T>): Promise<T | undefined> => {
    setBusy(name)
    setError(null)
    try {
      return await action()
    } catch (e) {
      setError(e instanceof Error ? e : new Error(String(e)))
      return undefined
    } finally {
      setBusy(null)
    }
  }, [])

  return { busy, error, setError, run }
}

export function Spinner() {
  return <span className="spinner" aria-hidden />
}
