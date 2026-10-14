import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, type VueWrapper } from '@vue/test-utils'
import { ApiError, confirmWorkItems, listWorkItems, revokeWorkItem } from './apiMock'
import { mountAt, signAs } from './mount'
import { resetSession } from '../src/composables/useSession'

vi.mock('../src/api/workItems', () => import('./apiMock'))

const created = '2026-01-02T00:00:00.000Z'

function row(id: number, title: string, status: 'pending' | 'confirmed') {
  return { id, title, status, createdAt: created }
}

function page(items: ReturnType<typeof row>[]) {
  return { page: 1, pageSize: 10 as const, total: items.length, direction: 'desc' as const, items }
}

function confirmButton(wrapper: VueWrapper) {
  return wrapper.findAll('button').find((button) => button.text() === '確認')!
}

beforeEach(() => {
  localStorage.clear()
  resetSession()
  vi.clearAllMocks()
  signAs('A', 'user')
})

describe('confirm and revoke', () => {
  it('highlights checked rows, selects the current page, and confirms them', async () => {
    let status: 'pending' | 'confirmed' = 'pending'
    listWorkItems.mockImplementation(async () => page([row(1, 'One', status), row(2, 'Two', status)]))
    confirmWorkItems.mockImplementation(async () => {
      status = 'confirmed'
      return { confirmedCount: 2 }
    })
    const { wrapper } = await mountAt('/work-items')
    expect(confirmButton(wrapper).attributes('disabled')).toBeDefined()
    const boxes = () => wrapper.findAll('tbody input[type="checkbox"]')
    await boxes()[0].setChecked(true)
    expect(wrapper.findAll('tbody tr')[0].classes()).toContain('bg-base-200')
    const header = wrapper.get('input[aria-label="全選"]').element as HTMLInputElement
    expect(header.indeterminate).toBe(true)
    await wrapper.get('input[aria-label="全選"]').trigger('change')
    await flushPromises()
    expect(boxes().every((box) => (box.element as HTMLInputElement).checked)).toBe(true)
    await wrapper.get('input[aria-label="全選"]').trigger('change')
    await flushPromises()
    expect(boxes().every((box) => (box.element as HTMLInputElement).checked)).toBe(false)
    expect(confirmButton(wrapper).attributes('disabled')).toBeDefined()
    await wrapper.get('input[aria-label="全選"]').trigger('change')
    await confirmButton(wrapper).trigger('click')
    await flushPromises()
    expect(confirmWorkItems).toHaveBeenCalledWith([1, 2])
    expect(wrapper.text()).toContain('已成功確認 2 項目')
    expect(wrapper.text()).toContain('已確認')
    expect(boxes().every((box) => (box.element as HTMLInputElement).checked)).toBe(false)
    expect(localStorage.getItem('work-item-session')).not.toContain('"1"')
  })

  it('revokes after confirm and keeps the status when cancelled or the save fails', async () => {
    let status: 'pending' | 'confirmed' = 'confirmed'
    listWorkItems.mockImplementation(async () => page([row(1, 'One', status)]))
    const { wrapper } = await mountAt('/work-items')
    const revoke = wrapper.findAll('button').find((button) => button.text() === '撤銷確認')!
    await revoke.trigger('click')
    expect(wrapper.text()).toContain('確定要將此項目標記回『待確認』嗎？')
    const cancel = wrapper.findAll('button').find((button) => button.text() === '取消')!
    await cancel.trigger('click')
    expect(revokeWorkItem).not.toHaveBeenCalled()
    expect(wrapper.text()).toContain('已確認')

    await wrapper.findAll('button').find((button) => button.text() === '撤銷確認')!.trigger('click')
    revokeWorkItem.mockRejectedValueOnce(new ApiError(500, 'fail'))
    await wrapper.get('button[name="confirm-revoke"]').trigger('click')
    await flushPromises()
    expect(wrapper.text()).toContain('更新失敗，狀態未變更')
    expect(wrapper.text()).toContain('已確認')

    status = 'pending'
    listWorkItems.mockImplementation(async () => page([row(1, 'One', 'pending')]))
    await wrapper.findAll('button').find((button) => button.text() === '撤銷確認')!.trigger('click')
    revokeWorkItem.mockResolvedValueOnce({ revokedCount: 1 })
    await wrapper.get('button[name="confirm-revoke"]').trigger('click')
    await flushPromises()
    expect(wrapper.text()).toContain('已將 1 項目標記為待確認')
    expect(wrapper.text()).toContain('待確認')
    expect(wrapper.findAll('button').some((button) => button.text() === '撤銷確認')).toBe(false)
  })

  it('keeps the previous status when confirm fails, and drops a missing item', async () => {
    listWorkItems.mockResolvedValueOnce(page([row(1, 'One', 'pending')]))
    confirmWorkItems.mockRejectedValueOnce(new ApiError(500, 'fail'))
    const failed = await mountAt('/work-items')
    await failed.wrapper.get('tbody input[type="checkbox"]').setChecked(true)
    await confirmButton(failed.wrapper).trigger('click')
    await flushPromises()
    expect(failed.wrapper.text()).toContain('更新失敗，狀態未變更')
    expect(failed.wrapper.text()).toContain('待確認')
    failed.wrapper.unmount()

    listWorkItems.mockResolvedValueOnce(page([row(9, 'Gone', 'pending')]))
    listWorkItems.mockResolvedValueOnce(page([]))
    confirmWorkItems.mockRejectedValueOnce(new ApiError(404, 'not_found'))
    const missing = await mountAt('/work-items')
    await missing.wrapper.get('tbody input[type="checkbox"]').setChecked(true)
    await confirmButton(missing.wrapper).trigger('click')
    await flushPromises()
    expect(missing.wrapper.text()).toContain('找不到此項目')
    expect(missing.wrapper.text()).not.toContain('Gone')
  })
})
