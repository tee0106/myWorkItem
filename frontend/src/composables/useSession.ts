import { ref } from 'vue'
import type { Session } from '../api/types'

const KEY = 'work-item-session'
const session = ref<Session | null>(load())

function load(): Session | null {
  const raw = localStorage.getItem(KEY)
  if (!raw) return null
  try {
    const data = JSON.parse(raw) as Session
    if (!data.displayName || (data.role !== 'user' && data.role !== 'admin')) return null
    return { displayName: data.displayName, role: data.role }
  } catch {
    return null
  }
}

export function currentSession() {
  return session.value
}

export function resetSession() {
  session.value = load()
}

export function useSession() {
  function signIn(next: Session) {
    const stored = { displayName: next.displayName, role: next.role }
    localStorage.setItem(KEY, JSON.stringify(stored))
    session.value = stored
  }

  function signOut() {
    localStorage.removeItem(KEY)
    session.value = null
  }

  return { session, signIn, signOut }
}
