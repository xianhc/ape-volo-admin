/**
 * 表格增删改查类型声明。
 */
import type { AxiosResponse } from 'axios'
import type { Ref } from 'vue'
import type {
  PagedResultDto,
  PagedAndSortedResultRequestDto,
  RequestOptions
} from '@/api/types/base'

/**
 * 由列表控制器读取和更新的响应式筛选对象。
 * @typeParam TQuery 列表筛选、分页与排序请求的类型。
 */
export type QueryRef<TQuery extends PagedAndSortedResultRequestDto> =
  Ref<TQuery>

/**
 * 列表查询前必须完成的初始化状态及入口。
 */
export interface ControllerReadiness {
  /** 是否就绪。 */
  ready: Ref<boolean>
  /**
   *  初始化查询前所需上下文，并返回是否准备成功。
   * @returns 依赖初始化完成且可继续查询时为 true；失败或失效时为 false。
   */
  initialize: () => Promise<boolean>
}

/**
 * 按筛选条件读取分页列表的异步适配器。
 * @typeParam TQuery 列表筛选、分页与排序请求的类型。
 * @typeParam TItem 列表中单条业务记录的类型。
 */
export type ListAdapter<
  TQuery extends PagedAndSortedResultRequestDto,
  TItem
> = (query: TQuery, options?: RequestOptions) => Promise<AxiosResponse<PagedResultDto<TItem>>>

/**
 * 按当前筛选条件请求文件导出的适配器。
 * @typeParam TQuery 列表筛选、分页与排序请求的类型。
 */
export type ExportAdapter<TQuery extends PagedAndSortedResultRequestDto> = (
  /** 查询。 */
  query: TQuery
) => Promise<AxiosResponse<Blob>>

/**
 * 分页控件的状态值与变化事件绑定。
 */
export interface PaginationBindings {
  /** 当前页码，从第一页开始。 */
  'current-page': number
  /** 每页显示的条目数。 */
  'page-size': number
  /** 匹配当前条件的记录总数，用于计算可用页码。 */
  total: number
  /** 每页条数选项。 */
  pageSizes: number[]
  /** 布局。 */
  layout: string
  /**
   *  分页页码变化时的回调。
   * @param page 用户选择的目标页码。
   */
  onCurrentChange: (page: number) => Promise<void>
  /**
   *  每页条目数变化时的回调。
   * @param size 用户选择的每页记录数。
   */
  onSizeChange: (size: number) => Promise<void>
}

/**
 * 表格当前排序方向，空值表示不指定排序。
 */
export type SortOrder = 'ascending' | 'descending' | ''
