<script setup lang="ts">
import { computed, ref, watchEffect } from 'vue'
import { ApiError, confirmWorkItems, revokeWorkItem } from '../api/workItems'
import { useWorkItemList } from '../composables/useWorkItemList'
import { useWorkItemSelection } from '../composables/useWorkItemSelection'
import type { Status } from '../api/types'

const { page, direction, items, total, error, notice, load, setPage, toggleSort } = useWorkItemList()
const pageIds = computed(() => items.value.map((item) => item.id))
const { selected, allChecked, mixed, canConfirm, onPage, toggle, toggleAll, clear } = useWorkItemSelection(pageIds, page, direction)
const headerBox = ref<HTMLInputElement | null>(null)
const revokeId = ref<number | null>(null)

watchEffect(() => {
  if (headerBox.value) headerBox.value.indeterminate = mixed.value
})

function label(status: Status) {
  return status === 'confirmed' ? '已確認' : '待確認'
}

async function confirm() {
  try {
    const result = await confirmWorkItems(onPage.value)
    clear()
    notice.value = `已成功確認 ${result.confirmedCount} 項目`
    error.value = ''
    await load()
  } catch (err) {
    notice.value = ''
    error.value = err instanceof ApiError && err.status === 404 ? '找不到此項目' : '更新失敗，狀態未變更'
    if (err instanceof ApiError && err.status === 404) await load()
  }
}

async function revoke() {
  if (revokeId.value == null) return
  const id = revokeId.value
  try {
    const result = await revokeWorkItem(id)
    revokeId.value = null
    notice.value = `已將 ${result.revokedCount} 項目標記為待確認`
    error.value = ''
    await load()
  } catch (err) {
    revokeId.value = null
    notice.value = ''
    error.value = err instanceof ApiError && err.status === 404 ? '找不到此項目' : '更新失敗，狀態未變更'
    if (err instanceof ApiError && err.status === 404) await load()
  }
}
</script>

<template>
  <section class="flex flex-col gap-3">
    <div class="flex flex-wrap items-center gap-2">
      <button class="btn btn-primary" type="button" :disabled="!canConfirm" @click="confirm">確認</button>
      <button class="btn" type="button" name="sort" @click="toggleSort">{{ direction === 'desc' ? '由新到舊' : '由舊到新' }}</button>
    </div>
    <div v-if="notice" class="alert alert-success" role="status">{{ notice }}</div>
    <div v-if="error" class="alert alert-error" role="alert">{{ error }}</div>
    <table class="table bg-base-100">
      <thead>
        <tr>
          <th>
            <input ref="headerBox" class="checkbox" type="checkbox" aria-label="全選" :checked="allChecked" @change="toggleAll" />
          </th>
          <th>編號</th>
          <th>標題</th>
          <th>狀態</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in items" :key="item.id" :class="{ 'bg-base-200': selected.includes(item.id) }">
          <td>
            <input class="checkbox" type="checkbox" :checked="selected.includes(item.id)" @change="toggle(item.id)" />
          </td>
          <td>{{ item.id }}</td>
          <td>{{ item.title }}</td>
          <td>{{ label(item.status) }}</td>
          <td class="flex gap-2">
            <RouterLink class="btn btn-ghost btn-sm" :to="{ path: `/work-items/${item.id}`, query: { page: String(page), direction } }">查看詳情</RouterLink>
            <button v-if="item.status === 'confirmed'" class="btn btn-ghost btn-sm" type="button" @click="revokeId = item.id">撤銷確認</button>
          </td>
        </tr>
      </tbody>
    </table>
    <p v-if="items.length === 0">目前無待辦項目</p>
    <div class="flex items-center gap-2">
      <button class="btn" type="button" name="prev" :disabled="page <= 1" @click="setPage(page - 1)">上一頁</button>
      <span>第 {{ page }} 頁</span>
      <button class="btn" type="button" name="next" :disabled="page * 10 >= total" @click="setPage(page + 1)">下一頁</button>
    </div>
    <div v-if="revokeId != null" class="modal modal-open">
      <div class="modal-box">
        <p>確定要將此項目標記回『待確認』嗎？</p>
        <div class="modal-action">
          <button class="btn" type="button" @click="revokeId = null">取消</button>
          <button class="btn btn-primary" type="button" name="confirm-revoke" @click="revoke">確定</button>
        </div>
      </div>
    </div>
  </section>
</template>
