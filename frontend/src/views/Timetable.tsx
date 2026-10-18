import { useCallback, useEffect, useMemo, useRef, useState, type CSSProperties } from 'react'
import { ChevronLeft, ChevronRight, X } from 'lucide-react'
import { api, ApiError, type Booking, type JoinPolicy, type Me, type Room } from '../api'
import {
  addDays,
  dayOfMonth,
  DAYS_AHEAD,
  duration,
  MAX_MINUTES,
  monthYear,
  nowMinutes,
  OPENS,
  CLOSES,
  shortTime,
  SLOTS,
  STEP,
  today,
  toMinutes,
  toTime,
  weekday,
} from '../time'
import { blockKind, ErrorNote, POLICY_HINT, POLICY_LABEL, Seats, Segmented, Spinner, useAction } from '../ui'
import { useSay } from '../toast'
import { BookingPanel } from './BookingPanel'

const HOURS = Array.from({ length: (CLOSES - OPENS) / 60 }, (_, i) => String(OPENS / 60 + i).padStart(2, '0'))
const MAX_SLOTS = MAX_MINUTES / STEP

interface Selection {
  roomId: number
  start: number // first slot
  end: number // slot after the last one
}

type AdminMode = 'self' | 'student' | 'block'

export function equipmentList(equipment: string): string[] {
  if (!equipment || equipment === 'None') return []
  const names: Record<string, string> = { Projector: 'Projector', Whiteboard: 'Whiteboard', Tv: 'TV', PowerOutlets: 'Outlets' }
  return equipment.split(',').map((e) => names[e.trim()] ?? e.trim())
}

function slotOf(time: string) {
  return (toMinutes(time) - OPENS) / STEP
}

// Below this width (phones, tablets, small windows) the timetable turns on its side: hours run down the page, rooms across.
const PHONE = 1080
// Height of one half hour in the phone layout, and the narrowest a room column may get.
const ROW = 34
const MIN_COLUMN = 96

function useWindowWidth() {
  const [width, setWidth] = useState(window.innerWidth)
  useEffect(() => {
    const onResize = () => setWidth(window.innerWidth)
    window.addEventListener('resize', onResize)
    return () => window.removeEventListener('resize', onResize)
  }, [])
  return width
}

function pct(slot: number) {
  return `${(slot / SLOTS) * 100}%`
}

export function Timetable({
  me,
  focus,
  onFocusDone,
}: {
  me: Me
  focus: { date: string; id: number } | null
  onFocusDone: () => void
}) {
  const [date, setDate] = useState(focus?.date ?? today())
  const [rooms, setRooms] = useState<Room[] | null>(null)
  const [bookings, setBookings] = useState<Booking[] | null>(null)
  const [loadError, setLoadError] = useState<ApiError | Error | null>(null)
  const [selection, setSelection] = useState<Selection | null>(null)
  const [openId, setOpenId] = useState<number | null>(null)
  const [now, setNow] = useState(nowMinutes())
  const [todayDate, setTodayDate] = useState(today())
  const [firstRoom, setFirstRoom] = useState(0)
  const width = useWindowWidth()
  const phone = width <= PHONE

  const load = useCallback(
    async (quiet = false) => {
      if (!quiet) setBookings(null)
      try {
        const [r, b] = await Promise.all([api.rooms(), api.timetable(date)])
        setRooms(r)
        setBookings(b)
        setLoadError(null)
      } catch (e) {
        if (!quiet) setLoadError(e as Error)
      }
    },
    [date],
  )

  useEffect(() => {
    load()
    setSelection(null)
  }, [load])

  // Keep the grid current: another student may book or join while this page is open.
  useEffect(() => {
    const tick = () => {
      setNow(nowMinutes())
      setTodayDate(today())
    }
    const poll = window.setInterval(() => {
      tick()
      if (document.visibilityState === 'visible') load(true)
    }, 15_000)
    const onFocus = () => {
      tick()
      load(true)
    }
    window.addEventListener('focus', onFocus)
    return () => {
      window.clearInterval(poll)
      window.removeEventListener('focus', onFocus)
    }
  }, [load])

  // Open a booking chosen in "My bookings".
  useEffect(() => {
    if (focus && bookings && focus.date === date) {
      if (bookings.some((b) => b.id === focus.id)) setOpenId(focus.id)
      onFocusDone()
    }
  }, [focus, bookings, date, onFocusDone])

  const isToday = date === todayDate
  const isPastDay = date < todayDate
  const lastDay = addDays(todayDate, DAYS_AHEAD)
  const beyond = date > lastDay && !me.isAdmin
  const firstFreeSlot = isPastDay ? SLOTS : isToday ? Math.max(0, Math.ceil((now - OPENS) / STEP)) : 0

  const taken = useMemo(() => {
    const map = new Map<number, boolean[]>()
    for (const room of rooms ?? []) map.set(room.id, Array(SLOTS).fill(false))
    for (const b of bookings ?? []) {
      const lane = map.get(b.roomId)
      if (!lane) continue
      for (let s = Math.max(0, slotOf(b.start)); s < Math.min(SLOTS, slotOf(b.end)); s++) lane[s] = true
    }
    return map
  }, [rooms, bookings])

  function bookable(roomId: number, slot: number) {
    return slot >= firstFreeSlot && !beyond && !taken.get(roomId)?.[slot]
  }

  function rangeFree(roomId: number, start: number, end: number) {
    for (let s = start; s < end; s++) if (!bookable(roomId, s)) return false
    return true
  }

  // A click picks the half hour it lands on. A second click in the same room moves the end there,
  // and a click inside the selection shrinks it to end there (or clears it if it is one half hour).
  function pick(roomId: number, slot: number) {
    if (selection && selection.roomId === roomId) {
      if (slot >= selection.start && slot < selection.end) {
        setSelection(selection.end - selection.start === 1 ? null : { ...selection, end: slot + 1 })
        return
      }
      if (slot >= selection.start) {
        const end = slot + 1
        if (end - selection.start <= MAX_SLOTS && rangeFree(roomId, selection.start, end)) {
          setSelection({ ...selection, end })
          return
        }
      } else if (selection.end - slot <= MAX_SLOTS && rangeFree(roomId, slot, selection.end)) {
        setSelection({ ...selection, start: slot })
        return
      }
    }
    setSelection({ roomId, start: slot, end: slot + 1 })
  }

  // Dragging with a mouse draws the range directly. A click without a drag works like a tap.
  const drag = useRef<{ roomId: number; anchor: number; moved: boolean } | null>(null)
  const suppressClick = useRef(false)

  function onCellDown(e: React.PointerEvent, roomId: number, slot: number) {
    suppressClick.current = false
    if (e.pointerType !== 'mouse' || e.button !== 0) return
    drag.current = { roomId, anchor: slot, moved: false }
  }

  function onCellEnter(roomId: number, slot: number) {
    const d = drag.current
    if (!d || d.roomId !== roomId || slot === d.anchor) return
    const start = Math.min(d.anchor, slot)
    const end = Math.max(d.anchor, slot) + 1
    if (end - start <= MAX_SLOTS && rangeFree(roomId, start, end)) {
      d.moved = true
      setSelection({ roomId, start, end })
    }
  }

  useEffect(() => {
    const up = () => {
      if (drag.current?.moved) suppressClick.current = true
      drag.current = null
    }
    window.addEventListener('pointerup', up)
    return () => window.removeEventListener('pointerup', up)
  }, [])

  function onCellClick(roomId: number, slot: number) {
    if (suppressClick.current) {
      suppressClick.current = false
      return
    }
    pick(roomId, slot)
  }

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && openId === null) setSelection(null)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [openId])

  // Keep the selected range in view: above the booking bar, and below the room header that stays at the top.
  useEffect(() => {
    if (!selection) return
    const frame = requestAnimationFrame(() => {
      const bar = document.querySelector('.bar')
      const range = document.querySelector('.sel')
      if (!bar || !range) return
      const r = range.getBoundingClientRect()
      const limit = window.innerHeight - bar.getBoundingClientRect().height - 12
      const head = document.querySelector('.vhead')?.getBoundingClientRect().bottom ?? 0
      if (r.bottom > limit) window.scrollBy({ top: Math.min(r.bottom - limit, r.top - head - 8), behavior: 'smooth' })
      else if (r.top < head) window.scrollBy({ top: r.top - head - 8, behavior: 'smooth' })
    })
    return () => cancelAnimationFrame(frame)
  }, [selection])

  const days = Array.from({ length: DAYS_AHEAD + 1 }, (_, i) => addDays(todayDate, i))
  const opened = bookings?.find((b) => b.id === openId) ?? null
  const selectedRoom = rooms?.find((r) => r.id === selection?.roomId) ?? null

  function blockButton(b: Booking, room: Room, style: CSSProperties, tall: boolean) {
    const kind = blockKind(b)
    const isBlock = b.owner === null
    return (
      <button
        key={b.id}
        className={`blk k-${kind}`}
        style={style}
        aria-expanded={openId === b.id}
        aria-label={`${isBlock ? b.note ?? 'Blocked' : b.owner}, ${shortTime(b.start)} to ${shortTime(b.end)}, ${POLICY_LABEL[b.joinPolicy]}`}
        onClick={() => setOpenId(b.id)}
      >
        <b>{isBlock ? b.note || 'Blocked' : b.myStatus === 'Joined' && b.owner === me.displayName ? 'You' : b.owner}</b>
        <span className="t">
          {shortTime(b.start)} to {shortTime(b.end)}
          {kind === 'full' && <span className="full-tag"> · full</span>}
          {kind === 'request' && ' · ask to join'}
        </span>
        {tall && !isBlock && b.joinPolicy !== 'Private' && (
          <Seats taken={b.people.length} capacity={room.capacity} label={false} />
        )}
      </button>
    )
  }

  function selectionBox(style: CSSProperties) {
    if (!selection) return null
    return (
      <div className="sel" style={style} aria-hidden="true">
        New booking
        <span>
          {toTime(OPENS + selection.start * STEP)} to {toTime(OPENS + selection.end * STEP)}
        </span>
      </div>
    )
  }

  function cell(room: Room, slot: number) {
    const time = toTime(OPENS + slot * STEP)
    const free = bookable(room.id, slot)
    const past = slot < firstFreeSlot
    return (
      <button
        key={slot}
        className={`cell${past ? ' past' : ''}${slot % 2 === 0 ? ' hour' : ''}`}
        disabled={!free}
        tabIndex={free ? 0 : -1}
        aria-label={`${room.name}, ${time}`}
        onPointerDown={(e) => free && onCellDown(e, room.id, slot)}
        onPointerEnter={() => free && onCellEnter(room.id, slot)}
        onClick={() => free && onCellClick(room.id, slot)}
      />
    )
  }

  // The phone layout shows as many room columns as fit; arrows move the view one room at a time.
  const roomCount = rooms?.length ?? 3
  const visible = Math.max(1, Math.min(roomCount, Math.floor((width - 32 - 48) / MIN_COLUMN)))
  const first = Math.min(firstRoom, Math.max(0, roomCount - visible))
  const shown = rooms ? rooms.slice(first, first + visible) : null
  const nowVisible = isToday && now >= OPENS && now < CLOSES

  function replaceBooking(updated: Booking) {
    setBookings((all) => {
      if (!all) return all
      if (updated.status === 'Cancelled') return all.filter((b) => b.id !== updated.id)
      const exists = all.some((b) => b.id === updated.id)
      return exists ? all.map((b) => (b.id === updated.id ? updated : b)) : [...all, updated]
    })
  }

  return (
    <main className={`page${selection ? ' has-bar' : ''}`}>
      <section className="day-head">
        <div className="day-title">
          <h1>
            {weekday(date)} {dayOfMonth(date)}
          </h1>
          <p>
            {isToday && <span className="today-tag">Today</span>}
            {monthYear(date)}
          </p>
        </div>
        <nav className="days" aria-label="Day">
          <button
            className="icon-btn"
            onClick={() => setDate(addDays(date, -1))}
            aria-label="Previous day"
            disabled={!me.isAdmin && date <= todayDate}
          >
            <ChevronLeft size={18} aria-hidden />
          </button>
          {days.map((d) => (
            <button key={d} aria-pressed={d === date} onClick={() => setDate(d)}>
              {weekday(d)}
              <b>{dayOfMonth(d)}</b>
            </button>
          ))}
          <button
            className="icon-btn"
            onClick={() => setDate(addDays(date, 1))}
            aria-label="Next day"
            disabled={!me.isAdmin && date >= lastDay}
          >
            <ChevronRight size={18} aria-hidden />
          </button>
        </nav>
        <div className="legend" aria-label="Legend">
          <span>
            <i className="k-open" />
            Open
          </span>
          <span>
            <i className="k-request" />
            On request
          </span>
          <span>
            <i className="k-private" />
            Private
          </span>
          <span>
            <i className="k-mine" />
            You are in it
          </span>
          <span>
            <i className="k-requested" />
            You asked
          </span>
          <span>
            <i className="k-block" />
            Blocked
          </span>
        </div>
      </section>

      {loadError ? (
        <div className="grid-empty">
          <ErrorNote error={loadError} />
          <button className="btn" onClick={() => load()}>
            Try again
          </button>
        </div>
      ) : rooms && rooms.length === 0 ? (
        <div className="grid-empty">
          <h2>No rooms yet</h2>
          <p>{me.isAdmin ? 'Add the first room on the Admin page.' : 'An admin has not added any rooms yet.'}</p>
          {me.isAdmin && (
            <a className="btn" href="#/admin">
              Add a room
            </a>
          )}
        </div>
      ) : phone ? (
        <div className={`vgrid${bookings ? '' : ' grid-loading'}`} aria-busy={bookings === null}>
          <div className="vhead">
            <div className="vcorner">
              {visible < roomCount && (
                <>
                  <button
                    className="icon-btn"
                    aria-label="Previous rooms"
                    disabled={first === 0}
                    onClick={() => setFirstRoom(first - 1)}
                  >
                    <ChevronLeft size={18} aria-hidden />
                  </button>
                  <button
                    className="icon-btn"
                    aria-label="Next rooms"
                    disabled={first + visible >= roomCount}
                    onClick={() => setFirstRoom(first + 1)}
                  >
                    <ChevronRight size={18} aria-hidden />
                  </button>
                </>
              )}
            </div>
            <div className="vrooms" style={{ gridTemplateColumns: `repeat(${visible}, 1fr)` }}>
              {(shown ?? Array.from({ length: visible }, () => null)).map((room, i) =>
                room === null ? (
                  <div className="vplate" key={i} />
                ) : (
                  <div className="vplate" key={room.id}>
                    <b>{room.name}</b>
                    <span className="building">{room.building}</span>
                    <span className="kit">{room.capacity} places</span>
                  </div>
                ),
              )}
            </div>
          </div>

          <div className="vbody" style={{ height: SLOTS * ROW }}>
            <div className="vtimes" aria-hidden="true">
              {HOURS.map((h, i) => (
                <span key={h} style={{ top: i * 2 * ROW }}>
                  {h}
                </span>
              ))}
            </div>
            <div className="vcols" style={{ gridTemplateColumns: `repeat(${visible}, 1fr)` }}>
              {shown?.map((room) => (
                <div className="vcol" key={room.id} data-room={room.id}>
                  <div className="vcells">{Array.from({ length: SLOTS }, (_, slot) => cell(room, slot))}</div>
                  {bookings
                    ?.filter((b) => b.roomId === room.id)
                    .map((b) => {
                      const start = Math.max(0, slotOf(b.start))
                      const end = Math.min(SLOTS, slotOf(b.end))
                      return blockButton(b, room, { top: start * ROW + 2, height: (end - start) * ROW - 4 }, end - start >= 3)
                    })}
                  {selection?.roomId === room.id &&
                    selectionBox({ top: selection.start * ROW + 2, height: (selection.end - selection.start) * ROW - 4 })}
                </div>
              ))}
            </div>
            {nowVisible && (
              <div className="vnow" style={{ top: ((now - OPENS) / STEP) * ROW }}>
                <span>{toTime(now)}</span>
              </div>
            )}
          </div>
        </div>
      ) : (
        <div className="grid-scroll">
          <div className={`grid${bookings ? '' : ' grid-loading'}`} aria-busy={bookings === null}>
            <div className="grid-row grid-hours" aria-hidden="true">
              <div className="plate-head" />
              <div className="hours">
                {HOURS.map((h) => (
                  <span key={h}>{h}</span>
                ))}
              </div>
            </div>

            <div className="lanes">
            {(rooms ?? [null, null, null]).map((room, i) =>
              room === null ? (
                <div className="grid-row" key={i}>
                  <div className="plate" />
                  <div className="lane" />
                </div>
              ) : (
                <div className="grid-row" key={room.id} data-room={room.id}>
                  <div className="plate">
                    <b>{room.name}</b>
                    <span className="building">{room.building}</span>
                    <span className="kit">
                      {room.capacity} places
                      <span className="kit-extra">
                        {equipmentList(room.equipment)
                          .map((e) => ` · ${e}`)
                          .join('')}
                      </span>
                    </span>
                  </div>
                  <div className="lane">
                    <div className="cells">{Array.from({ length: SLOTS }, (_, slot) => cell(room, slot))}</div>

                    {bookings
                      ?.filter((b) => b.roomId === room.id)
                      .map((b) => {
                        const start = Math.max(0, slotOf(b.start))
                        const end = Math.min(SLOTS, slotOf(b.end))
                        return blockButton(b, room, { left: pct(start), width: `calc(${pct(end - start)} - 4px)` }, end - start >= 3 || width >= 1300)
                      })}

                    {selection?.roomId === room.id &&
                      selectionBox({
                        left: pct(selection.start),
                        width: `calc(${pct(selection.end - selection.start)} - 4px)`,
                      })}
                  </div>
                </div>
              ),
            )}

            {nowVisible && (
              <div
                className="now"
                style={{ left: `calc(var(--plate) + (100% - var(--plate)) * ${(now - OPENS) / (CLOSES - OPENS)})` }}
              >
                <span>{toTime(now)}</span>
              </div>
            )}
            </div>
          </div>
        </div>
      )}

      {rooms && rooms.length > 0 && !loadError && (
        <p className="grid-note">
          {isPastDay
            ? 'This day is over. You can look, but not book.'
            : beyond
              ? `Rooms can be booked up to ${DAYS_AHEAD} days ahead.`
              : phone
                ? 'Tap a free time to book it, then tap again to move the end.'
                : 'Click a free time to book it, then click again to move the end. With a mouse you can also drag.'}
        </p>
      )}

      {selection && selectedRoom && (
        <BookingBar
          me={me}
          room={selectedRoom}
          date={date}
          selection={selection}
          isFree={(start, end) => rangeFree(selection.roomId, start, end)}
          firstFreeSlot={firstFreeSlot}
          onChange={setSelection}
          onClose={() => setSelection(null)}
          onBooked={() => {
            setSelection(null)
            load(true)
          }}
        />
      )}

      {opened && (
        <BookingPanel
          me={me}
          booking={opened}
          room={rooms?.find((r) => r.id === opened.roomId) ?? null}
          onClose={() => setOpenId(null)}
          onChanged={(b) => {
            replaceBooking(b)
            if (b.status === 'Cancelled') setOpenId(null)
          }}
          onGone={() => {
            setOpenId(null)
            load(true)
          }}
        />
      )}
    </main>
  )
}

function BookingBar({
  me,
  room,
  date,
  selection,
  isFree,
  firstFreeSlot,
  onChange,
  onClose,
  onBooked,
}: {
  me: Me
  room: Room
  date: string
  selection: Selection
  isFree: (start: number, end: number) => boolean
  firstFreeSlot: number
  onChange: (s: Selection) => void
  onClose: () => void
  onBooked: () => void
}) {
  const [policy, setPolicy] = useState<JoinPolicy>('Open')
  const [mode, setMode] = useState<AdminMode>('self')
  const [studentEmail, setStudentEmail] = useState('')
  const [note, setNote] = useState('Class: ')
  const { busy, error, setError, run } = useAction()
  const say = useSay()

  useEffect(() => setError(null), [selection, setError])

  const start = toTime(OPENS + selection.start * STEP)
  const end = toTime(OPENS + selection.end * STEP)
  const minutes = (selection.end - selection.start) * STEP

  const startOptions: number[] = []
  for (let s = Math.max(firstFreeSlot, selection.end - MAX_SLOTS); s < selection.end; s++) {
    if (isFree(s, selection.end)) startOptions.push(s)
  }
  const endOptions: number[] = []
  for (let e = selection.start + 1; e <= Math.min(SLOTS, selection.start + MAX_SLOTS); e++) {
    if (isFree(selection.start, e)) endOptions.push(e)
  }

  async function book() {
    const done = await run('book', async () => {
      if (mode === 'student') return api.adminBook(room.id, date, start, end, studentEmail.trim(), policy)
      if (mode === 'block') return api.adminBlock(room.id, date, start, end, note.trim())
      return api.book(room.id, date, start, end, policy)
    })
    if (done) {
      say(mode === 'block' ? `${room.name} blocked, ${start} to ${end}` : `Booked ${room.name}, ${start} to ${end}`)
      onBooked()
    }
  }

  const canSubmit = (mode !== 'student' || studentEmail.includes('@')) && (mode !== 'block' || note.trim().length > 0)

  return (
    <aside className="bar" aria-label="New booking">
      {error && (
        <div className="bar-errors">
          <ErrorNote error={error} onDismiss={() => setError(null)} />
        </div>
      )}
      <div className="bar-inner">
        <div className="bar-what">
          <strong>
            {room.name} · {start} to {end}
          </strong>
          <span>
            {weekday(date, 'long')} {dayOfMonth(date)} · {duration(minutes)} · up to {room.capacity}{' '}
            {room.capacity === 1 ? 'person' : 'people'}
          </span>
        </div>

        <div className="bar-times">
          <label className="visually-hidden" htmlFor="bar-start">
            Start
          </label>
          <select
            id="bar-start"
            className="select"
            value={selection.start}
            onChange={(e) => onChange({ ...selection, start: Number(e.target.value) })}
          >
            {startOptions.map((s) => (
              <option key={s} value={s}>
                {toTime(OPENS + s * STEP)}
              </option>
            ))}
          </select>
          <span aria-hidden>to</span>
          <label className="visually-hidden" htmlFor="bar-end">
            End
          </label>
          <select
            id="bar-end"
            className="select"
            value={selection.end}
            onChange={(e) => onChange({ ...selection, end: Number(e.target.value) })}
          >
            {endOptions.map((e) => (
              <option key={e} value={e}>
                {toTime(OPENS + e * STEP)}
              </option>
            ))}
          </select>
        </div>

        <div className="bar-controls">
          {me.isAdmin && (
            <Segmented<AdminMode>
              label="Who the booking is for"
              value={mode}
              onChange={setMode}
              options={[
                { value: 'self', label: 'For me' },
                { value: 'student', label: 'For a student' },
                { value: 'block', label: 'Block' },
              ]}
            />
          )}
          {mode === 'student' && (
            <input
              className="input"
              type="email"
              placeholder="Student's email"
              aria-label="Student's email"
              value={studentEmail}
              onChange={(e) => setStudentEmail(e.target.value)}
            />
          )}
          {mode === 'block' ? (
            <input
              className="input"
              placeholder="Class: PCLP"
              aria-label="Note"
              value={note}
              onChange={(e) => setNote(e.target.value)}
            />
          ) : (
            <Segmented<JoinPolicy>
              label="Who can join"
              value={policy}
              onChange={setPolicy}
              options={(['Private', 'Open', 'OnRequest'] as JoinPolicy[]).map((p) => ({
                value: p,
                label: POLICY_LABEL[p],
                hint: POLICY_HINT[p],
              }))}
            />
          )}
          <button className="btn primary" onClick={book} disabled={busy !== null || !canSubmit}>
            {busy && <Spinner />}
            {mode === 'block' ? 'Block room' : 'Book room'}
          </button>
          <button className="icon-btn" onClick={onClose} aria-label="Cancel selection" title="Esc">
            <X size={20} aria-hidden />
          </button>
        </div>
      </div>
    </aside>
  )
}
