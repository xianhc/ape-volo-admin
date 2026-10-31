/**
 * 界面偏好持久化组合式函数。
 *
 * 本项目后端契约：
 * - `/user/update-preferences-config` 只接收偏好 JSON 字符串，没有并发版本戳；
 * - 登录会话标识使用稳定的 sessionId，刷新访问令牌不会改变会话；
 * - 错误提示由 request.ts 统一处理，这里只记录保存状态。
 */
import { editPreferencesConfig } from '@/api/permission/user'
import { useAppStore, useUserStore } from '@/pinia'
import { appLogger } from '@/services/logging/appLogger'
import {
  PREFERENCES_STORAGE_KEY,
  serializePreferences
} from '@/services/appearance/preferences'
import type { PreferencesSaveStatus } from '@/services/appearance/preferences.types'
import { getApeToken } from '@/services/session/tokenStorage'
import { writeStoredJson } from '@/services/storage/appStorage'
import { ElMessage } from 'element-plus'
import { ref, watch } from 'vue'

const saveStatus = ref<PreferencesSaveStatus>('idle')
const lastSavedAt = ref<number | null>(null)
let started = false
let debounceTimer: ReturnType<typeof setTimeout> | undefined
let pendingSave:
  | {
    /** 用户标识。 */
    userId: string
    /** 登录会话标识，用于隔离不同登录。 */
    clientSessionId: string
    /** 按传输契约序列化后的内容。 */
    serialized: string
    /** 初始同步不弹提示，避免登录后无操作也提示。 */
    silent: boolean
  }
  | undefined
let saveLoop: Promise<void> | undefined
/** 上一条“设置已保存”提示，重复保存时先关掉它，避免叠成一列。 */
let lastSaveMessage: ReturnType<typeof ElMessage> | null = null

/**
 * 取得当前用户标识与登录会话标识。
 * @param userId 用户标识。
 * @returns 当前用户标识与登录会话标识。
 */
const currentIdentity = (userId: unknown) => ({
  userId: String(userId ?? ''),
  clientSessionId: getApeToken().sessionId ?? ''
})

/**
 * 判断当前登录是否仍与操作开始时捕获的身份一致。
 * @param userId 用户标识。
 * @param clientSessionId 登录会话标识。
 * @returns 身份一致时返回 true。
 */
const identityMatches = (userId: string, clientSessionId: string): boolean => {
  const current = currentIdentity(useUserStore().userInfo.id)
  return current.userId === userId && current.clientSessionId === clientSessionId
}

/**
 * 串行保存当前偏好并协调尚未结束的保存任务。
 */
async function flushPendingSave(): Promise<void> {
  if (saveLoop) return saveLoop

  saveLoop = (async () => {
    while (pendingSave) {
      const task = pendingSave
      pendingSave = undefined
      if (!identityMatches(task.userId, task.clientSessionId)) continue

      saveStatus.value = 'saving'
      try {
        await editPreferencesConfig(task.serialized)
        if (identityMatches(task.userId, task.clientSessionId)) {
          saveStatus.value = 'saved'
          lastSavedAt.value = Date.now()
          // 用户主动改配置时给一个自动消失的轻提示；初始同步静默。
          if (!task.silent) {
            lastSaveMessage?.close()
            lastSaveMessage = ElMessage({
              type: 'success',
              message: '设置已保存',
              duration: 2500
            })
          }
        }
      } catch (error) {
        if (identityMatches(task.userId, task.clientSessionId)) {
          saveStatus.value = 'error'
          appLogger.error('preferences.save', error)
        }
      }
    }
  })().finally(() => {
    saveLoop = undefined
    if (pendingSave) void flushPendingSave()
  })

  return saveLoop
}

/**
 * 防抖后排队保存当前偏好。
 * @param serialized 按传输契约序列化后的内容。
 * @param silent 是否静默保存（不弹提示），用于登录后的初始同步。
 */
function queueRemoteSave(serialized: string, silent = false): void {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    const identity = currentIdentity(useUserStore().userInfo.id)
    if (!identity.userId || !identity.clientSessionId) return
    pendingSave = { ...identity, serialized, silent }
    void flushPendingSave()
  }, 500)
}

/**
 * 启动一次共享的界面偏好监听：本地变更写 localStorage，并防抖同步到后端。
 * @returns 共享保存状态、最近保存时间及重试入口。
 */
export function usePreferencesPersistence() {
  const appStore = useAppStore()
  const userStore = useUserStore()

  if (!started) {
    started = true
    watch(
      appStore.config,
      (config) => {
        writeStoredJson(localStorage, PREFERENCES_STORAGE_KEY, config)
        queueRemoteSave(serializePreferences(config))
      },
      { deep: true }
    )
    watch(
      () => userStore.userInfo.id,
      (userId) => {
        if (debounceTimer) clearTimeout(debounceTimer)
        debounceTimer = undefined
        pendingSave = undefined
        saveStatus.value = 'idle'
        // 登录后的初始同步不打扰用户。
        if (userId) queueRemoteSave(serializePreferences(appStore.config), true)
      }
    )
  }

  /**
   * 按当前身份重新提交界面偏好。
   */
  const retry = (): void => {
    const identity = currentIdentity(userStore.userInfo.id)
    if (!identity.userId || !identity.clientSessionId) return
    pendingSave = {
      ...identity,
      serialized: serializePreferences(appStore.config),
      silent: false
    }
    void flushPendingSave()
  }

  return { saveStatus, lastSavedAt, retry }
}
