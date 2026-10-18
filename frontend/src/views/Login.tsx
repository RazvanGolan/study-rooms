import { useState, type FormEvent } from 'react'
import { API_URL, login, register } from '../api'
import { ErrorNote, Spinner, useAction } from '../ui'

// The sample accounts the API creates on a developer's computer (see the README).
const SAMPLES = [
  { email: 'ana.popescu@example.com', name: 'Ana P.' },
  { email: 'mihai.ionescu@example.com', name: 'Mihai I.' },
  { email: 'ioana.dumitru@example.com', name: 'Ioana D.' },
  { email: 'radu.stan@example.com', name: 'Radu S.' },
  { email: 'admin@example.com', name: 'Admin', admin: true },
]

const LOCAL_API = /\/\/(localhost|127\.0\.0\.1)(:|\/|$)/.test(API_URL)

export function Login({ onSignedIn }: { onSignedIn: () => void }) {
  const [mode, setMode] = useState<'login' | 'signup'>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [displayName, setDisplayName] = useState('')
  const { busy, error, setError, run } = useAction()

  async function submit(e: FormEvent) {
    e.preventDefault()
    const done = await run('submit', async () => {
      if (mode === 'login') await login(email.trim(), password)
      else await register(email.trim(), password, displayName.trim())
      return true
    })
    if (done) onSignedIn()
  }

  function switchTo(next: 'login' | 'signup') {
    setMode(next)
    setError(null)
  }

  return (
    <main className="login">
      <section className="login-poster" aria-hidden="true">
        <div>
          <h1>
            Study
            <br />
            Rooms<span>.</span>
          </h1>
          <p>Book a room at the faculty, alone or with others, and see who else is coming.</p>
        </div>
        <div>
          <div className="poster-grid">
            {['08', '09', '10', '11', '12', '13', '14', '15', '16', '17', '18', '19'].map((h) => (
              <span key={h}>{h}</span>
            ))}
          </div>
          <div className="poster-blocks">
            <i style={{ left: '16.6%', width: '16.6%' }} />
            <i className="r" style={{ left: '50%', width: '12.5%' }} />
            <i className="o" style={{ left: '25%', width: '20.8%', top: 34 }} />
            <i style={{ left: '62.5%', width: '16.6%', top: 34 }} />
          </div>
        </div>
      </section>

      <section className="login-form">
        <h2>{mode === 'login' ? 'Log in' : 'Create an account'}</h2>
        <form onSubmit={submit}>
          <label className="field">
            <span>Email</span>
            <input
              className="input"
              type="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </label>
          {mode === 'signup' && (
            <label className="field">
              <span>Display name</span>
              <input
                className="input"
                autoComplete="nickname"
                required
                maxLength={40}
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
              />
              <small>Other students see this name, never your email.</small>
            </label>
          )}
          <label className="field">
            <span>Password</span>
            <input
              className="input"
              type="password"
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </label>
          <ErrorNote error={error} />
          <button className="btn primary" type="submit" disabled={busy !== null}>
            {busy && <Spinner />}
            {mode === 'login' ? 'Log in' : 'Create account'}
          </button>
        </form>
        <p className="login-switch">
          {mode === 'login' ? (
            <>
              No account yet?{' '}
              <button className="link-btn" onClick={() => switchTo('signup')}>
                Create one
              </button>
            </>
          ) : (
            <>
              Have an account?{' '}
              <button className="link-btn" onClick={() => switchTo('login')}>
                Log in
              </button>
            </>
          )}
        </p>
        {LOCAL_API && mode === 'login' && (
          <div className="samples">
            <p>Sample accounts on your own API. The password is Password1!</p>
            <ul>
              {SAMPLES.map((s) => (
                <li key={s.email}>
                  <button
                    type="button"
                    onClick={() => {
                      setEmail(s.email)
                      setPassword('Password1!')
                      setError(null)
                    }}
                  >
                    {s.name}
                    {s.admin && <small>admin</small>}
                  </button>
                </li>
              ))}
            </ul>
          </div>
        )}
      </section>
    </main>
  )
}
