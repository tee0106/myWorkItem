import { computed, ref, watch, type Ref } from 'vue'

export function useWorkItemSelection(pageIds: Ref<number[]>, page: Ref<number>, direction: Ref<string>) {
  const selected = ref<number[]>([])

  watch([page, direction], () => {
    selected.value = []
  })

  const onPage = computed(() => selected.value.filter((id) => pageIds.value.includes(id)))
  const allChecked = computed(() => pageIds.value.length > 0 && pageIds.value.every((id) => selected.value.includes(id)))
  const mixed = computed(() => onPage.value.length > 0 && !allChecked.value)
  const canConfirm = computed(() => onPage.value.length > 0)

  function toggle(id: number) {
    selected.value = selected.value.includes(id)
      ? selected.value.filter((item) => item !== id)
      : [...selected.value, id]
  }

  function toggleAll() {
    selected.value = allChecked.value ? [] : [...pageIds.value]
  }

  function clear() {
    selected.value = []
  }

  return { selected, allChecked, mixed, canConfirm, onPage, toggle, toggleAll, clear }
}
