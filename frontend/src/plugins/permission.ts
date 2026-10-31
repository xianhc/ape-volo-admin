import router from '@/router'
import { useRouterStore } from '@/pinia/modules/router'
import { initializeSession, resetApplicationSession } from '@/services/session/applicationSession'
import { SessionChangedError } from '@/services/session/tokenRefresh'
import { httpFeedback } from '@/services/http/httpFeedback'
import { getApeToken, clearToken } from '@/services/session/tokenStorage'
import setting from '@/setting'
import Nprogress from 'nprogress'
import 'nprogress/nprogress.css'

export function setupPermission(): void {
  let loadingRemoved = false
  const removeLoading = (): void => {
    if (loadingRemoved) return
    loadingRemoved = true
    document.getElementById('loading-box')?.remove()
  }
  Nprogress.configure({ showSpinner: false, ease: 'ease', speed: 500 })
  router.beforeEach(async (to) => {
    Nprogress.start()
    try {
      if (to.meta.client) return true
      const token = getApeToken()
      const hasSession = Boolean(token.sessionId && token.refreshToken && Date.now() < token.refreshTokenExpires)
      if (!hasSession) {
        clearToken()
        resetApplicationSession()
        return to.name === 'Login' ? true : { name: 'Login', query: { redirect: to.fullPath } }
      }
      const revision = useRouterStore().asyncRouterFlag
      await initializeSession()
      if (to.name === 'Login') return router.hasRoute(setting.defaultRouter) ? { name: setting.defaultRouter } : { path: '/403' }
      if (revision !== useRouterStore().asyncRouterFlag) return { path: to.fullPath, replace: true }
      return true
    } catch (error) {
      if (!(error instanceof SessionChangedError)) httpFeedback.error(error)
      Nprogress.done()
      removeLoading()
      return false
    }
  })
  router.afterEach((to) => {
    useRouterStore().syncNavigation(typeof to.name === 'string' ? to.name : undefined)
    removeLoading()
    Nprogress.done()
  })
  router.onError((error) => {
    httpFeedback.error(error)
    removeLoading()
    Nprogress.done()
  })
}
