import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { ApiError, deleteWorkItem, getWorkItem, listAdminWorkItems } from './apiMock'
import { mountAt, signAs } from './mount'
import { resetSession } from '../src/composables/useSession'

vi.mock('../src/api/workItems', () => import('./apiMock'))

const stamp = '2026-01-02T03:04:05.000Z'
const item = { id: 4, title: 'Gone', description: '', createdAt: stamp, updatedAt: stamp }

beforeEach(() => {
  localStorage.clear()
  resetSession()
  vi.clearAllMocks()
  signAs('Admin', 'admin')
})

describe('admin delete', () => {
  it('cancels, deletes, and keeps the row when delete fails', async () => {
    listAdminWorkItems.mockResolvedValue({ page: 1, pageSize: 10, total: 1, direction: 'desc', items: [item] })
    const { wrapper } = await mountAt('/admin/work-items')
    await wrapper.get('button[name="delete"]').trigger('click')
    expect(wrapper.text()).toContain('確定要刪除此項目嗎？')
    await wrapper.get('button[name="cancel-delete"]').trigger('click')
    expect(deleteWorkItem).not.toHaveBeenCalled()
    expect(wrapper.text()).toContain('Gone')

    deleteWorkItem.mockRejectedValueOnce(new ApiError(500, 'fail'))
    await wrapper.get('button[name="delete"]').trigger('click')
    await wrapper.get('button[name="confirm-delete"]').trigger('click')
    await flushPromises()
    expect(wrapper.text()).toContain('刪除失敗，項目仍保留')
    expect(wrapper.text()).toContain('Gone')

    deleteWorkItem.mockImplementationOnce(async () => {
      listAdminWorkItems.mockResolvedValue({ page: 1, pageSize: 10, total: 0, direction: 'desc', items: [] })
    })
    await wrapper.get('button[name="delete"]').trigger('click')
    await wrapper.get('button[name="confirm-delete"]').trigger('click')
    await flushPromises()
    expect(wrapper.text()).toContain('刪除成功')
    expect(wrapper.text()).not.toContain('Gone')
  })

  it('shows not found for a deleted detail address', async () => {
    signAs('A', 'user')
    getWorkItem.mockRejectedValue(new ApiError(404, 'not_found'))
    const { wrapper } = await mountAt('/work-items/4')
    expect(wrapper.text()).toContain('找不到此項目')
    expect(wrapper.text()).toContain('返回列表')
  })
})
