import axios, { AxiosError } from 'axios'
import type { AxiosRequestConfig, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import type { ApeToken } from '@/services/session/tokenStorage'
import { SessionChangedError, SessionExpiredError } from '../session/tokenRefresh'
import type { HttpFeedback } from './httpFeedback'

declare module 'axios' {
  interface AxiosRequestConfig {
    /** 列表等局部错误由所属界面展示。 */
    errorFeedback?: boolean
    authSessionId?: string
    tokenRefreshRetried?: boolean
  }
}

interface SessionAccess {
  read: () => ApeToken
  refresh: (failedAccessToken?: string) => Promise<void>
  invalidate: (sessionId: string) => void
}

interface ClientOptions {
  config: AxiosRequestConfig
  feedback: HttpFeedback
  session?: SessionAccess
  now?: () => number
  /** 与界面反馈独立的服务端错误诊断。 */
  onServerError?: (error: AxiosError) => void
}

/** 刷新失败已统一反馈，避免被业务请求的响应拦截器再次处理。 */
class RefreshFailedError extends Error {
  constructor(readonly originalError: unknown) {
    super(originalError instanceof Error ? originalError.message : 'Token refresh failed')
  }
}

/** HTTP 传输与鉴权重试；界面反馈通过应用提供的回调连接。 */
export function createHttpClient(options: ClientOptions) {
  const client = axios.create(options.config)
  const now = options.now ?? Date.now
  let expiredSession: string | undefined
  const expire = (sessionId: string): void => {
    if (options.session?.read().sessionId !== sessionId) return
    options.session.invalidate(sessionId)
    if (expiredSession !== sessionId) {
      expiredSession = sessionId
      options.feedback.expired()
    }
  }

  const refreshSession = async (sessionId: string, failedAccessToken?: string, showError = true): Promise<void> => {
    try {
      await options.session!.refresh(failedAccessToken)
    } catch (error) {
      if (error instanceof SessionChangedError) throw error
      if (error instanceof SessionExpiredError || (axios.isAxiosError(error) && error.response?.status === 401)) {
        expire(sessionId)
      } else if (!axios.isCancel(error) && showError) {
        options.feedback.error(error)
      }
      throw new RefreshFailedError(error)
    }
  }

  client.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
    const session = options.session
    if (session) {
      let token = session.read()
      if (config.authSessionId && config.authSessionId !== token.sessionId) {
        throw new SessionChangedError('Session changed before request')
      }
      config.authSessionId = token.sessionId
      if (!token.sessionId || !token.refreshToken || token.refreshTokenExpires <= now()) {
        expire(token.sessionId)
        throw new SessionChangedError('Session expired')
      }
      expiredSession = undefined
      if (!token.accessToken || token.expires <= now()) {
        await refreshSession(token.sessionId, undefined, config.errorFeedback !== false)
        token = session.read()
      }
      if (config.authSessionId !== token.sessionId) throw new SessionChangedError('Session changed while preparing request')
      config.headers.Authorization = `${token.tokenType} ${token.accessToken}`
    } else {
      config.headers.delete('Authorization')
    }
    if (config.data instanceof FormData) config.headers.delete('Content-Type')
    else config.headers.setContentType('application/json', false)
    return config
  })

  const fail = async (error: unknown): Promise<AxiosResponse> => {
    if (error instanceof SessionChangedError || error instanceof RefreshFailedError || axios.isCancel(error)) throw error
    if (axios.isAxiosError(error)) {
      const { config, response } = error
      const session = options.session
      if (session && config?.authSessionId !== undefined && config.authSessionId !== session.read().sessionId) {
        throw new SessionChangedError('Session changed before error response')
      }
      if (response && response.status >= 500) {
        try { options.onServerError?.(error) } catch { /* 诊断失败不影响业务错误传播。 */ }
      }
      if (response?.status === 401 && config && session) {
        if (config.authSessionId !== session.read().sessionId) throw new SessionChangedError('Session changed after request')
        const failed = String(config.headers.Authorization ?? '').replace(/^Bearer\s+/i, '')
        if (config.tokenRefreshRetried) {
          if (session.read().accessToken === failed) expire(config.authSessionId ?? '')
          throw error
        }
        await refreshSession(config.authSessionId ?? '', failed, config.errorFeedback !== false)
        config.tokenRefreshRetried = true
        return client(config)
      }
      if (response?.status === 403) options.feedback.forbidden()
      if (config?.errorFeedback !== false) options.feedback.error(error)
    } else if (!options.session || options.session.read().sessionId) options.feedback.error(error)
    throw error
  }

  client.interceptors.response.use((response) => {
    if (options.session && response.config.authSessionId !== options.session.read().sessionId) {
      throw new SessionChangedError('Session changed before response')
    }
    const body: unknown = response.data
    if (body && typeof body === 'object' && 'status' in body &&
        typeof body.status === 'number' && (body.status < 200 || body.status >= 300)) {
      return fail(new AxiosError('API operation failed', 'ERR_BUSINESS', response.config, response.request,
        { ...response, status: body.status }))
    }
    return response
  }, fail)
  return client
}
