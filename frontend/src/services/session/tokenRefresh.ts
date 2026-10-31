import type { ApeToken, SetTokenData } from '@/services/session/tokenStorage'

export class SessionChangedError extends Error {}
export class SessionExpiredError extends Error {}

interface RefreshDependencies {
  read: () => ApeToken
  save: (token: SetTokenData) => void
  request: (refreshToken: string) => Promise<SetTokenData>
  withLock: (work: () => Promise<void>) => Promise<void>
  now?: () => number
}

/** 合并同页刷新，并在跨标签页锁内重新读取凭证，避免重复消费及退出后恢复登录。 */
export const createTokenRefresher = (dependencies: RefreshDependencies) => {
  let pending: Promise<void> | null = null
  const now = dependencies.now || Date.now

  return (failedAccessToken?: string): Promise<void> => {
    const original = dependencies.read()
    if (
      failedAccessToken &&
      original.accessToken !== failedAccessToken &&
      original.accessToken &&
      original.expires > now()
    ) {
      return Promise.resolve()
    }

    if (!pending) {
      pending = dependencies.withLock(async () => {
        const current = dependencies.read()
        if (current.sessionId !== original.sessionId) {
          throw new SessionChangedError('Session changed while waiting for refresh')
        }
        if (!current.refreshToken || current.refreshTokenExpires <= now()) {
          throw new SessionExpiredError('Refresh token expired')
        }
        if (current.refreshToken !== original.refreshToken) {
          if (current.accessToken && current.expires > now()) return
          throw new SessionChangedError('Session changed while waiting for refresh')
        }

        const pair = await dependencies.request(current.refreshToken)
        if (dependencies.read().sessionId !== current.sessionId ||
            dependencies.read().refreshToken !== current.refreshToken) {
          throw new SessionChangedError('Session changed while refreshing')
        }
        if (
          pair.sessionId !== current.sessionId || !pair.accessToken || !pair.refreshToken ||
          !Number.isFinite(pair.expires) || !Number.isFinite(pair.refreshTokenExpires) ||
          pair.expires! <= now() || pair.refreshTokenExpires! <= now()
        ) {
          throw new SessionExpiredError('Invalid token refresh response')
        }
        dependencies.save(pair)
      }).finally(() => {
        pending = null
      })
    }
    return pending
  }
}
