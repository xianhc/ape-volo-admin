/**
 * 全局快捷键注册组合式函数。
 */
import { useAppStore } from '@/pinia'
import { emitter } from '@/utils/bus'
import { useEventListener } from '@vueuse/core'

/**
 * 注册全局快捷键，按界面偏好触发操作，并在组件卸载时移除监听。
 */
export function useGlobalShortcuts(): void {
  const app = useAppStore()

  useEventListener(document, 'keydown', (event) => {
    if (!app.config.enableShortcuts || event.isComposing) return
    const key = event.key.toLowerCase()
    const command = event.ctrlKey || event.metaKey

    if (
      command &&
      event.shiftKey &&
      key === 'p' &&
      app.config.shortcutPreferences
    ) {
      event.preventDefault()
      emitter.emit('openPreferences')
    } else if (
      command &&
      event.shiftKey &&
      key === 'l' &&
      app.config.shortcutLockScreen
    ) {
      event.preventDefault()
      emitter.emit('openScreenLock')
    } else if (
      command &&
      event.shiftKey &&
      key === 'q' &&
      app.config.shortcutLogout
    ) {
      event.preventDefault()
      emitter.emit('requestSignOut')
    } else if (
      event.key === 'Escape' &&
      app.config.shortcutEscape &&
      app.workspaceMaximized
    ) {
      event.preventDefault()
      app.toggleWorkspaceMaximized()
    }
  })
}
