export type Role = 'user' | 'admin'
export type Status = 'pending' | 'confirmed'

export interface Session {
  displayName: string
  role: Role
}

export interface WorkItemSummary {
  id: number
  title: string
  status: Status
  createdAt: string
}

export interface WorkItemDetail extends WorkItemSummary {
  description: string
  updatedAt: string
}

export interface WorkItemPage {
  page: number
  pageSize: 10
  total: number
  direction: 'asc' | 'desc'
  items: WorkItemSummary[]
}

export interface AdminWorkItem {
  id: number
  title: string
  description: string
  createdAt: string
  updatedAt: string
}

export interface AdminWorkItemPage {
  page: number
  pageSize: 10
  total: number
  direction: 'asc' | 'desc'
  items: AdminWorkItem[]
}

export interface WorkItemWrite {
  title: string
  description?: string
}

export interface ConfirmRequest {
  ids: number[]
}

export interface ConfirmResult {
  confirmedCount: number
}

export interface RevokeResult {
  revokedCount: 1
}

export interface ErrorBody {
  error: 'name_required' | 'invalid_role' | 'sign_in_required' | 'admin_required' | 'empty_selection' | 'not_found' | 'validation'
}

export interface FieldError {
  error: 'validation'
  field: 'title'
}
