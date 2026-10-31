<template>
  <div class="ape-volo-bg">
    <iframe
      v-if="reloadFlag"
      id="base-load-dom"
      class="w-full border-t dark:border-gray-700"
      :src="url"
      sandbox="allow-scripts allow-forms allow-popups allow-downloads"
      referrerpolicy="no-referrer"
      style="height: calc(100vh - 200px)"
    ></iframe>
  </div>
</template>

<script setup lang="ts">
  import useResponsive from '@/composables/useResponsive'
  import { emitter } from '@/utils/bus.js'
  import {
    ref,
    onMounted,
    onUnmounted,
    nextTick,
    reactive,
    watchEffect
  } from 'vue'
  import { useRouter, useRoute } from 'vue-router'
  import { useAppStore } from '@/pinia'
  import { toHttpUrl } from '@/services/navigation/menuNavigation'
  import { storeToRefs } from 'pinia'
  const appStore = useAppStore()
  const { isDark } = storeToRefs(appStore)

  defineOptions({
    name: 'IFrame'
  })

  useResponsive(true)
  const font = reactive({
    color: 'rgba(0, 0, 0, .15)'
  })

  watchEffect(() => {
    font.color = isDark.value ? 'rgba(255,255,255, .15)' : 'rgba(0, 0, 0, .15)'
  })

  const router = useRouter()
  const route = useRoute()

  const url = toHttpUrl(route.meta.iframeUrl as string) || 'about:blank'
  onMounted(() => {
    emitter.on('reload', reload)
  })

  const reloadFlag = ref(true)
  let reloadTimer: number | null = null
  const reload = async () => {
    if (reloadTimer) {
      window.clearTimeout(reloadTimer)
    }
    reloadTimer = window.setTimeout(async () => {
      if (route.meta.keepAlive) {
        reloadFlag.value = false
        await nextTick()
        reloadFlag.value = true
      } else {
        const title = route.meta.title as string
        router.push({ name: 'Reload', params: { title } })
      }
    }, 400)
  }

  onUnmounted(() => {
    emitter.off('reload', reload)
    if (reloadTimer) {
      window.clearTimeout(reloadTimer)
    }
  })
</script>
