<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useSession } from './composables/useSession'

const router = useRouter()
const { session, signOut } = useSession()

function logout() {
  signOut()
  void router.push('/sign-in')
}
</script>

<template>
  <div class="min-h-screen bg-base-200">
    <header class="navbar bg-base-100 px-4 shadow-sm">
      <span class="font-semibold">Work Item List</span>
      <div v-if="session" class="ml-auto flex items-center gap-2">
        <span>{{ session.displayName }}</span>
        <button class="btn btn-ghost btn-sm" type="button" @click="logout">登出</button>
      </div>
    </header>
    <main class="mx-auto max-w-5xl p-4">
      <RouterView />
    </main>
  </div>
</template>
