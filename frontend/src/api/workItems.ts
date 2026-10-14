import { currentSession } from '../composables/useSession'
import type {
  AdminWorkItem,
  AdminWorkItemPage,
  ConfirmResult,
  RevokeResult,
  Role,
  Session,
  WorkItemDetail,
  WorkItemPage,
  WorkItemWrite,
} from './types'

export class ApiError extends Error {
  status: number
  error: string
  field?: string

  constructor(status: number, error: string, field?: string) {
    super(error)
    this.name = 'ApiError'
    this.status = status
    this.error = error
    this.field = field
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers)
  const session = currentSession()
  if (session) {
    headers.set('X-Display-Name', session.displayName)
    headers.set('X-Role', session.role)
  }
  if (init?.body) headers.set('Content-Type', 'application/json')
  const response = await fetch(path, { ...init, headers })
  if (response.status === 204) return undefined as T
  const data = await response.json().catch(() => ({}))
  if (!response.ok) throw new ApiError(response.status, data.error ?? 'error', data.field)
  return data as T
}

export function createSession(body: { displayName: string; role: Role }) {
  return request<Session>('/api/session', { method: 'POST', body: JSON.stringify(body) })
}

export function listWorkItems(page: number, direction: 'asc' | 'desc') {
  return request<WorkItemPage>(`/api/work-items?page=${page}&direction=${direction}`)
}

export function getWorkItem(id: number) {
  return request<WorkItemDetail>(`/api/work-items/${id}`)
}

export function confirmWorkItems(ids: number[]) {
  return request<ConfirmResult>('/api/work-items/confirm', { method: 'POST', body: JSON.stringify({ ids }) })
}

export function revokeWorkItem(id: number) {
  return request<RevokeResult>(`/api/work-items/${id}/revoke`, { method: 'POST' })
}

export function listAdminWorkItems(page: number, direction: 'asc' | 'desc') {
  return request<AdminWorkItemPage>(`/api/admin/work-items?page=${page}&direction=${direction}`)
}

export function getAdminWorkItem(id: number) {
  return request<AdminWorkItem>(`/api/admin/work-items/${id}`)
}

export function createWorkItem(body: WorkItemWrite) {
  return request<AdminWorkItem>('/api/admin/work-items', { method: 'POST', body: JSON.stringify(body) })
}

export function updateWorkItem(id: number, body: WorkItemWrite) {
  return request<AdminWorkItem>(`/api/admin/work-items/${id}`, { method: 'PUT', body: JSON.stringify(body) })
}

export function deleteWorkItem(id: number) {
  return request<void>(`/api/admin/work-items/${id}`, { method: 'DELETE' })
}
