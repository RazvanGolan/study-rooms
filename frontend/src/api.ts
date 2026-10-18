// Everything the interface knows about the API: the shapes it sends back, and one helper that
// adds the token to each request and refreshes it when it runs out.

export const API_URL = (import.meta.env.VITE_API_URL ?? 'http://localhost:5080').replace(/\/$/, '')

export type JoinPolicy = 'Private' | 'Open' | 'OnRequest'
export type BookingStatus = 'Active' | 'Cancelled'
export type ParticipantStatus = 'Requested' | 'Joined' | 'Declined' | 'Left' | 'Removed'
export type CancelReason = 'ByOwner' | 'ByAdmin' | 'RoomRemoved'

// Equipment is a [Flags] enum in C#. With JsonStringEnumConverter it arrives as "Whiteboard, PowerOutlets",
// or as "None", and goes back the same way.
export const EQUIPMENT = ['Projector', 'Whiteboard', 'Tv', 'PowerOutlets'] as const
export type EquipmentName = (typeof EQUIPMENT)[number]

export interface Me {
  id: string
  email: string
  displayName: string
  isAdmin: boolean
}

export interface Room {
  id: number
  name: string
  building: string
  capacity: number
  equipment: string
}

export interface Person {
  id: string
  displayName: string
}

export interface Booking {
  id: number
  roomId: number
  room: string
  date: string
  start: string
  end: string
  owner: string | null
  note: string | null
  people: Person[]
  requests: Person[]
  placesLeft: number
  joinPolicy: JoinPolicy
  status: BookingStatus
  reason: CancelReason | null
  myStatus: ParticipantStatus | null
}

export interface AdminParticipant {
  id: string
  displayName: string
  email: string
  status: ParticipantStatus
}

export interface AdminBooking {
  id: number
  roomId: number
  room: string
  date: string
  start: string
  end: string
  ownerEmail: string | null
  note: string | null
  participants: AdminParticipant[]
  joinPolicy: JoinPolicy
  status: BookingStatus
  reason: CancelReason | null
}

interface Tokens {
  accessToken: string
  refreshToken: string
  expiresAt: number
}

// An error the API refused with. Title and problems come from its ErrorResponse when it sent one.
export class ApiError extends Error {
  status: number
  problems: string[]

  constructor(status: number, title: string, problems: string[]) {
    super(title)
    this.status = status
    this.problems = problems
  }
}

const TOKENS_KEY = 'study-rooms.tokens'

function readTokens(): Tokens | null {
  try {
    const saved = localStorage.getItem(TOKENS_KEY)
    return saved ? (JSON.parse(saved) as Tokens) : null
  } catch {
    return null
  }
}

function saveTokens(answer: { accessToken: string; refreshToken: string; expiresIn: number }) {
  const tokens: Tokens = {
    accessToken: answer.accessToken,
    refreshToken: answer.refreshToken,
    // Refresh a minute early, so a request never leaves with a token that expires on the way.
    expiresAt: Date.now() + (answer.expiresIn - 60) * 1000,
  }
  try {
    localStorage.setItem(TOKENS_KEY, JSON.stringify(tokens))
  } catch {
    // Private windows may refuse storage; the session then lasts until the page is reloaded.
  }
  memoryTokens = tokens
}

let memoryTokens: Tokens | null = readTokens()
let signedOutListener: (() => void) | null = null

export function hasSession() {
  return memoryTokens !== null
}

export function onSignedOut(listener: () => void) {
  signedOutListener = listener
}

export function signOut() {
  memoryTokens = null
  try {
    localStorage.removeItem(TOKENS_KEY)
  } catch {
    // nothing to clear
  }
}

async function toError(response: Response): Promise<ApiError> {
  try {
    const body = await response.json()
    if (body && typeof body.title === 'string') {
      // Our own ErrorResponse has "problems"; ASP.NET's validation answer has "errors" by field.
      let problems: string[] = Array.isArray(body.problems) ? body.problems : []
      if (problems.length === 0 && body.errors && typeof body.errors === 'object') {
        problems = Object.values(body.errors as Record<string, string[]>).flat()
      }
      return new ApiError(response.status, body.title, problems)
    }
  } catch {
    // not JSON
  }
  const titles: Record<number, string> = {
    401: 'You are not logged in.',
    403: 'You are not allowed to do that.',
    404: 'That does not exist any more.',
  }
  return new ApiError(response.status, titles[response.status] ?? `The API answered ${response.status}.`, [])
}

let refreshing: Promise<boolean> | null = null

async function refresh(): Promise<boolean> {
  if (!memoryTokens) return false
  if (!refreshing) {
    const refreshToken = memoryTokens.refreshToken
    refreshing = fetch(`${API_URL}/account/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    })
      .then(async (response) => {
        if (!response.ok) return false
        saveTokens(await response.json())
        return true
      })
      .catch(() => false)
      .finally(() => {
        refreshing = null
      })
  }
  return refreshing
}

async function send(method: string, path: string, body?: unknown): Promise<Response> {
  const headers: Record<string, string> = {}
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  if (memoryTokens) headers['Authorization'] = `Bearer ${memoryTokens.accessToken}`
  try {
    return await fetch(`${API_URL}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ApiError(0, 'The API is not reachable.', [
      `Nothing answered at ${API_URL}. Start it with dotnet run --project src/StudyRooms.Api.`,
    ])
  }
}

export async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  if (memoryTokens && Date.now() > memoryTokens.expiresAt) {
    await refresh()
  }
  let response = await send(method, path, body)
  if (response.status === 401 && memoryTokens && (await refresh())) {
    response = await send(method, path, body)
  }
  if (response.status === 401 && memoryTokens) {
    signOut()
    signedOutListener?.()
  }
  if (!response.ok) throw await toError(response)
  if (response.status === 204) return undefined as T
  const text = await response.text()
  return (text ? JSON.parse(text) : undefined) as T
}

export async function login(email: string, password: string) {
  const response = await send('POST', '/account/login', { email, password })
  if (!response.ok) throw await toError(response)
  saveTokens(await response.json())
}

export async function register(email: string, password: string, displayName: string) {
  const response = await send('POST', '/account/register', { email, password, displayName })
  if (!response.ok) throw await toError(response)
  await login(email, password)
}

export const api = {
  me: () => request<Me>('GET', '/me'),
  myBookings: () => request<Booking[]>('GET', '/me/bookings'),
  rooms: () => request<Room[]>('GET', '/rooms'),
  timetable: (date: string) => request<Booking[]>('GET', `/timetable?date=${date}`),
  booking: (id: number) => request<Booking>('GET', `/bookings/${id}`),
  book: (roomId: number, date: string, start: string, end: string, joinPolicy: JoinPolicy) =>
    request<Booking>('POST', '/bookings', { roomId, date, start, end, joinPolicy }),
  join: (id: number) => request<Booking>('POST', `/bookings/${id}/join`),
  leave: (id: number) => request<Booking>('POST', `/bookings/${id}/leave`),
  cancel: (id: number) => request<Booking>('POST', `/bookings/${id}/cancel`),
  approve: (id: number, personId: string) =>
    request<Booking>('POST', `/bookings/${id}/participants/${encodeURIComponent(personId)}/approve`),
  decline: (id: number, personId: string) =>
    request<Booking>('POST', `/bookings/${id}/participants/${encodeURIComponent(personId)}/decline`),
  remove: (id: number, personId: string) =>
    request<Booking>('POST', `/bookings/${id}/participants/${encodeURIComponent(personId)}/remove`),

  adminBookings: (date: string) => request<AdminBooking[]>('GET', `/admin/bookings?date=${date}`),
  adminBook: (roomId: number, date: string, start: string, end: string, studentEmail: string, joinPolicy: JoinPolicy) =>
    request<AdminBooking>('POST', '/admin/bookings', { roomId, date, start, end, studentEmail, joinPolicy }),
  adminBlock: (roomId: number, date: string, start: string, end: string, note: string) =>
    request<AdminBooking>('POST', '/admin/bookings/blocks', { roomId, date, start, end, note }),
  adminCancel: (id: number) => request<AdminBooking>('POST', `/admin/bookings/${id}/cancel`),
  addRoom: (room: Omit<Room, 'id'>) => request<Room>('POST', '/admin/rooms', room),
  editRoom: (id: number, room: Omit<Room, 'id'>) => request<Room>('PUT', `/admin/rooms/${id}`, room),
  removeRoom: (id: number) => request<void>('DELETE', `/admin/rooms/${id}`),
}
