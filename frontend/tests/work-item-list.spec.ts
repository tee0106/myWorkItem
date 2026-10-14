import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { listWorkItems } from './apiMock'
import { mountAt, signAs } from './mount'
import { resetSession } from '../src/composables/useSession'

vi.mock('../src/api/workItems', () => import('./apiMock'))

beforeEach(() => {
  localStorage.clear()
  resetSession()
  vi.clearAllMocks()
  signAs('A', 'user')
})

describe('work item list', () => {
  it('shows the columns and the empty sentence', async () => {
    listWorkItems.mockResolvedValue({ page: 1, pageSize: 10, total: 0, direction: 'desc', items: [] })
    const { wrapper } = await mountAt('/work-items')
    expect(wrapper.text()).toContain('編號')
    expect(wrapper.text()).toContain('標題')
    expect(wrapper.text()).toContain('狀態')
    expect(wrapper.text()).toContain('目前無待辦項目')
  })

  it('toggles sort and shows only the second page', async () => {
    listWorkItems.mockImplementation(async (page: number, direction: string) => ({
      page,
      pageSize: 10,
      total: 11,
      direction,
      items: page === 1
        ? [{ id: 2, title: 'Newer', status: 'pending', createdAt: '2026-01-02T00:00:00.000Z' }]
        : [{ id: 1, title: 'Older', status: 'pending', createdAt: '2026-01-01T00:00:00.000Z' }],
    }))
    const { wrapper, router } = await mountAt('/work-items')
    expect(wrapper.text()).toContain('Newer')
    await wrapper.get('button[name="sort"]').trigger('click')
    await flushPromises()
    expect(router.currentRoute.value.query.direction).toBe('asc')
    await wrapper.get('button[name="next"]').trigger('click')
    await flushPromises()
    expect(router.currentRoute.value.query.page).toBe('2')
    expect(wrapper.text()).toContain('Older')
    expect(wrapper.text()).not.toContain('Newer')
  })
})
