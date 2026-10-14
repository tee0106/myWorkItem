<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiError, deleteWorkItem, listAdminWorkItems } from '../api/workItems'
import type { AdminWorkItem } from '../api/types'

const route = useRoute()
const router = useRouter()
const items = ref<AdminWorkItem[]>([])
const total = ref(0)
const notice = ref('')
const error = ref('')
const deleteId = ref<number | null>(null)
const page = computed(() => Math.max(1, Number(route.query.page) || 1))
const direction = computed<'asc' | 'desc'>(() => (route.query.direction === 'asc' ? 'asc' : 'desc'))

function formatWhen(iso: string) {
  const date = new Date(iso)
  return Number.isNaN(date.getTime()) ? iso : date.toLocaleString()
}

async function load() {
  const data = await listAdminWorkItems(page.value, direction.value)
  items.value = data.items
  total.value = data.total
}

watch([page, direction], () => void load(), { immediate: true })
watch(() => route.query.notice, (value) => {
  if (value === 'created') notice.value = '新增成功'
  if (value === 'updated') notice.value = '更新成功'
}, { immediate: true })
watch(() => route.query.error, (value) => {
  if (value === 'missing') error.value = '找不到此項目'
}, { immediate: true })

function setPage(next: number) {
  void router.replace({ query: { ...route.query, page: String(next), direction: direction.value } })
}

function toggleSort() {
  void router.replace({ query: { page: '1', direction: direction.value === 'desc' ? 'asc' : 'desc' } })
}

async function confirmDelete() {
  if (deleteId.value == null) return
  try {
    await deleteWorkItem(deleteId.value)
    deleteId.value = null
    notice.value = '刪除成功'
    error.value = ''
    await load()
  } catch (err) {
    deleteId.value = null
    notice.value = ''
    error.value = err instanceof ApiError && err.status === 404 ? '找不到此項目' : '刪除失敗，項目仍保留'
    if (err instanceof ApiError && err.status === 404) await load()
  }
}
</script>

<template>
  <section class="flex flex-col gap-3">
    <div class="flex gap-2">
      <RouterLink class="btn btn-primary" to="/admin/work-items/new">新增</RouterLink>
      <button class="btn" type="button" name="sort" @click="toggleSort">{{ direction === 'desc' ? '由新到舊' : '由舊到新' }}</button>
    </div>
    <div v-if="notice" class="alert alert-success" role="status">{{ notice }}</div>
    <div v-if="error" class="alert alert-error" role="alert">{{ error }}</div>
    <table class="table bg-base-100">
      <thead>
        <tr>
          <th>編號</th>
          <th>標題</th>
          <th>建立時間</th>
          <th>最後更新時間</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in items" :key="item.id">
          <td>{{ item.id }}</td>
          <td>{{ item.title }}</td>
          <td>{{ formatWhen(item.createdAt) }}</td>
          <td>{{ formatWhen(item.updatedAt) }}</td>
          <td class="flex gap-2">
            <RouterLink class="btn btn-ghost btn-sm" :to="`/admin/work-items/${item.id}/edit`">編輯</RouterLink>
            <button class="btn btn-ghost btn-sm" type="button" name="delete" @click="deleteId = item.id">刪除</button>
          </td>
        </tr>
      </tbody>
    </table>
    <p v-if="items.length === 0">目前無待辦項目</p>
    <div class="flex items-center gap-2">
      <button class="btn" type="button" :disabled="page <= 1" @click="setPage(page - 1)">上一頁</button>
      <span>第 {{ page }} 頁 / {{ total }}</span>
      <button class="btn" type="button" :disabled="page * 10 >= total" @click="setPage(page + 1)">下一頁</button>
    </div>
    <div v-if="deleteId != null" class="modal modal-open">
      <div class="modal-box">
        <p>確定要刪除此項目嗎？</p>
        <div class="modal-action">
          <button class="btn" type="button" name="cancel-delete" @click="deleteId = null">取消</button>
          <button class="btn btn-error" type="button" name="confirm-delete" @click="confirmDelete">確定</button>
        </div>
      </div>
    </div>
  </section>
</template>
