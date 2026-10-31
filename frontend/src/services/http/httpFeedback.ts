export interface HttpFeedback {
  error: (error: unknown) => void
  expired: () => void
  forbidden: () => void
}

let handlers: HttpFeedback = { error: () => {}, expired: () => {}, forbidden: () => {} }
const reported = new WeakSet<object>()

/** 应用启动时连接 UI；请求核心无需导入 Element Plus、路由或 Store。 */
export function configureHttpFeedback(next: HttpFeedback): void { handlers = next }

export const httpFeedback: HttpFeedback = {
  error(error) {
    if (error && typeof error === 'object') {
      if (reported.has(error)) return
      reported.add(error)
    }
    handlers.error(error)
  },
  expired: () => handlers.expired(),
  forbidden: () => handlers.forbidden()
}
