<template>
  <div id="app" class="ape-volo-bg">
    <el-config-provider :locale="zhCn" :size="app.config.componentSize">
      <div
        :inert="screenLock.locked || undefined"
        :aria-hidden="screenLock.locked || undefined"
      >
        <router-view />
      </div>
      <ScreenLock />
    </el-config-provider>
  </div>
</template>

<script setup lang="ts">
  import zhCn from 'element-plus/es/locale/lang/zh-cn'
  import { useRoute } from 'vue-router'
  import { useEventListener, useTitle } from '@vueuse/core'
  import { watch } from 'vue'
  import { useAppStore } from '@/pinia'
  import { useScreenLockStore } from '@/pinia/modules/screenLock'
  import { useUserStore } from '@/pinia/modules/user'
  import ScreenLock from '@/components/ScreenLock.vue'
  import { usePreferencesPersistence } from '@/composables/usePreferencesPersistence'
  import { useGlobalShortcuts } from '@/composables/useGlobalShortcuts'
  import { menuTitle } from '@/utils/menuTitle'
  import setting from '@/setting'

  defineOptions({
    name: 'App'
  })

  const app = useAppStore()
  const route = useRoute()
  const screenLock = useScreenLockStore()
  const user = useUserStore()

  // 界面偏好：本地 localStorage + 后端 preferencesConfig 双向同步
  usePreferencesPersistence()
  // 全局快捷键（可在偏好设置中关闭）
  useGlobalShortcuts()

  // 锁屏状态跟随登录用户，多标签页之间同步
  watch(() => user.userInfo.id, screenLock.syncSession, { immediate: true })
  useEventListener(window, 'storage', screenLock.syncSession)

  // 动态页面标题
  const title = useTitle()
  watch(
    () => [route.fullPath, app.config.dynamicTitle] as const,
    () => {
      if (!app.config.dynamicTitle) {
        title.value = setting.appName
        return
      }
      const pageTitle = menuTitle(route.meta as Record<string, unknown>)
      title.value = pageTitle ? `${pageTitle} - ${setting.appName}` : setting.appName
    },
    { immediate: true }
  )
</script>
<style lang="scss">
  #app {
    overflow: hidden;
    font-weight: 400 !important;
  }
</style>
