<template>
  <el-tooltip v-if="showTrigger" :content="'锁定屏幕'" placement="bottom">
    <button
      type="button"
      class="header-tool-button"
      :aria-label="'锁定屏幕'"
      :disabled="lock.pending"
      aria-haspopup="dialog"
      @click="dialogVisible = true"
    >
      <el-icon><AppIcon icon="lucide:lock-keyhole" /></el-icon>
    </button>
  </el-tooltip>
  <el-dialog
    v-model="dialogVisible"
    :title="'锁定屏幕'"
    width="min(480px, calc(100vw - 32px))"
    class="screen-lock-setup-dialog"
    append-to-body
    align-center
    :close-on-click-modal="false"
    :close-on-press-escape="!lock.pending"
    :show-close="!lock.pending"
    :close-icon="createIconifyIcon('lucide:x')"
    @opened="passwordInput?.focus()"
    @closed="clearForm"
  >
    <div class="lock-identity">
      <el-avatar :size="80" :src="avatar" />
      <h2>{{ user.userInfo.nickName || user.userInfo.userName }}</h2>
    </div>
    <el-form label-position="top" @submit.prevent="submit">
      <el-form-item>
        <el-input
          ref="passwordInput"
          v-model="password"
          size="large"
          type="password"
          show-password
          autocomplete="new-password"
          :maxlength="64"
          :disabled="lock.pending"
          :placeholder="'请输入锁屏密码'"
          :aria-label="'解锁密码'"
        />
      </el-form-item>
      <p v-if="error" class="lock-error" role="alert">{{ error }}</p>
      <div class="lock-actions">
        <el-button
          type="primary"
          size="large"
          native-type="submit"
          :loading="lock.pending"
        >
          {{ '锁定' }}
        </el-button
        >
      </div>
    </el-form>
  </el-dialog>
</template>
<script setup lang="ts">
  /**
   * 锁屏入口按钮组件。
   */
  import { computed, ref, onMounted, onBeforeUnmount } from 'vue'
  import AppIcon from '@/components/AppIcon.vue'
  import { useScreenLockStore } from '@/pinia/modules/screenLock'
  import { useUserStore } from '@/pinia/modules/user'
  import { createIconifyIcon } from '@/components/icons'
  import { getStaticBaseUrl } from '@/utils/index'
  import noAvatar from '@/assets/noAvatar.png'
  import type { InputInstance } from 'element-plus'
  import { emitter } from '@/utils/bus'
  withDefaults(defineProps<{
    /** 是否显示打开或展开入口。 */
    showTrigger?: boolean
  }>(), { showTrigger: true })
  defineExpose({ open: openDialog })
  const lock = useScreenLockStore()
  const user = useUserStore()
  const avatar = computed(() =>
    user.userInfo.headerImg
      ? getStaticBaseUrl() + user.userInfo.headerImg
      : noAvatar
  )
  const passwordInput = ref<InputInstance>()
  const dialogVisible = ref(false)
  const password = ref('')
  const error = ref('')
  /**
   * 打开布局菜单的目标视图或编辑表单。
   */
  function openDialog(): void {
    if (!lock.pending) dialogVisible.value = true
  }

  onMounted(() => emitter.on('openScreenLock', openDialog))
  onBeforeUnmount(() => emitter.off('openScreenLock', openDialog))

  /**
   * 恢复布局菜单当前输入或状态的初始值。
   */
  function clearForm(): void {
    password.value = ''
    error.value = ''
  }
  /**
   * 校验并提交布局菜单当前编辑数据。
   */
  async function submit(): Promise<void> {
    if (lock.pending) return
    error.value = ''
    if (password.value.trim().length < 4) {
      error.value = '请输入 4 至 64 位解锁密码'
      return
    }
    try {
      if (!(await lock.lock(password.value))) {
        error.value = '无法锁定当前会话，请确认会话有效，并使用 HTTPS 或本机地址后重试。'
        return
      }
      dialogVisible.value = false
      clearForm()
    } catch {
      error.value = '无法锁定当前会话，请确认会话有效，并使用 HTTPS 或本机地址后重试。'
    }
  }
</script>
<style scoped>
  .lock-identity {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 8px 0 24px;
  }
  .lock-identity h2 {
    margin: 18px 0 0;
    color: var(--app-text-primary);
    font-size: 20px;
    font-weight: 600;
  }
  .lock-actions {
    margin-top: 20px;
  }
  .lock-actions .el-button {
    width: 100%;
  }
  .lock-error {
    color: var(--el-color-danger);
  }
  :global(.el-dialog.screen-lock-setup-dialog) {
    padding: 0;
    border: 1px solid var(--app-border);
    border-radius: 16px;
    background: var(--app-surface);
  }
  :global(.screen-lock-setup-dialog .el-dialog__header) {
    margin: 0;
    padding: 20px 24px;
    border-bottom: 1px solid var(--app-border);
  }
  :global(.screen-lock-setup-dialog .el-dialog__title) {
    font-size: 20px;
    font-weight: 600;
  }
  :global(.screen-lock-setup-dialog .el-dialog__body) {
    padding: 20px 36px 36px;
  }
  @media (max-width: 480px) {
    :global(.screen-lock-setup-dialog .el-dialog__body) {
      padding-inline: 24px;
    }
  }
</style>
