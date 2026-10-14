import { beforeEach, describe, expect, it, vi } from 'vitest'
import { listWorkItems } from './apiMock'
import { mountAt, signAs } from './mount'
import { resetSession } from '../src/composables/useSession'

vi.mock('../src/api/workItems', () => import('./apiMock'))

beforeEach(() => {
  localStorage.clear()
  resetSession()
  vi.clearAllMocks()
})

describe('confirmation persistence', () => {
  it('restores 已確認 for the same name and 待確認 for another name, with checkboxes unchecked', async () => {
    signAs('A', 'user')
    listWorkItems.mockResolvedValue({
      page: 1,
      pageSize: 10,
      total: 1,
      direction: 'desc',
      items: [{ id: 1, title: 'One', status: 'confirmed', createdAt: '2026-01-02T00:00:00.000Z' }],
    })
    const first = await mountAt('/work-items')
    expect(first.wrapper.text()).toContain('已確認')
    expect((first.wrapper.get('tbody input').element as HTMLInputElement).checked).toBe(false)
    first.wrapper.unmount()

    signAs('B', 'user')
    listWorkItems.mockResolvedValue({
      page: 1,
      pageSize: 10,
      total: 1,
      direction: 'desc',
      items: [{ id: 1, title: 'One', status: 'pending', createdAt: '2026-01-02T00:00:00.000Z' }],
    })
    const second = await mountAt('/work-items')
    expect(second.wrapper.text()).toContain('待確認')
    expect((second.wrapper.get('tbody input').element as HTMLInputElement).checked).toBe(false)
  })
})
