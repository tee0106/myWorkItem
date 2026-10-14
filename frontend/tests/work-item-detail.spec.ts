import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { ApiError, getWorkItem, listWorkItems } from './apiMock'
import { mountAt, signAs } from './mount'
import { resetSession } from '../src/composables/useSession'

vi.mock('../src/api/workItems', () => import('./apiMock'))

const createdAt = '2026-01-02T03:04:05.000Z'
const updatedAt = '2026-01-03T03:04:05.000Z'

beforeEach(() => {
  localStorage.clear()
  resetSession()
  vi.clearAllMocks()
  signAs('A', 'user')
})

describe('work item detail', () => {
  it('shows the fields and returns to the same page and sort with checkboxes unchecked', async () => {
    listWorkItems.mockResolvedValue({
      page: 2,
      pageSize: 10,
      total: 11,
      direction: 'desc',
      items: [{ id: 5, title: 'Hello', status: 'confirmed', createdAt }],
    })
    getWorkItem.mockResolvedValue({
      id: 5,
      title: 'Hello',
      description: 'Body',
      status: 'confirmed',
      createdAt,
      updatedAt,
    })
    const { wrapper, router } = await mountAt('/work-items?page=2&direction=desc')
    await wrapper.get('a').trigger('click')
    await flushPromises()
    expect(router.currentRoute.value.path).toBe('/work-items/5')
    expect(wrapper.text()).toContain('編號')
    expect(wrapper.text()).toContain('5')
    expect(wrapper.text()).toContain('Hello')
    expect(wrapper.text()).toContain('Body')
    expect(wrapper.text()).toContain('已確認')
    expect(wrapper.text()).toContain('建立時間')
    expect(wrapper.text()).toContain('最後更新時間')
    expect(wrapper.text()).toContain(new Date(createdAt).toLocaleString())
    expect(wrapper.text()).toContain(new Date(updatedAt).toLocaleString())
    await wrapper.get('a').trigger('click')
    await flushPromises()
    expect(router.currentRoute.value.path).toBe('/work-items')
    expect(router.currentRoute.value.query.page).toBe('2')
    expect(router.currentRoute.value.query.direction).toBe('desc')
    expect((wrapper.get('tbody input').element as HTMLInputElement).checked).toBe(false)
  })

  it('shows not found and a way back', async () => {
    getWorkItem.mockRejectedValue(new ApiError(404, 'not_found'))
    listWorkItems.mockResolvedValue({ page: 1, pageSize: 10, total: 0, direction: 'desc', items: [] })
    const { wrapper } = await mountAt('/work-items/404')
    expect(wrapper.text()).toContain('找不到此項目')
    expect(wrapper.text()).toContain('返回列表')
  })
})
