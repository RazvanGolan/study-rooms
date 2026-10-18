import { useCallback, useEffect, useState } from 'react'
import { LogOut } from 'lucide-react'
import { api, ApiError, hasSession, onSignedOut, signOut, type Me } from './api'
import { Login } from './views/Login'
import { Timetable } from './views/Timetable'
import { MyBookings } from './views/MyBookings'
import { Admin } from './views/Admin'
import { Toasts, ToastProvider } from './toast'
import { ErrorNote } from './ui'

type Route = 'timetable' | 'mine' | 'admin'

function readRoute(): Route {
  const hash = window.location.hash.replace(/^#\/?/, '')
  if (hash === 'mine' || hash === 'admin') return hash
  return 'timetable'
}

export default function App() {
  const [me, setMe] = useState<Me | null>(null)
  const [loading, setLoading] = useState(hasSession())
  const [error, setError] = useState<ApiError | null>(null)
  const [route, setRoute] = useState<Route>(readRoute)
  // Booking ids to open from "My bookings" in the timetable: the day and the booking.
  const [focus, setFocus] = useState<{ date: string; id: number } | null>(null)

  const loadMe = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setMe(await api.me())
    } catch (e) {
      if (e instanceof ApiError && e.status !== 401) setError(e)
      setMe(null)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    if (hasSession()) loadMe()
    onSignedOut(() => setMe(null))
    const onHash = () => setRoute(readRoute())
    window.addEventListener('hashchange', onHash)
    return () => window.removeEventListener('hashchange', onHash)
  }, [loadMe])

  function go(next: Route) {
    window.location.hash = `/${next}`
    setRoute(next)
  }

  function logOut() {
    signOut()
    setMe(null)
    go('timetable')
  }

  if (loading) {
    return (
      <div className="boot" aria-busy="true">
        <span className="brand">Study Rooms</span>
      </div>
    )
  }

  if (error) {
    return (
      <main className="boot">
        <span className="brand">Study Rooms</span>
        <ErrorNote error={error} />
        <button className="btn" onClick={loadMe}>
          Try again
        </button>
      </main>
    )
  }

  if (!me) {
    return (
      <ToastProvider>
        <Login onSignedIn={loadMe} />
        <Toasts />
      </ToastProvider>
    )
  }

  const current = route === 'admin' && !me.isAdmin ? 'timetable' : route

  return (
    <ToastProvider>
      <div className="shell">
        <header className="topbar">
          <a className="brand" href="#/timetable" onClick={() => go('timetable')}>
            Study Rooms
          </a>
          <nav className="tabs" aria-label="Main">
            <a href="#/timetable" aria-current={current === 'timetable' ? 'page' : undefined}>
              Timetable
            </a>
            <a href="#/mine" aria-current={current === 'mine' ? 'page' : undefined}>
              My bookings
            </a>
            {me.isAdmin && (
              <a href="#/admin" aria-current={current === 'admin' ? 'page' : undefined}>
                Admin
              </a>
            )}
          </nav>
          <div className="who">
            <span className="who-name">
              {me.displayName}
              {me.isAdmin && <span className="who-role">admin</span>}
            </span>
            <button className="icon-btn" onClick={logOut} aria-label="Log out" title="Log out">
              <LogOut size={18} strokeWidth={2} aria-hidden />
            </button>
          </div>
        </header>

        {current === 'timetable' && <Timetable me={me} focus={focus} onFocusDone={() => setFocus(null)} />}
        {current === 'mine' && (
          <MyBookings
            me={me}
            onOpen={(date, id) => {
              setFocus({ date, id })
              go('timetable')
            }}
          />
        )}
        {current === 'admin' && me.isAdmin && <Admin />}
      </div>
      <Toasts />
    </ToastProvider>
  )
}
