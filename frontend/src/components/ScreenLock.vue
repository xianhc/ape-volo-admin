<template>
  <el-dialog :model-value="lock.locked" :title="'屏幕已锁定'" fullscreen append-to-body :show-close="false" :close-on-click-modal="false" :close-on-press-escape="false" :z-index="20000" class="screen-lock-dialog" @opened="focusPassword">
    <main class="screen-lock" @keydown.stop>
      <div class="screen-lock-clock" aria-hidden="true">{{ clock }}</div>
      <section class="screen-lock-card" :aria-label="'屏幕已锁定'">
        <div class="screen-lock-identity">
          <el-avatar :size="80" :src="avatar" :alt="user.userInfo.nickName || user.userInfo.userName" />
          <span class="screen-lock-symbol" aria-hidden="true"><AppIcon icon="lucide:lock-keyhole" :size="16" /></span>
        </div>
        <h1>{{ '屏幕已锁定' }}</h1>
        <p class="screen-lock-user">{{ user.userInfo.nickName || user.userInfo.userName }}</p>
        <p class="screen-lock-description">{{ '请输入设置的解锁密码，继续使用。' }}</p>
        <el-form @submit.prevent="unlock">
          <el-form-item>
            <el-input ref="passwordInput" v-model="password" type="password" show-password autocomplete="off" :maxlength="64" :disabled="lock.pending || signingOut" :aria-label="'解锁密码'" />
          </el-form-item>
          <p v-if="error" class="screen-lock-error" role="alert">{{ error }}</p>
          <el-button type="primary" native-type="submit" class="screen-lock-unlock" :loading="lock.pending" :disabled="signingOut || !password">{{ '解锁' }}</el-button>
        </el-form>
        <el-button text class="screen-lock-exit" :loading="signingOut" :disabled="lock.pending" @click="signOut">
          <AppIcon icon="lucide:log-out" /><span>{{ '退出登录' }}</span>
        </el-button>
      </section>
    </main>
  </el-dialog>
</template>
<script setup lang="ts">
  /**
   * 锁屏遮罩组件。
   */
  import { computed, ref, watch, nextTick } from 'vue'
  import { useNow } from '@vueuse/core'
  import type { InputInstance } from 'element-plus'
  import AppIcon from './AppIcon.vue'
  import { useScreenLockStore } from '@/pinia/modules/screenLock'
  import { useUserStore } from '@/pinia/modules/user'
  import { useSignOut } from '@/composables/useSignOut'
  import { getStaticBaseUrl } from '@/utils/index'
  import noAvatar from '@/assets/noAvatar.png'
    const lock = useScreenLockStore()
  const user = useUserStore()
  const avatar = computed(() => user.userInfo.headerImg ? getStaticBaseUrl() + user.userInfo.headerImg : noAvatar)
  const { signingOut, signOut } = useSignOut()
  const now = useNow({ interval: 1000 })
  const clock = computed(() => new Intl.DateTimeFormat('zh-CN', { hour: '2-digit', minute: '2-digit', hourCycle: 'h23' }).format(now.value))
  const password = ref('')
  const error = ref('')
  const passwordInput = ref<InputInstance>()
  /**
   * 将焦点移到当前输入、密码字段或抽屉内容。
   */
  function focusPassword(): void {
    passwordInput.value?.focus()
  }
  watch(() => lock.locked, () => {
    password.value = ''
    error.value = ''
  })
  /**
   * 设置或验证当前登录会话的本地屏幕锁。
   */
  async function unlock(): Promise<void> {
    if (lock.pending || signingOut.value || !password.value) return
    error.value = ''
    try {
      if (!await lock.unlock(password.value)) error.value = '解锁密码不正确'
    } catch {
      error.value = '无法锁定当前会话，请确认会话有效，并使用 HTTPS 或本机地址后重试。'
    } finally {
      password.value = ''
      if (lock.locked) {
        await nextTick()
        focusPassword()
      }
    }
  }
</script>
<style scoped>
  .screen-lock { display: flex; flex-direction: column; align-items: center; justify-content: center; min-height: calc(100dvh - 120px); gap: 32px; color: var(--app-text-primary); }
  .screen-lock-clock { font-size: clamp(56px, 10vw, 96px); font-weight: 500; font-variant-numeric: tabular-nums; line-height: 1; letter-spacing: -3px; }
  .screen-lock-card { box-sizing: border-box; width: min(380px, 100%); padding: 28px; border: 1px solid var(--app-border); border-radius: 16px; background: var(--app-surface); text-align: center; }
  .screen-lock-identity { position: relative; display: inline-flex; }
  .screen-lock-symbol { position: absolute; right: -2px; bottom: -2px; display: inline-flex; align-items: center; justify-content: center; width: 26px; height: 26px; border: 3px solid var(--app-surface); border-radius: 50%; color: var(--el-color-primary); background: var(--el-color-primary-light-9); }
  h1 { margin: 20px 0 8px; font-size: 22px; }
  .screen-lock-user { margin: 0; font-weight: 500; }
  .screen-lock-description { color: var(--app-text-secondary); line-height: 1.7; }
  .screen-lock-unlock { width: 100%; }
  .screen-lock-error { color: var(--el-color-danger); text-align: left; }
  .screen-lock-exit { margin-top: 16px; }
  .screen-lock-exit span { margin-left: 8px; }
  :global(.el-dialog.screen-lock-dialog) { background: var(--app-canvas); }
  :global(.screen-lock-dialog .el-dialog__header) { display: none; }
</style>
