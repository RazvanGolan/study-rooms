import { createContext, useCallback, useContext, useState, type ReactNode } from 'react'
import { Check, X } from 'lucide-react'

interface Toast {
  id: number
  text: string
}

const ToastContext = createContext<{
  toasts: Toast[]
  say: (text: string) => void
  dismiss: (id: number) => void
}>({ toasts: [], say: () => {}, dismiss: () => {} })

let nextId = 1

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const dismiss = useCallback((id: number) => setToasts((all) => all.filter((t) => t.id !== id)), [])

  const say = useCallback(
    (text: string) => {
      const id = nextId++
      setToasts((all) => [...all.slice(-2), { id, text }])
      window.setTimeout(() => dismiss(id), 4500)
    },
    [dismiss],
  )

  return <ToastContext.Provider value={{ toasts, say, dismiss }}>{children}</ToastContext.Provider>
}

export function useSay() {
  return useContext(ToastContext).say
}

export function Toasts() {
  const { toasts, dismiss } = useContext(ToastContext)
  return (
    <div className="toasts" role="status" aria-live="polite">
      {toasts.map((t) => (
        <div className="toast" key={t.id}>
          <Check size={18} strokeWidth={2.5} aria-hidden />
          <span>{t.text}</span>
          <button className="icon-btn" onClick={() => dismiss(t.id)} aria-label="Dismiss">
            <X size={16} aria-hidden />
          </button>
        </div>
      ))}
    </div>
  )
}
