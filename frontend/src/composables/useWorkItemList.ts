import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { listWorkItems } from '../api/workItems'
import type { WorkItemSummary } from '../api/types'

export function useWorkItemList() {
  const route = useRoute()
  const router = useRouter()
  const page = computed(() => Math.max(1, Number(route.query.page) || 1))
  const direction = computed<'asc' | 'desc'>(() => (route.query.direction === 'asc' ? 'asc' : 'desc'))
  const items = ref<WorkItemSummary[]>([])
  const total = ref(0)
  const error = ref('')
  const notice = ref('')

  async function load() {
    const data = await listWorkItems(page.value, direction.value)
    items.value = data.items
    total.value = data.total
  }

  watch([page, direction], () => void load(), { immediate: true })

  function setPage(next: number) {
    void router.replace({ query: { page: String(next), direction: direction.value } })
  }

  function toggleSort() {
    void router.replace({
      query: { page: '1', direction: direction.value === 'desc' ? 'asc' : 'desc' },
    })
  }

  return { page, direction, items, total, error, notice, load, setPage, toggleSort }
}
