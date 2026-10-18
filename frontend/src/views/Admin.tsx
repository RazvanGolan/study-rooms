import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { api, EQUIPMENT, type AdminBooking, type CancelReason, type EquipmentName, type Room } from '../api'
import { addDays, dayOfMonth, longDate, shortTime, today, weekday } from '../time'
import { EQUIPMENT_LABEL, ErrorNote, POLICY_LABEL, Segmented, Spinner, useAction } from '../ui'
import { useSay } from '../toast'
import { equipmentList } from './Timetable'

const REASON: Record<CancelReason, string> = {
  ByOwner: 'by the owner',
  ByAdmin: 'by an admin',
  RoomRemoved: 'room removed',
}

type Draft = { name: string; building: string; capacity: string; equipment: EquipmentName[] }

const EMPTY: Draft = { name: '', building: '', capacity: '4', equipment: [] }

function toDraft(room: Room): Draft {
  const set = room.equipment.split(',').map((e) => e.trim())
  return {
    name: room.name,
    building: room.building,
    capacity: String(room.capacity),
    equipment: EQUIPMENT.filter((e) => set.includes(e)),
  }
}

function fromDraft(d: Draft): Omit<Room, 'id'> {
  return {
    name: d.name.trim(),
    building: d.building.trim(),
    capacity: Number(d.capacity),
    equipment: d.equipment.length ? d.equipment.join(', ') : 'None',
  }
}

export function Admin() {
  const [tab, setTab] = useState<'rooms' | 'bookings'>('rooms')
  return (
    <main className="page">
      <section className="page-title">
        <h1>Admin</h1>
        <p>Rooms and every booking, with emails. To book for a student or block a room, select the time on the timetable.</p>
        <Segmented
          label="Section"
          value={tab}
          onChange={setTab}
          options={[
            { value: 'rooms', label: 'Rooms' },
            { value: 'bookings', label: 'Bookings' },
          ]}
        />
      </section>
      {tab === 'rooms' ? <Rooms /> : <Bookings />}
    </main>
  )
}

function RoomFields({ draft, onChange }: { draft: Draft; onChange: (d: Draft) => void }) {
  return (
    <>
      <label className="field">
        <span>Name</span>
        <input
          className="input"
          required
          value={draft.name}
          placeholder="A101"
          onChange={(e) => onChange({ ...draft, name: e.target.value })}
        />
      </label>
      <label className="field">
        <span>Building</span>
        <input
          className="input"
          required
          value={draft.building}
          placeholder="Library"
          onChange={(e) => onChange({ ...draft, building: e.target.value })}
        />
      </label>
      <label className="field">
        <span>Places</span>
        <input
          className="input"
          type="number"
          min={1}
          required
          value={draft.capacity}
          onChange={(e) => onChange({ ...draft, capacity: e.target.value })}
        />
      </label>
      <fieldset className="field wide" style={{ border: 0, padding: 0, margin: 0 }}>
        <span>Equipment</span>
        <div className="checks">
          {EQUIPMENT.map((e) => (
            <label className="check" key={e}>
              <input
                type="checkbox"
                checked={draft.equipment.includes(e)}
                onChange={(ev) =>
                  onChange({
                    ...draft,
                    equipment: ev.target.checked ? [...draft.equipment, e] : draft.equipment.filter((x) => x !== e),
                  })
                }
              />
              {EQUIPMENT_LABEL[e]}
            </label>
          ))}
        </div>
      </fieldset>
    </>
  )
}

function Rooms() {
  const [rooms, setRooms] = useState<Room[] | null>(null)
  const [loadError, setLoadError] = useState<Error | null>(null)
  const [draft, setDraft] = useState<Draft>(EMPTY)
  const add = useAction()
  const say = useSay()

  const load = useCallback(async () => {
    try {
      setRooms(await api.rooms())
      setLoadError(null)
    } catch (e) {
      setLoadError(e as Error)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  async function submit(e: FormEvent) {
    e.preventDefault()
    const room = await add.run('add', () => api.addRoom(fromDraft(draft)))
    if (room) {
      say(`${room.name} added`)
      setDraft(EMPTY)
      load()
    }
  }

  return (
    <div className="admin-grid">
      <section>
        <h2 className="group-title">
          Rooms <span className="count">{rooms?.length ?? ''}</span>
        </h2>
        {loadError && <ErrorNote error={loadError} />}
        {rooms?.length === 0 && (
          <div className="empty">
            <h2>No rooms yet</h2>
            <p>Add the first room with the form. Students see it on the timetable right away.</p>
          </div>
        )}
        {rooms?.map((room) => <RoomRow key={room.id} room={room} onChanged={load} />)}
      </section>

      <form className="form-card" onSubmit={submit}>
        <h2>Add a room</h2>
        <RoomFields draft={draft} onChange={setDraft} />
        <ErrorNote error={add.error} onDismiss={() => add.setError(null)} />
        <div className="actions">
          <button className="btn primary" type="submit" disabled={add.busy !== null}>
            {add.busy && <Spinner />}
            Add room
          </button>
        </div>
      </form>
    </div>
  )
}

function RoomRow({ room, onChanged }: { room: Room; onChanged: () => void }) {
  const [editing, setEditing] = useState<Draft | null>(null)
  const [confirming, setConfirming] = useState(false)
  const { busy, error, setError, run } = useAction()
  const say = useSay()

  async function save(e: FormEvent) {
    e.preventDefault()
    if (!editing) return
    const updated = await run('save', () => api.editRoom(room.id, fromDraft(editing)))
    if (updated) {
      say(`${updated.name} saved`)
      setEditing(null)
      onChanged()
    }
  }

  async function remove() {
    const done = await run('remove', async () => {
      await api.removeRoom(room.id)
      return true
    })
    if (done) {
      say(`${room.name} removed. Its future bookings are cancelled.`)
      onChanged()
    }
  }

  if (editing) {
    return (
      <form className="inline-form" onSubmit={save}>
        <RoomFields draft={editing} onChange={setEditing} />
        <div className="wide">
          <ErrorNote error={error} onDismiss={() => setError(null)} />
        </div>
        <div className="actions">
          <button className="btn solid" type="submit" disabled={busy !== null}>
            {busy && <Spinner />}
            Save
          </button>
          <button className="btn" type="button" onClick={() => setEditing(null)}>
            Cancel
          </button>
        </div>
      </form>
    )
  }

  return (
    <div className="room-row">
      <b>{room.name}</b>
      <span className="building">{room.building}</span>
      <span className="cap">
        {room.capacity}
        <small>places</small>
      </span>
      <span className="kit-list">{equipmentList(room.equipment).join(' · ') || 'No equipment'}</span>
      <span className="entry-actions">
        {confirming ? (
          <>
            <button className="btn small primary" onClick={remove} disabled={busy !== null}>
              {busy && <Spinner />}
              Remove {room.name}
            </button>
            <button className="btn small" onClick={() => setConfirming(false)}>
              Keep
            </button>
          </>
        ) : (
          <>
            <button className="btn small" onClick={() => setEditing(toDraft(room))}>
              Edit
            </button>
            <button className="btn small danger" onClick={() => setConfirming(true)}>
              Remove
            </button>
          </>
        )}
      </span>
      {confirming && (
        <p className="grid-note" style={{ gridColumn: '1 / -1', marginTop: 0 }}>
          Every future booking in {room.name} is cancelled and its people are notified. Past bookings keep the room.
        </p>
      )}
      {error && (
        <div style={{ gridColumn: '1 / -1' }}>
          <ErrorNote error={error} onDismiss={() => setError(null)} />
        </div>
      )}
    </div>
  )
}

function Bookings() {
  const [date, setDate] = useState(today())
  const [bookings, setBookings] = useState<AdminBooking[] | null>(null)
  const [loadError, setLoadError] = useState<Error | null>(null)
  const { busy, error, setError, run } = useAction()
  const say = useSay()

  const load = useCallback(async () => {
    setBookings(null)
    try {
      setBookings(await api.adminBookings(date))
      setLoadError(null)
    } catch (e) {
      setLoadError(e as Error)
    }
  }, [date])

  useEffect(() => {
    load()
  }, [load])

  async function cancel(b: AdminBooking) {
    const updated = await run('cancel' + b.id, () => api.adminCancel(b.id))
    if (updated) {
      say(`Booking in ${b.room} cancelled`)
      setBookings((all) => all?.map((x) => (x.id === updated.id ? updated : x)) ?? all)
    }
  }

  return (
    <section>
      <div className="group-title" style={{ justifyContent: 'space-between', flexWrap: 'wrap' }}>
        <span>{longDate(date)}</span>
        <nav className="days" aria-label="Day">
          <button className="icon-btn" onClick={() => setDate(addDays(date, -1))} aria-label="Previous day">
            <ChevronLeft size={18} aria-hidden />
          </button>
          <button aria-pressed={date === today()} onClick={() => setDate(today())}>
            Today
            <b>
              {weekday(today())} {dayOfMonth(today())}
            </b>
          </button>
          <button className="icon-btn" onClick={() => setDate(addDays(date, 1))} aria-label="Next day">
            <ChevronRight size={18} aria-hidden />
          </button>
        </nav>
      </div>
      {loadError && <ErrorNote error={loadError} />}
      <ErrorNote error={error} onDismiss={() => setError(null)} />
      {bookings?.length === 0 && (
        <div className="empty">
          <h2>No bookings this day</h2>
          <p>Bookings and blocks for this day show up here, cancelled ones too.</p>
        </div>
      )}
      {bookings && bookings.length > 0 && (
        <div className="table-scroll">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Time</th>
                <th>Room</th>
                <th>Owner</th>
                <th>People</th>
                <th>Policy</th>
                <th>Status</th>
                <th>
                  <span className="visually-hidden">Actions</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {bookings.map((b) => (
                <tr key={b.id} className={b.status === 'Cancelled' ? 'cancelled' : undefined}>
                  <td className="time">
                    {shortTime(b.start)} to {shortTime(b.end)}
                  </td>
                  <td>{b.room}</td>
                  <td>{b.ownerEmail ?? <em>{b.note || 'Blocked'}</em>}</td>
                  <td>
                    <ul className="people-list">
                      {b.participants.map((p) => (
                        <li key={p.id}>
                          {p.displayName} <small>{p.email}</small>
                          {p.status !== 'Joined' && <small> · {p.status.toLowerCase()}</small>}
                        </li>
                      ))}
                    </ul>
                  </td>
                  <td>{b.ownerEmail ? POLICY_LABEL[b.joinPolicy] : 'Block'}</td>
                  <td>
                    {b.status === 'Cancelled' ? (
                      <span className="status-tag red">Cancelled{b.reason ? `, ${REASON[b.reason]}` : ''}</span>
                    ) : (
                      'Active'
                    )}
                  </td>
                  <td>
                    {b.status === 'Active' && (
                      <button className="btn small danger" disabled={busy !== null} onClick={() => cancel(b)}>
                        {busy === 'cancel' + b.id && <Spinner />}
                        Cancel
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
