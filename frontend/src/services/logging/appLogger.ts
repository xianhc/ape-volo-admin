import type { ErrorReportContext } from './errorReporter'
import type { AxiosError } from 'axios'

type Reporter = (code: string, error?: unknown, context?: ErrorReportContext) => void
let report: Reporter = () => {}

/** 连接应用提供的错误收集器；默认不进行网络访问。 */
export function configureErrorReporter(reporter: Reporter): void { report = reporter }

/** 保留服务端关联标识，诊断收集不依赖全局错误弹窗。 */
export function reportServerError(error: AxiosError): void {
  appLogger.error('http.server', error, { requestId: String(error.response?.headers['x-request-id'] ?? '') })
}
interface SafeError {
  /** 当前对象的名称。 */
  name: string
  /** 供调用方阅读的结果说明。 */
  message: string
}

/**
 * 取得适合当前界面展示的错误信息。
 * @param error 当前捕获的未知错误。
 * @returns 只包含错误名称与消息的安全摘要；未知结构为 undefined。
 */
const safeError = (error: unknown): SafeError | undefined =>
  error instanceof Error
    ? { name: error.name, message: error.message }
    : undefined

export const appLogger = {
  /**
   * 在开发环境输出经过安全整理的警告信息。
   * @param message 供调用方阅读的结果说明。
   * @param error 当前捕获的未知错误。
   */
  warn(message: string, error?: unknown): void {
    if (import.meta.env.DEV) console.warn(message, safeError(error))
  },
  /**
   * 读取当前操作保留的错误，供界面反馈或重试。
   * @param message 供调用方阅读的结果说明。
   * @param error 当前捕获的未知错误。
   */
  error(message: string, error?: unknown, context?: ErrorReportContext): void {
    if (import.meta.env.DEV) console.error(message, safeError(error))
    report(message, error, context)
  }
}
