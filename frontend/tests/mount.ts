import { mount, flushPromises } from '@vue/test-utils'
import { createMemoryHistory } from 'vue-router'
import App from '../src/App.vue'
import { resetSession } from '../src/composables/useSession'
import { createAppRouter } from '../src/router'
import type { Role } from '../src/api/types'

export function signAs(name: string, role: Role) {
  localStorage.setItem('work-item-session', JSON.stringify({ displayName: name, role }))
  resetSession()
}

export async function mountAt(path: string) {
  const router = createAppRouter(createMemoryHistory())
  await router.push(path)
  await router.isReady()
  const wrapper = mount(App, { global: { plugins: [router] } })
  await flushPromises()
  return { router, wrapper }
}
