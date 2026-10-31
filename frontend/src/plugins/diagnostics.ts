import type { App } from 'vue'
import router from '@/router'
import { getApeToken } from '@/services/session/tokenStorage'
import { getBaseUrl } from '@/utils'
import { appLogger, configureErrorReporter } from '@/services/logging/appLogger'
import { createErrorReporter } from '@/services/logging/errorReporter'

/** 接入 Vue 和浏览器错误；仅用有效访问令牌向本项目后端发送有限诊断字段。 */
export function setupDiagnostics(app: App): void {
  configureErrorReporter(createErrorReporter({
    context: () => ({ routeName: String(router.currentRoute.value.name ?? ''), build: import.meta.env.VITE_BUILD_TAG }),
    send: async report => {
      const token = getApeToken()
      if (!token.accessToken || token.expires <= Date.now()) return
      await fetch(`${getBaseUrl().replace(/\/$/, '')}/client-errors`, {
        method: 'POST', credentials: 'omit', keepalive: true,
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token.accessToken}` },
        body: JSON.stringify(report)
      })
    }
  }))
  const previous = app.config.errorHandler
  app.config.errorHandler = (error, instance, info) => {
    appLogger.error('vue.unhandled', error)
    previous?.(error, instance, info)
  }
  window.addEventListener('error', event => appLogger.error('browser.unhandled', event.error))
  window.addEventListener('unhandledrejection', event => appLogger.error('promise.unhandled', event.reason))
}
