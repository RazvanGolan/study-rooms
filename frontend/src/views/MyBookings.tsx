import { useCallback, useEffect, useState } from 'react'
import { api, type Booking, type CancelReason, type Me, type Room } from '../api'
import { CANCEL_CUTOFF_HOURS, dayOfMonth, minutesUntil, shortTime, toMinutes, weekday, duration, monthYear } from '../time'
import { ErrorNote, POLICY_LABEL, Seats, Spinner, useAction } from '../ui'
import { useSay } from '../toast'
import { isOwner } from './BookingPanel'

const REASON: Record<CancelReason, string> = {
  ByOwner: 'Cancelled by the owner',
  ByAdmin: 'Cancelled by an admin',
  RoomRemoved: 'Cancelled, the room was removed',
}

function ended(b: Booking) {
  return minutesUntil(b.date, b.end) <= 0
}

export function MyBookings({ me, onOpen }: { me: Me; onOpen: (date: string, id: number) => void }) {
  const [bookings, setBookings] = useState<Booking[] | null>(null)
  const [rooms, setRooms] = useState<Room[]>([])
  const [loadError, setLoadError] = useState<Error | null>(null)

  const load = useCallback(async () => {
    try {
      const [b, r] = await Promise.all([api.myBookings(), api.rooms()])
      setBookings(b)
      setRooms(r)
      setLoadError(null)
    } catch (e) {
      setLoadError(e as Error)
    }
  }, [])

  useEffect(() => {
    load()
    const onFocus = () => load()
    window.addEventListener('focus', onFocus)
    return () => window.removeEventListener('focus', onFocus)
  }, [load])

  const sorted = [...(bookings ?? [])].sort((a, b) =>
    a.date === b.date ? toMinutes(a.start) - toMinutes(b.start) : a.date < b.date ? -1 : 1,
  )
  const upcoming = sorted.filter((b) => b.status === 'Active' && b.myStatus === 'Joined' && !ended(b))
  const waiting = sorted.filter((b) => b.status === 'Active' && b.myStatus === 'Requested' && !ended(b))
  const rest = sorted
    .filter((b) => !upcoming.includes(b) && !waiting.includes(b))
    .reverse()

  function update(updated: Booking) {
    setBookings((all) => all?.map((b) => (b.id === updated.id ? updated : b)) ?? all)
  }

  const requests = upcoming.reduce((n, b) => n + (isOwner(b, me) ? b.requests.length : 0), 0)

  return (
    <main className="page">
      <section className="page-title">
        <h1>My bookings</h1>
        <p>
          {bookings === null
            ? 'Loading your bookings.'
            : requests > 0
              ? `${requests} ${requests === 1 ? 'person is' : 'people are'} asking to join your bookings.`
              : 'Bookings you own or joined, and the ones you asked to join.'}
        </p>
      </section>

      {loadError && (
        <div className="empty">
          <ErrorNote error={loadError} />
          <button className="btn" onClick={load}>
            Try again
          </button>
        </div>
      )}

      {bookings && bookings.length === 0 && (
        <div className="empty">
          <h2>Nothing booked yet</h2>
          <p>Pick a free time on the timetable to book a room, or open someone else's booking to join it.</p>
          <a className="btn solid" href="#/timetable">
            Open the timetable
          </a>
        </div>
      )}

      {upcoming.length > 0 && (
        <Group title="Coming up" count={upcoming.length}>
          {upcoming.map((b) => (
            <Entry key={b.id} booking={b} me={me} rooms={rooms} onOpen={onOpen} onChanged={update} />
          ))}
        </Group>
      )}

      {waiting.length > 0 && (
        <Group title="Waiting for an answer" count={waiting.length}>
          {waiting.map((b) => (
            <Entry key={b.id} booking={b} me={me} rooms={rooms} onOpen={onOpen} onChanged={update} />
          ))}
        </Group>
      )}

      {rest.length > 0 && (
        <Group title="Past and cancelled" count={rest.length}>
          {rest.map((b) => (
            <Entry key={b.id} booking={b} me={me} rooms={rooms} onOpen={onOpen} onChanged={update} />
          ))}
        </Group>
      )}
    </main>
  )
}

function Group({ title, count, children }: { title: string; count: number; children: React.ReactNode }) {
  return (
    <section>
      <h2 className="group-title">
        {title}
        <span className="count">{count}</span>
      </h2>
      {children}
    </section>
  )
}

function Entry({
  booking: b,
  me,
  rooms,
  onOpen,
  onChanged,
}: {
  booking: Booking
  me: Me
  rooms: Room[]
  onOpen: (date: string, id: number) => void
  onChanged: (b: Booking) => void
}) {
  const { busy, error, setError, run } = useAction()
  const [confirming, setConfirming] = useState(false)
  const say = useSay()
  const room = rooms.find((r) => r.id === b.roomId)
  const owner = isOwner(b, me)
  const active = b.status === 'Active' && !ended(b)
  const started = minutesUntil(b.date, b.start) <= 0
  const canCancel = owner && minutesUntil(b.date, b.start) >= CANCEL_CUTOFF_HOURS * 60
  const cancelled = b.status === 'Cancelled'

  async function act(name: string, action: () => Promise<Booking>, message: string) {
    const updated = await run(name, action)
    if (updated) {
      say(message)
      onChanged(updated)
      setConfirming(false)
    }
  }

  let state = ''
  if (cancelled) state = b.reason ? REASON[b.reason] : 'Cancelled'
  else if (b.myStatus === 'Left') state = 'You left'
  else if (b.myStatus === 'Removed') state = 'The owner removed you'
  else if (b.myStatus === 'Declined') state = 'Request declined'
  else if (ended(b)) state = 'Over'

  return (
    <article className={`entry${cancelled ? ' cancelled' : ''}`}>
      <div className="entry-date">
        <b>{dayOfMonth(b.date)}</b>
        <span>
          {weekday(b.date)} · {monthYear(b.date).split(' ')[0].slice(0, 3)}
        </span>
      </div>

      <div className="entry-when">
        <strong>
          {shortTime(b.start)} to {shortTime(b.end)}
        </strong>
        <span>
          {b.room}
          {room ? `, ${room.building}` : ''} · {duration(toMinutes(b.end) - toMinutes(b.start))}
        </span>
      </div>

      <div className="entry-who">
        <div>
          {state && <span className={`status-tag${cancelled ? ' red' : ''}`}>{state}</span>}{' '}
          {owner ? 'You own it' : `Owner: ${b.owner}`} · {POLICY_LABEL[b.joinPolicy]}
        </div>
        {room && b.joinPolicy !== 'Private' && <Seats taken={b.people.length} capacity={room.capacity} />}
        {b.people.length > 1 && (
          <div className="muted">
            With {b.people.filter((p) => p.id !== me.id).map((p) => p.displayName).join(', ')}
          </div>
        )}
        {owner &&
          active &&
          b.requests.map((p) => (
            <div className="request-row" key={p.id}>
              <span className="name">{p.displayName} asks to join</span>
              <button
                className="btn small solid"
                disabled={busy !== null}
                onClick={() => act('approve' + p.id, () => api.approve(b.id, p.id), `${p.displayName} is in`)}
              >
                {busy === 'approve' + p.id && <Spinner />}
                Approve
              </button>
              <button
                className="btn small"
                disabled={busy !== null}
                onClick={() =>
                  act('decline' + p.id, () => api.decline(b.id, p.id), `Request from ${p.displayName} declined`)
                }
              >
                {busy === 'decline' + p.id && <Spinner />}
                Decline
              </button>
            </div>
          ))}
        {error && <ErrorNote error={error} onDismiss={() => setError(null)} />}
      </div>

      <div className="entry-actions">
        {!cancelled && (
          <button className="btn small" onClick={() => onOpen(b.date, b.id)}>
            Show on timetable
          </button>
        )}
        {active && !started && !owner && b.myStatus === 'Joined' && (
          <button
            className="btn small"
            disabled={busy !== null}
            onClick={() => act('leave', () => api.leave(b.id), `You left ${b.room}`)}
          >
            {busy === 'leave' && <Spinner />}
            Leave
          </button>
        )}
        {active && canCancel && !confirming && (
          <button className="btn small danger" onClick={() => setConfirming(true)}>
            Cancel
          </button>
        )}
        {confirming && (
          <>
            <button
              className="btn small primary"
              disabled={busy !== null}
              onClick={() => act('cancel', () => api.cancel(b.id), `Booking in ${b.room} cancelled`)}
            >
              {busy === 'cancel' && <Spinner />}
              Yes, cancel it
            </button>
            <button className="btn small" onClick={() => setConfirming(false)}>
              Keep it
            </button>
          </>
        )}
      </div>
    </article>
  )
}
