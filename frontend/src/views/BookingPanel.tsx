import { useEffect, useRef, useState } from 'react'
import { X } from 'lucide-react'
import { api, ApiError, type Booking, type Me, type Room } from '../api'
import { CANCEL_CUTOFF_HOURS, duration, longDate, minutesUntil, shortTime, toMinutes, toTime } from '../time'
import { blockKind, ErrorNote, POLICY_HINT, POLICY_LABEL, Seats, Spinner, useAction } from '../ui'
import { useSay } from '../toast'

export function isOwner(booking: Booking, me: Me) {
  // The API sends the owner's display name, not their id. The owner is always a joined participant.
  return booking.owner !== null && booking.owner === me.displayName && booking.myStatus === 'Joined'
}

export function BookingPanel({
  me,
  booking,
  room,
  onClose,
  onChanged,
  onGone,
}: {
  me: Me
  booking: Booking
  room: Room | null
  onClose: () => void
  onChanged: (booking: Booking) => void
  onGone: () => void
}) {
  const { busy, error, setError, run } = useAction()
  const [confirming, setConfirming] = useState(false)
  const say = useSay()
  const panelRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    panelRef.current?.focus()
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  useEffect(() => {
    setConfirming(false)
    setError(null)
  }, [booking.id, setError])

  const owner = isOwner(booking, me)
  const isBlock = booking.owner === null
  const untilStart = minutesUntil(booking.date, booking.start)
  const started = untilStart <= 0
  const canCancel = untilStart >= CANCEL_CUTOFF_HOURS * 60
  const capacity = room?.capacity ?? booking.people.length + booking.placesLeft
  const kind = blockKind(booking)
  const minutes = toMinutes(booking.end) - toMinutes(booking.start)
  const cutoff = toTime(Math.max(0, toMinutes(booking.start) - CANCEL_CUTOFF_HOURS * 60))

  async function act(name: string, action: () => Promise<Booking>, message: string) {
    const updated = await run(name, action)
    if (updated) {
      say(message)
      onChanged(updated)
    }
  }

  async function adminCancel() {
    const done = await run('cancel', () => api.adminCancel(booking.id))
    if (done) {
      say(`Booking in ${booking.room} cancelled`)
      onGone()
    }
  }

  const notFound = error instanceof ApiError && error.status === 404

  // Only states the rows above do not already show: who owns it and who is in it are in the list.
  let status: { title: string; text: string } | null = null
  if (isBlock) status = { title: 'Blocked by an admin', text: 'Nobody can book the room at this time.' }
  else if (booking.myStatus === 'Requested')
    status = { title: 'You asked to join', text: `${booking.owner} has not answered yet.` }
  else if (booking.myStatus === 'Declined') status = { title: 'Your request was declined', text: '' }
  else if (booking.myStatus === 'Removed') status = { title: 'The owner removed you', text: '' }
  else if (booking.myStatus === 'Left') status = { title: 'You left this booking', text: '' }

  const canJoin =
    !isBlock &&
    !started &&
    booking.myStatus !== 'Joined' &&
    booking.myStatus !== 'Requested' &&
    booking.joinPolicy !== 'Private' &&
    booking.placesLeft > 0

  return (
    <>
      <div className="scrim" onClick={onClose} />
      <aside
        className="panel"
        role="dialog"
        aria-modal="true"
        aria-labelledby="panel-title"
        tabIndex={-1}
        ref={panelRef}
      >
        <header className="panel-head">
          <div>
            <h2 id="panel-title">{booking.room}</h2>
            <p>{room ? room.building : ''}</p>
          </div>
          <button className="icon-btn" onClick={onClose} aria-label="Close">
            <X size={22} aria-hidden />
          </button>
        </header>

        <div className="panel-body">
          <div className="panel-time">
            <strong>
              {shortTime(booking.start)} to {shortTime(booking.end)}
            </strong>
            <span>
              {longDate(booking.date)} · {duration(minutes)}
            </span>
          </div>

          {isBlock ? (
            <dl className="facts">
              <dt>Note</dt>
              <dd>{booking.note || 'No note'}</dd>
            </dl>
          ) : (
            <dl className="facts">
              <dt>Who can join</dt>
              <dd>
                <i className={`policy-mark k-${booking.joinPolicy === 'Open' ? 'open' : booking.joinPolicy === 'OnRequest' ? 'request' : 'private'}`} />
                {POLICY_LABEL[booking.joinPolicy]}
                <small>{POLICY_HINT[booking.joinPolicy]}</small>
              </dd>
              <dt>Places</dt>
              <dd>
                <Seats taken={booking.people.length} capacity={capacity} />
                {booking.people.length > capacity && (
                  <small>The room was made smaller. Nobody new can join until it fits again.</small>
                )}
              </dd>
            </dl>
          )}

          {!isBlock && (
            <>
              <h3 className="section-title">In this booking</h3>
              <ul className="people">
                {booking.people.map((p) => {
                  const isTheOwner = p.displayName === booking.owner
                  const isMe = p.id === me.id
                  return (
                    <li key={p.id}>
                      <span className="name">{isMe ? `${p.displayName} (you)` : p.displayName}</span>
                      {isTheOwner && <span className="tag">Owner</span>}
                      {owner && !isMe && !started && (
                        <button
                          className="btn small danger"
                          disabled={busy !== null}
                          onClick={() =>
                            act('remove' + p.id, () => api.remove(booking.id, p.id), `${p.displayName} removed`)
                          }
                        >
                          {busy === 'remove' + p.id && <Spinner />}
                          Remove
                        </button>
                      )}
                    </li>
                  )
                })}
              </ul>
            </>
          )}

          {owner && booking.requests.length > 0 && (
            <>
              <h3 className="section-title">Asking to join</h3>
              <ul className="people">
                {booking.requests.map((p) => (
                  <li key={p.id}>
                    <span className="name">{p.displayName}</span>
                    <span className="row-actions">
                      <button
                        className="btn small solid"
                        disabled={busy !== null}
                        onClick={() =>
                          act('approve' + p.id, () => api.approve(booking.id, p.id), `${p.displayName} is in`)
                        }
                      >
                        {busy === 'approve' + p.id && <Spinner />}
                        Approve
                      </button>
                      <button
                        className="btn small"
                        disabled={busy !== null}
                        onClick={() =>
                          act('decline' + p.id, () => api.decline(booking.id, p.id), `Request from ${p.displayName} declined`)
                        }
                      >
                        {busy === 'decline' + p.id && <Spinner />}
                        Decline
                      </button>
                    </span>
                  </li>
                ))}
              </ul>
            </>
          )}

          {status && (
            <div className="panel-status">
              <b>{status.title}</b>
              {status.text}
            </div>
          )}

          {started && !isBlock && <p className="grid-note">This booking has started, so nobody can join or leave.</p>}
          {kind === 'full' && !started && booking.myStatus !== 'Joined' && (
            <p className="grid-note">This booking is full.</p>
          )}
        </div>

        <footer className="panel-foot">
          {error && !notFound && <ErrorNote error={error} onDismiss={() => setError(null)} />}
          {notFound && (
            <div className="confirm">
              <p>This booking does not exist any more.</p>
              <div>
                <button className="btn" onClick={onGone}>
                  Close
                </button>
              </div>
            </div>
          )}

          {confirming ? (
            <div className="confirm">
              <p>Cancel this booking? Everyone in it is notified.</p>
              <div>
                <button
                  className="btn primary"
                  disabled={busy !== null}
                  onClick={() =>
                    me.isAdmin && !owner
                      ? adminCancel()
                      : act('cancel', () => api.cancel(booking.id), `Booking in ${booking.room} cancelled`)
                  }
                >
                  {busy === 'cancel' && <Spinner />}
                  Cancel booking
                </button>
                <button className="btn" onClick={() => setConfirming(false)}>
                  Keep it
                </button>
              </div>
            </div>
          ) : (
            <>
              {canJoin && (
                <button
                  className="btn primary"
                  disabled={busy !== null}
                  onClick={() =>
                    act(
                      'join',
                      () => api.join(booking.id),
                      booking.joinPolicy === 'OnRequest' ? 'Request sent to the owner' : `You joined ${booking.room}`,
                    )
                  }
                >
                  {busy === 'join' && <Spinner />}
                  {booking.joinPolicy === 'OnRequest' ? 'Ask to join' : 'Join'}
                </button>
              )}
              {!owner && booking.myStatus === 'Joined' && !started && (
                <button
                  className="btn"
                  disabled={busy !== null}
                  onClick={() => act('leave', () => api.leave(booking.id), `You left ${booking.room}`)}
                >
                  {busy === 'leave' && <Spinner />}
                  Leave
                </button>
              )}
              {owner && canCancel && (
                <button className="btn danger" onClick={() => setConfirming(true)}>
                  Cancel booking
                </button>
              )}
              {me.isAdmin && !(owner && canCancel) && (
                <button className="btn danger" onClick={() => setConfirming(true)}>
                  Cancel as admin
                </button>
              )}
              {owner && !canCancel && !me.isAdmin && (
                <p className="grid-note">
                  You could cancel until {cutoff}. Now only an admin can cancel it.
                </p>
              )}
            </>
          )}
        </footer>
      </aside>
    </>
  )
}
