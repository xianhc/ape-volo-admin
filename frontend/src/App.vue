<!--
******************************************************************************
 * 项目名称：APE-VOLO-ADMIN
 * 开源地址：https://github.com/xianhc/ape-volo-admin
 * 系统官网：https://www.apevolo.com
 * 联系邮箱：apevolo@gmail.com
 *
 * Copyright © 2022 xianhc（冼浩春）。保留所有权利。
 * SPDX-License-Identifier: Apache-2.0
 *
 * 本项目采用 Apache License 2.0，完整许可条款详见根目录 LICENSE 文件。
 *
 * 版权与署名保留要求（Apache License 2.0 第 4 条）：
 * 再分发本项目或其衍生作品时，须向接收者提供许可证副本。
 * 以源码形式分发衍生作品时，须保留与该作品相关的原始版权及署名通知，
 * 不得删除原始通知或以他人署名替换；修改过的文件须显著注明已作修改。
 * 上述说明为许可证要求的提示，完整义务以 LICENSE 条款为准。
 *
 * 作者保留依法维护自身合法权益的权利。授权与合作事宜请通过上述邮箱联系。
 ******************************************************************************
-->

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
  import { resetApplicationSession } from '@/services/session/applicationSession'
  import { isSessionIdentityChange } from '@/services/session/tokenStorage'
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
  useEventListener(window, 'storage', (event) => {
    if (event.storageArea === localStorage && isSessionIdentityChange(event)) {
      resetApplicationSession()
      // 重新挂载页面，丢弃旧账号的表单、列表及 KeepAlive 实例。
      window.location.reload()
      return
    }
    screenLock.syncSession()
  })

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
