import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { getAdminWorkItem, listAdminWorkItems, updateWorkItem } from './apiMock'
import { mountAt, signAs } from './mount'
import { resetSession } from '../src/composables/useSession'

vi.mock('../src/api/workItems', () => import('./apiMock'))

const stamp = '2026-01-02T03:04:05.000Z'
const oldItem = { id: 1, title: 'Old', description: 'Desc', createdAt: stamp, updatedAt: stamp }

beforeEach(() => {
  localStorage.clear()
  resetSession()
  vi.clearAllMocks()
  signAs('Admin', 'admin')
})

describe('admin edit', () => {
  it('prefills the form, blocks a blank title, and shows the new title', async () => {
    listAdminWorkItems.mockResolvedValue({ page: 1, pageSize: 10, total: 1, direction: 'desc', items: [oldItem] })
    getAdminWorkItem.mockResolvedValue(oldItem)
    const { wrapper, router } = await mountAt('/admin/work-items')
    await wrapper.findAll('a').find((link) => link.text() === '編輯')!.trigger('click')
    await flushPromises()
    expect(router.currentRoute.value.path).toBe('/admin/work-items/1/edit')
    expect((wrapper.get('input[name="title"]').element as HTMLInputElement).value).toBe('Old')
    expect((wrapper.get('textarea[name="description"]').element as HTMLTextAreaElement).value).toBe('Desc')
    await wrapper.get('input[name="title"]').setValue('   ')
    await wrapper.get('form').trigger('submit')
    expect(updateWorkItem).not.toHaveBeenCalled()
    expect(wrapper.text()).toContain('標題不得為空')
    expect(router.currentRoute.value.path).toBe('/admin/work-items/1/edit')
    listAdminWorkItems.mockResolvedValue({
      page: 1,
      pageSize: 10,
      total: 1,
      direction: 'desc',
      items: [{ ...oldItem, title: 'New' }],
    })
    await wrapper.get('input[name="title"]').setValue('New')
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(updateWorkItem).toHaveBeenCalledWith(1, { title: 'New', description: 'Desc' })
    expect(wrapper.text()).toContain('更新成功')
    expect(wrapper.text()).toContain('New')
  })
})
