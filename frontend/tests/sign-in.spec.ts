import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { createSession } from './apiMock'
import { mountAt } from './mount'
import { resetSession } from '../src/composables/useSession'

vi.mock('../src/api/workItems', () => import('./apiMock'))

beforeEach(() => {
  localStorage.clear()
  resetSession()
  vi.clearAllMocks()
})

describe('sign in', () => {
  it('hides the list until a role and name are entered', async () => {
    const { wrapper } = await mountAt('/work-items')
    expect(wrapper.text()).toContain('Sign in')
    expect(wrapper.text()).not.toContain('目前無待辦項目')
    expect(wrapper.text()).not.toContain('編號')
  })

  it('strips surrounding spaces and leaves the sign-in page for each role', async () => {
    const { wrapper, router } = await mountAt('/sign-in')
    await wrapper.get('input[name="displayName"]').setValue('  Ada  ')
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(createSession).toHaveBeenCalledWith({ displayName: 'Ada', role: 'user' })
    expect(router.currentRoute.value.path).toBe('/work-items')

    localStorage.clear()
    resetSession()
    await router.push('/sign-in')
    await flushPromises()
    await wrapper.get('input[value="admin"]').setChecked(true)
    await wrapper.get('input[name="displayName"]').setValue('  Bo  ')
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(createSession).toHaveBeenCalledWith({ displayName: 'Bo', role: 'admin' })
    expect(router.currentRoute.value.path).toBe('/admin/work-items')
  })

  it('rejects a name that is empty after trim', async () => {
    const { wrapper, router } = await mountAt('/sign-in')
    await wrapper.get('input[name="displayName"]').setValue('   ')
    await wrapper.get('form').trigger('submit')
    await flushPromises()
    expect(createSession).not.toHaveBeenCalled()
    expect(router.currentRoute.value.path).toBe('/sign-in')
    expect(wrapper.text()).toContain('請輸入顯示名稱')
  })
})
