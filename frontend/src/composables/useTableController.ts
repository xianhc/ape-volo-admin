import { computed, getCurrentScope, onScopeDispose, ref, shallowRef } from 'vue'
import type { ComputedRef, Ref } from 'vue'
import type { PagedAndSortedResultRequestDto } from '@/api/types/base'
import type {
  ControllerReadiness,
  ListAdapter,
  PaginationBindings,
  QueryRef,
  SortOrder
} from '@/components/crud/crud.types'

/**
 * 列表查询适配器、初始条件、分页排序及就绪约束。
 * @typeParam TQuery 列表筛选、分页与排序请求的类型。
 * @typeParam TItem 列表中单条业务记录的类型。
 */
export interface TableControllerOptions<
  TQuery extends PagedAndSortedResultRequestDto,
  TItem
> {
  /** 由所属功能提供的列表查询适配器。 */
  list: ListAdapter<TQuery, TItem>
  /** 当前查询的筛选条件。 */
  query: QueryRef<TQuery>
  /**
   *  创建初始筛选条件或配置的函数。
   * @returns 新的页面筛选条件或初始配置，调用方可独立修改。
   */
  defaults: () => TQuery
  /** 查询前需要满足的初始化状态与操作。 */
  readiness?: ControllerReadiness
  /** 可选择的每页条目数集合。 */
  pageSizes?: number[]
  /** 每页显示的条目数。 */
  defaultPageSize?: number
  /** 初始排序字段与方向。 */
  defaultSort?: {
    /** 表格列绑定的属性名或排序字段。 */
    prop: string
    /** 当前排序方向。 */
    order: SortOrder
  }
  /** 是否在初始化时立即查询。 */
  refreshOnInit?: boolean
  /**
   *  判断当前上下文是否允许发起查询。
   * @returns 当前页面上下文允许发起列表查询时为 true。
   */
  canLoad?: () => boolean
}

/**
 * 列表数据、分页排序、选择项和查询状态控制器。
 * @typeParam TQuery 列表筛选、分页与排序请求的类型。
 * @typeParam TItem 列表中单条业务记录的类型。
 */
export interface TableController<
  TQuery extends PagedAndSortedResultRequestDto,
  TItem
> {
  /** 当前记录或响应内容。 */
  data: Ref<TItem[]>
  /** 当前查询的筛选条件。 */
  query: QueryRef<TQuery>
  /** 是否正在加载数据。 */
  loading: Ref<boolean>
  /** 当前操作保留的错误，供界面反馈或重试。 */
  error: Ref<unknown>
  /** 最近一次加载或操作是否失败。 */
  loadFailed: ComputedRef<boolean>
  /** 是否展开搜索区域。 */
  searchVisible: Ref<boolean>
  /** 当前选中的记录集合。 */
  selection: Ref<TItem[]>
  /** 当前页码，从第一页开始。 */
  pageIndex: Ref<number>
  /** 每页显示的条目数。 */
  pageSize: Ref<number>
  /** 匹配当前条件的记录总数，用于计算可用页码。 */
  total: Ref<number>
  /** 表格列绑定的属性名或排序字段。 */
  sortProp: Ref<string>
  /** 当前排序方向。 */
  sortOrder: Ref<SortOrder>
  /** 分页状态与事件绑定。 */
  pagination: ComputedRef<PaginationBindings>
  /** 重新加载当前数据的操作。 */
  refresh: () => Promise<void>
  /** 从第一页开始查询的操作。 */
  search: () => Promise<void>
  /** 恢复初始筛选与排序后重新查询的操作。 */
  resetQuery: () => Promise<void>
  /**
   *  选中记录变化时的回调。
   * @param rows 已经整理的 Redis 命令统计条目。
   */
  onSelectionChange: (rows: TItem[]) => void
  /**
   *  表格排序变化时的回调。
   * @param sort 表格当前排序字段和方向。
   */
  onSortChange: (sort: {
    /** 表格列绑定的属性名或排序字段。 */
    prop: string
    /** 当前排序方向。 */
    order: 'ascending' | 'descending' | null
  }) => Promise<void>
  /**
   *  取得当前筛选条件，并按调用方要求合并分页与排序。
   * @returns 当前有效的筛选、分页及排序条件快照。
   * @param includePagination 是否在当前筛选和排序中附带分页参数。
   */
  currentQuery: (includePagination?: boolean) => TQuery
}

/**
 * 协调列表查询、分页排序、选择项及加载错误状态。
 *
 * search 与 resetQuery 从第一页查询，refresh 保留有效的当前页；页码因删除而失效时最多补查一次。
 * 请求代次阻止旧响应覆盖新结果，成功刷新后清空选择项；失败保留在 error，卸载后不再应用响应。
 * @typeParam TQuery 页面筛选、分页及排序请求的类型。
 * @typeParam TItem 单条列表记录的类型。
 * @param options 列表请求、初始条件、排序及查询就绪约束。
 * @returns 响应式列表状态、分页绑定及查询操作入口。
 */
export function useTableController<
  TQuery extends PagedAndSortedResultRequestDto,
  TItem
>(
  options: TableControllerOptions<TQuery, TItem>
): TableController<TQuery, TItem> {
  const pageSizes = options.pageSizes ?? [10, 20, 30, 50, 100]
  const data = ref<TItem[]>([]) as Ref<TItem[]>
  const loading = ref(false)
  const error = shallowRef<unknown>(null)
  const loadFailed = computed(() => error.value !== null)
  const searchVisible = ref(true)
  const selection = ref<TItem[]>([]) as Ref<TItem[]>
  const pageIndex = ref(1)
  const pageSize = ref(options.defaultPageSize ?? pageSizes[0] ?? 10)
  const total = ref(0)
  const sortProp = ref(options.defaultSort?.prop ?? 'id')
  const sortOrder = ref<SortOrder>(options.defaultSort?.order ?? 'descending')
  let generation = 0
  let disposed = false
  let activeRequest: AbortController | undefined
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
   * 取得当前操作使用的身份、筛选条件或初始上下文。
   * @param includePagination 是否在当前筛选条件中包含分页与排序。
   * @returns 按要求组合好的筛选、分页和排序条件。
   */
  const currentQuery = (includePagination = true): TQuery => ({
    ...options.query.value,
    ...(includePagination
      ? {
        pageIndex: pageIndex.value,
        pageSize: pageSize.value,
        ...(sortProp.value && sortOrder.value
          ? {
            sorting: `${sortProp.value} ${sortOrder.value === 'ascending' ? 'asc' : 'desc'}`
          }
          : {})
      }
      : {})
  })

  /**
   * 重新加载表格列表当前上下文的数据。
   */
  const refresh = async (): Promise<void> => {
    if (disposed || options.canLoad?.() === false) return
    activeRequest?.abort()
    const request = new AbortController()
    activeRequest = request
    const currentGeneration = ++generation
    loading.value = true
    error.value = null
    try {
      if (!(await ensureReady()) || disposed || currentGeneration !== generation) return
      let response = await options.list(currentQuery(), { errorFeedback: false, signal: request.signal })
      if (currentGeneration !== generation) return
      const lastPage = Math.max(
        1,
        Math.ceil(response.data.totalCount / pageSize.value)
      )
      // 删除末页最后几条后最多补查一次，避免停留在已不存在的页码。
      if (pageIndex.value > lastPage) {
        pageIndex.value = lastPage
        response = await options.list(currentQuery(), { errorFeedback: false, signal: request.signal })
      }
      if (currentGeneration !== generation) return
      data.value = response.data.items
      total.value = response.data.totalCount
      selection.value = []
    } catch (cause) {
      if (currentGeneration === generation) error.value = cause
    } finally {
      if (currentGeneration === generation) {
        loading.value = false
        activeRequest = undefined
      }
    }
  }

  /**
   * 按当前条件筛选表格列表。
   */
  const search = async (): Promise<void> => {
    pageIndex.value = 1
    await refresh()
  }

  /**
   * 替换当前查询对象的字段并保留其响应式引用。
   * @param next 准备应用的新状态或值。
   */
  const replaceQuery = (next: TQuery): void => {
    const target = options.query.value as Record<string, unknown>
    Object.keys(target).forEach((key) => delete target[key])
    Object.assign(target, next)
  }

  /**
   * 恢复表格列表初始筛选、分页和排序条件。
   */
  const resetQuery = async (): Promise<void> => {
    replaceQuery(options.defaults())
    pageIndex.value = 1
    sortProp.value = options.defaultSort?.prop ?? 'id'
    sortOrder.value = options.defaultSort?.order ?? 'descending'
    await refresh()
  }

  /**
   * 记录表格当前选中的条目。
   * @param rows 当前选中或查询得到的记录集合。
   */
  const onSelectionChange = (rows: TItem[]): void => {
    selection.value = rows
  }

  /**
   * 更新当前页码并重新查询。
   * @param page 准备切换到的页码。
   */
  const onPageChange = async (page: number): Promise<void> => {
    pageIndex.value = page
    await refresh()
  }

  /**
   * 更新每页数量并从第一页重新查询。
   * @param size 新的每页数量或组件尺寸。
   */
  const onSizeChange = async (size: number): Promise<void> => {
    pageSize.value = size
    pageIndex.value = 1
    await refresh()
  }

  /**
   * 更新排序条件并重新查询。
   * @param sort 包含排序字段与排序方向的事件数据。
   * @param sort 排序事件，其中 prop 为字段名、order 为排序方向。
   */
  const onSortChange = async ({
    prop,
    order
  }: {
    /** 表格列绑定的属性名或排序字段。 */
    prop: string
    /** 当前排序方向。 */
    order: 'ascending' | 'descending' | null
  }): Promise<void> => {
    sortProp.value = prop
    sortOrder.value = order ?? ''
    pageIndex.value = 1
    await refresh()
  }

  const pagination = computed<PaginationBindings>(() => ({
    'current-page': pageIndex.value,
    'page-size': pageSize.value,
    total: total.value,
    pageSizes,
    layout: 'total, sizes, prev, pager, next, jumper',
    onCurrentChange: onPageChange,
    onSizeChange
  }))

  if (getCurrentScope()) onScopeDispose(() => {
    disposed = true
    generation++
    activeRequest?.abort()
  })

  if (options.refreshOnInit !== false) void refresh()

  return {
    data,
    query: options.query,
    loading,
    error,
    loadFailed,
    searchVisible,
    selection,
    pageIndex,
    pageSize,
    total,
    sortProp,
    sortOrder,
    pagination,
    refresh,
    search,
    resetQuery,
    onSelectionChange,
    onSortChange,
    currentQuery
  }
}
