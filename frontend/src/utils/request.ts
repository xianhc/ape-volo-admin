import { createHttpClient } from '@/services/http/client'
import { httpFeedback } from '@/services/http/httpFeedback'
import { refreshToken } from '@/api/authClient'
import { createTokenRefresher } from '@/services/session/tokenRefresh'
import { clearToken, getApeToken, setApeToken } from '@/services/session/tokenStorage'
import setting from '@/setting'
import { reportServerError } from '@/services/logging/appLogger'

const refresh = createTokenRefresher({
  read: getApeToken,
  save: setApeToken,
  request: async (token) => (await refreshToken({ token })).data,
  withLock: async (work) => {
    if (navigator.locks) await navigator.locks.request('ape-volo-token-refresh', work)
    else await work()
  }
})

export default createHttpClient({
  config: {
    baseURL: import.meta.env.VITE_BASE_API,
    timeout: setting.timeout,
    headers: import.meta.env.VITE_BUILD_TAG ? { 'X-Client-Build': import.meta.env.VITE_BUILD_TAG } : {}
  },
  session: {
    read: getApeToken,
    refresh,
    invalidate: (sessionId) => { if (getApeToken().sessionId === sessionId) clearToken() }
  },
  feedback: httpFeedback,
  onServerError: reportServerError
})
