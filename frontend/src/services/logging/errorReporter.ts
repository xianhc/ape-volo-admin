/** 仅传输固定事件码和运行上下文，不传错误正文、堆栈、URL 或请求数据。 */
export interface ClientErrorReport {
  code: string
  errorName: string
  routeName: string
  build: string
  requestId: string
}

export interface ErrorReportContext {
  routeName?: string
  build?: string
  requestId?: string
}

const identifier = (value: unknown, max: number): string =>
  typeof value === 'string' ? value.replace(/[^a-zA-Z0-9_.:-]/g, '').slice(0, max) : ''

/** 每分钟最多发送十条，同类错误每分钟只发一次；发送失败不递归报告。 */
export function createErrorReporter(options: {
  send: (report: ClientErrorReport) => Promise<unknown>
  context: () => ErrorReportContext
  now?: () => number
}) {
  const now = options.now ?? Date.now
  let windowStart = now()
  const sent = new Set<string>()
  return (code: string, error?: unknown, context: ErrorReportContext = {}): void => {
    try {
      const time = now()
      if (time - windowStart >= 60_000 || time < windowStart) {
        windowStart = time
        sent.clear()
      }
      const current = { ...options.context(), ...context }
      const report: ClientErrorReport = {
        code: /^[a-z][a-z0-9.-]{0,63}$/.test(code) ? code : 'app.error',
        errorName: error instanceof Error ? identifier(error.name, 64) || 'Error' : 'UnknownError',
        routeName: identifier(current.routeName, 100),
        build: identifier(current.build, 64),
        requestId: identifier(current.requestId, 128)
      }
      const key = [report.code, report.errorName, report.routeName].join(':')
      if (sent.size >= 10 || sent.has(key)) return
      sent.add(key)
      void options.send(report).catch(() => {})
    } catch {
      // 诊断通道不能影响业务操作。
    }
  }
}
