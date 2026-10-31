import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { getApeToken } from '@/services/session/tokenStorage'
import { readScreenLockRecord, saveScreenLockRecord, clearScreenLockRecord, createScreenLockRecord, verifyScreenLockPassword } from '@/services/session/screenLock'
import type { ScreenLockRecord } from '@/services/session/screenLock'

export const useScreenLockStore = defineStore('screenLock', () => {
  const record = ref<ScreenLockRecord | null>(readScreenLockRecord())
  const pending = ref(false)
  const locked = computed(() => record.value !== null)

  /**
   * 恢复屏幕锁当前输入或状态的初始值。
   */
  function reset(): void {
    record.value = null
    clearScreenLockRecord()
  }

  /**
   * 检查屏幕锁记录是否仍属于当前登录；会话变化时清理旧锁。
   */
  function syncSession(): void {
    if (record.value && record.value.clientSessionId !== getApeToken().sessionId) reset()
  }

  /**
   * 派生屏幕锁校验值并持久化，派生期间登录变化时放弃锁定。
   * @param password 密码策略配置或身份认证输入中的密码，不能记录到日志。
   * @returns 当前会话的锁记录已验证保存且锁定成功时为 true；被阻止或会话变化时为 false。
   */
  async function lock(password: string): Promise<boolean> {
    if (pending.value || locked.value || password.trim().length < 4 || password.length > 64) return false
    const clientSessionId = getApeToken().sessionId
    if (!clientSessionId || !crypto.subtle) return false
    pending.value = true
    try {
      const next = await createScreenLockRecord(clientSessionId, password)
      // 密码派生期间可能退出或切换账号，不能锁住后来的会话。
      if (getApeToken().sessionId !== clientSessionId) return false
      if (!saveScreenLockRecord(next)) return false
      record.value = next
      return true
    } finally {
      pending.value = false
    }
  }

  /**
   * 验证当前屏幕锁密码，并仅清理校验期间仍属于同一会话的锁记录。
   * @param password 密码策略配置或身份认证输入中的密码，不能记录到日志。
   * @returns 密码匹配且当前会话锁已清除时为 true；密码错误、会话变化或忙碌时为 false。
   */
  async function unlock(password: string): Promise<boolean> {
    if (pending.value || !record.value) return false
    syncSession()
    const expected = record.value
    if (!expected) return false
    pending.value = true
    try {
      const valid = await verifyScreenLockPassword(expected, password)
      syncSession()
      if (!valid || record.value !== expected) return false
      reset()
      return true
    } finally {
      pending.value = false
    }
  }

  syncSession()
  return { locked, pending, lock, unlock, reset, syncSession }
})
