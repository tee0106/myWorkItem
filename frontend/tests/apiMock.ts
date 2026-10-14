import { vi } from 'vitest'

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

const emptyUser = { page: 1, pageSize: 10 as const, total: 0, direction: 'desc' as const, items: [] }
const emptyAdmin = { page: 1, pageSize: 10 as const, total: 0, direction: 'desc' as const, items: [] }

export const createSession = vi.fn(async (body: { displayName: string; role: 'user' | 'admin' }) => body)
export const listWorkItems = vi.fn(async () => emptyUser)
export const getWorkItem = vi.fn()
export const confirmWorkItems = vi.fn()
export const revokeWorkItem = vi.fn()
export const listAdminWorkItems = vi.fn(async () => emptyAdmin)
export const getAdminWorkItem = vi.fn()
export const createWorkItem = vi.fn()
export const updateWorkItem = vi.fn()
export const deleteWorkItem = vi.fn()
