import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { createWorkItem, listAdminWorkItems } from './apiMock'
import { mountAt, signAs } from './mount'
import { resetSession } from '../src/composables/useSession'

vi.mock('../src/api/workItems', () => import('./apiMock'))

const stamp = '2026-01-02T03:04:05.000Z'

beforeEach(() => {
  localStorage.clear()
  resetSession()
  vi.clearAllMocks()
})

describe('admin create', () => {
  it('blocks a blank title, then shows the new row first', async () => {
    signAs('Admin', 'admin')
    listAdminWorkItems.mockResolvedValue({
      page: 1,
      pageSize: 10,
      total: 1,
      direction: 'desc',
      items: [{ id: 1, title: 'First', description: 'Notes', createdAt: stamp, updatedAt: stamp }],
    })
    createWorkItem.mockResolvedValue({ id: 1, title: 'First', description: 'Notes', createdAt: stamp, updatedAt: stamp })
    const { wrapper, router } = await mountAt('/admin/work-items/new')
    await wrapper.get('form').trigger('submit')
    expect(wrapper.text()).toContain('標題不得為空')
    await wrapper.get('input[name="title"]').setValue('   ')
    await wrapper.get('form').trigger('submit')
    expect(createWorkItem).not.toHaveBeenCalled()
    await wrapper.get('input[name="title"]').setValue('  First  ')
    await wrapper.get('textarea[name="description"]').setValue('Notes')
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(createWorkItem).toHaveBeenCalledWith({ title: 'First', description: 'Notes' })
    expect(router.currentRoute.value.path).toBe('/admin/work-items')
    expect(wrapper.text()).toContain('新增成功')
    expect(wrapper.text()).toContain('First')
    expect(wrapper.text()).toContain(new Date(stamp).toLocaleString())
  })

  it('hides add, edit, and delete from a front-end user', async () => {
    signAs('A', 'user')
    const { wrapper, router } = await mountAt('/admin/work-items')
    expect(router.currentRoute.value.path).toBe('/work-items')
    expect(wrapper.text()).not.toContain('新增')
    expect(wrapper.text()).not.toContain('編輯')
    expect(wrapper.text()).not.toContain('刪除')
  })
})
