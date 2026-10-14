<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { ApiError, getWorkItem } from '../api/workItems'
import type { WorkItemDetail } from '../api/types'

const route = useRoute()
const item = ref<WorkItemDetail | null>(null)
const missing = ref(false)

function formatWhen(iso: string) {
  const date = new Date(iso)
  return Number.isNaN(date.getTime()) ? iso : date.toLocaleString()
}

onMounted(async () => {
  try {
    item.value = await getWorkItem(Number(route.params.id))
  } catch (err) {
    if (err instanceof ApiError && err.status === 404) missing.value = true
  }
})
</script>

<template>
  <section class="card bg-base-100 shadow-sm">
    <div class="card-body">
      <p v-if="missing" role="alert">找不到此項目</p>
      <dl v-else-if="item" class="grid grid-cols-[8rem_1fr] gap-2">
        <dt>編號</dt>
        <dd>{{ item.id }}</dd>
        <dt>標題</dt>
        <dd>{{ item.title }}</dd>
        <dt>描述</dt>
        <dd>{{ item.description }}</dd>
        <dt>建立時間</dt>
        <dd>{{ formatWhen(item.createdAt) }}</dd>
        <dt>狀態</dt>
        <dd>{{ item.status === 'confirmed' ? '已確認' : '待確認' }}</dd>
        <dt>最後更新時間</dt>
        <dd>{{ formatWhen(item.updatedAt) }}</dd>
      </dl>
      <RouterLink class="btn btn-ghost mt-4 w-fit" :to="{ path: '/work-items', query: { page: route.query.page ?? '1', direction: route.query.direction ?? 'desc' } }">返回列表</RouterLink>
    </div>
  </section>
</template>
