<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiError, createWorkItem, getAdminWorkItem, updateWorkItem } from '../api/workItems'

const route = useRoute()
const router = useRouter()
const title = ref('')
const description = ref('')
const fieldError = ref('')
const error = ref('')
const isEdit = computed(() => route.path.endsWith('/edit'))

onMounted(async () => {
  if (!isEdit.value) return
  try {
    const item = await getAdminWorkItem(Number(route.params.id))
    title.value = item.title
    description.value = item.description
  } catch (err) {
    if (err instanceof ApiError && err.status === 404) error.value = '找不到此項目'
  }
})

async function save() {
  const trimmed = title.value.trim()
  if (!trimmed) {
    fieldError.value = '標題不得為空'
    return
  }
  fieldError.value = ''
  const body = { title: trimmed, description: description.value.trim() }
  try {
    if (isEdit.value) {
      await updateWorkItem(Number(route.params.id), body)
      await router.push({ path: '/admin/work-items', query: { notice: 'updated' } })
    } else {
      await createWorkItem(body)
      await router.push({ path: '/admin/work-items', query: { notice: 'created' } })
    }
  } catch (err) {
    if (err instanceof ApiError && err.status === 400 && err.field === 'title') {
      fieldError.value = '標題不得為空'
      return
    }
    if (err instanceof ApiError && err.status === 404) {
      await router.push({ path: '/admin/work-items', query: { error: 'missing' } })
      return
    }
    error.value = '儲存失敗'
  }
}
</script>

<template>
  <form class="card bg-base-100 shadow-sm" @submit.prevent="save">
    <div class="card-body gap-3">
      <h1 class="card-title">{{ isEdit ? '編輯' : '新增' }}</h1>
      <p v-if="error" class="alert alert-error" role="alert">{{ error }}</p>
      <label class="form-control">
        <span class="label-text">標題</span>
        <input v-model="title" class="input input-bordered" name="title" />
      </label>
      <p v-if="fieldError" class="text-error">{{ fieldError }}</p>
      <label class="form-control">
        <span class="label-text">描述</span>
        <textarea v-model="description" class="textarea textarea-bordered" name="description"></textarea>
      </label>
      <button class="btn btn-primary w-fit" type="submit">{{ isEdit ? '儲存' : '新增' }}</button>
    </div>
  </form>
</template>
