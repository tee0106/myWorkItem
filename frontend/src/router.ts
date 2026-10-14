import { createRouter, createWebHistory, type RouterHistory } from 'vue-router'
import { currentSession } from './composables/useSession'
import AdminListPage from './pages/AdminListPage.vue'
import AdminWorkItemFormPage from './pages/AdminWorkItemFormPage.vue'
import SignInPage from './pages/SignInPage.vue'
import WorkItemDetailPage from './pages/WorkItemDetailPage.vue'
import WorkItemListPage from './pages/WorkItemListPage.vue'

export function createAppRouter(history: RouterHistory = createWebHistory()) {
  const router = createRouter({
    history,
    routes: [
      {
        path: '/',
        redirect: () => {
          const session = currentSession()
          if (!session) return '/sign-in'
          return session.role === 'admin' ? '/admin/work-items' : '/work-items'
        },
      },
      { path: '/sign-in', component: SignInPage },
      { path: '/work-items', component: WorkItemListPage },
      { path: '/work-items/:id', component: WorkItemDetailPage },
      { path: '/admin/work-items', component: AdminListPage },
      { path: '/admin/work-items/new', component: AdminWorkItemFormPage },
      { path: '/admin/work-items/:id/edit', component: AdminWorkItemFormPage },
    ],
  })

  router.beforeEach((to) => {
    const session = currentSession()
    if (to.path === '/sign-in') {
      if (!session) return true
      return session.role === 'admin' ? '/admin/work-items' : '/work-items'
    }
    if (!session) return '/sign-in'
    if (to.path.startsWith('/admin') && session.role !== 'admin') return '/work-items'
    return true
  })

  return router
}
