<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError, createSession } from '../api/workItems'
import { useSession } from '../composables/useSession'
import type { Role } from '../api/types'

const router = useRouter()
const { signIn } = useSession()
const role = ref<Role>('user')
const displayName = ref('')
const error = ref('')

async function submit() {
  const name = displayName.value.trim()
  if (!name) {
    error.value = '請輸入顯示名稱'
    return
  }
  try {
    const session = await createSession({ displayName: name, role: role.value })
    signIn(session)
    error.value = ''
    await router.push(session.role === 'admin' ? '/admin/work-items' : '/work-items')
  } catch (err) {
    error.value = err instanceof ApiError && err.error === 'invalid_role' ? '角色不正確' : '登入失敗'
  }
}
</script>

<template>
  <form class="card mx-auto max-w-md bg-base-100 shadow-sm" @submit.prevent="submit">
    <div class="card-body gap-3">
      <h1 class="card-title">Sign in</h1>
      <label class="flex items-center gap-2">
        <input v-model="role" class="radio" type="radio" name="role" value="user" />
        Front-end user
      </label>
      <label class="flex items-center gap-2">
        <input v-model="role" class="radio" type="radio" name="role" value="admin" />
        Administrator
      </label>
      <label class="form-control">
        <span class="label-text">Display name</span>
        <input v-model="displayName" class="input input-bordered" name="displayName" />
      </label>
      <p v-if="error" class="text-error" role="alert">{{ error }}</p>
      <button class="btn btn-primary" type="submit">Sign in</button>
    </div>
  </form>
</template>
