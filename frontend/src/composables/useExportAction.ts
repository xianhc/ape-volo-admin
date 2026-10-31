import { ref } from 'vue'
import type { Ref } from 'vue'
import type { PagedAndSortedResultRequestDto } from '@/api/types/base'
import { downloadFile } from '@/utils/downloadFile'
import type {
  ControllerReadiness,
  ExportAdapter
} from '@/components/crud/crud.types'

/**
 * 导出查询、就绪状态及文件反馈所需的依赖。
 * @typeParam TQuery 列表筛选、分页与排序请求的类型。
 */
export interface ExportActionOptions<
  TQuery extends PagedAndSortedResultRequestDto
> {
  /** 当前功能的文件导出操作。 */
  export: ExportAdapter<TQuery>
  /**
   *  取得当前筛选条件，并按调用方要求合并分页与排序。
   * @returns 当前有效的筛选、分页及排序条件快照。
   */
  currentQuery: () => TQuery
  /** 查询前需要满足的初始化状态与操作。 */
  readiness?: ControllerReadiness
  /** 响应未提供文件名时的下载名称。 */
  fallbackFileName?: string
}

/**
 * 文件导出的忙碌状态与执行入口。
 */
export interface ExportAction {
  /** 是否正在导出文件。 */
  exporting: Ref<boolean>
  /** 在确认、行锁及业务结果校验下执行写入。 */
  run: () => Promise<void>
}

/**
 * 从下载响应头提取文件名，解码后移除路径分隔符、控制字符和前导点。
 * @param disposition Content-Disposition 响应头原文。
 * @param fallback 响应未提供有效文件名时使用的名称。
 * @returns 可用于下载的文件名；响应头缺失或清理后为空时使用备用名。
 */
export const getDownloadFileName = (
  disposition: string | undefined,
  fallback: string
): string => {
  if (!disposition) return fallback
  const encoded = /filename\*\s*=\s*(?:UTF-8'')?"?([^";]+)"?/i.exec(
    disposition
  )?.[1]
  const plain = /(?:^|;)\s*filename\s*=\s*"?([^";]+)"?/i.exec(disposition)?.[1]
  const candidate = (encoded ?? plain)?.replace(/^['"]|['"]$/g, '')
  if (!candidate) return fallback
  let decoded = candidate
  try {
    decoded = decodeURIComponent(candidate)
  } catch {
    // Malformed percent encoding still yields a safe fallback name below.
  }
  const sanitized = Array.from(decoded, (character) => {
    const code = character.charCodeAt(0)
    return character === '/' || character === '\\' || code < 32 || code === 127
      ? '_'
      : character
  })
    .join('')
    .replace(/^\.+/, '')
    .trim()
  return sanitized || fallback
}

/**
 * 等待筛选初始化后按当前查询导出文件，防止同一次导出重复提交。
 * @param options 导出请求、当前筛选、就绪状态及备用文件名。
 * @returns 导出中的状态和执行下载的 run 入口。
 * @typeParam TQuery 导出使用的筛选、分页及排序条件类型。
 */
export function useExportAction<TQuery extends PagedAndSortedResultRequestDto>(
  options: ExportActionOptions<TQuery>
): ExportAction {
  const exporting = ref(false)
  let readinessPromise: Promise<boolean> | undefined

  /**
   * 等待查询依赖初始化，并共享同一次初始化任务。
   * @returns 查询依赖初始化成功时返回 true，否则返回 false。
   */
  const ensureReady = async (): Promise<boolean> => {
    if (!options.readiness || options.readiness.ready.value) return true
    readinessPromise ??= options.readiness.initialize().finally(() => {
      readinessPromise = undefined
    })
    return readinessPromise
  }

  /**
   * 读取当前筛选条件、请求导出文件并下载；完成或失败后释放导出忙碌状态。
   */
  const run = async (): Promise<void> => {
    if (exporting.value) return
    exporting.value = true
    try {
      if (!(await ensureReady())) return
      const response = await options.export(options.currentQuery())
      const fileName = getDownloadFileName(
        response.headers['content-disposition'],
        options.fallbackFileName ?? 'download.xlsx'
      )
      downloadFile(response.data, fileName)
    } finally {
      exporting.value = false
    }
  }

  return { exporting, run }
}
