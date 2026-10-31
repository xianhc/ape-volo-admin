/**
 * 基础查询参数接口
 * 注意：分页参数（pageIndex, pageSize）和排序参数（sorting）由 Table Controller 自动注入
 * 各模块需要的特定查询字段（如时间范围、关键字等）请在各自的 QueryParams 中定义
 */
export interface LimitedResultRequestDto {
  /** 每页条数。 */
  pageSize?: number
}

/**
 * 列表查询使用的页码及每页条目数。
 */
export interface PagedResultRequestDto extends LimitedResultRequestDto {
  /** 页码。 */
  pageIndex?: number
}

/**
 * 同时支持分页及允许字段排序的列表请求。
 */
export interface PagedAndSortedResultRequestDto extends PagedResultRequestDto {
  /** 排序。 */
  sorting?: string
}

/**
 * 业务实体标识，保留字符串表示以避免长整型精度丢失。
 */
export type EntityId = string

/** 调用方展示局部错误时，显式关闭该请求的全局错误提示。 */
export interface RequestOptions {
  errorFeedback?: boolean
  /** 取消已被新查询替代或随页面卸载结束的请求。 */
  signal?: AbortSignal
}

/**
 * 基础分页响应接口
 * @typeParam T 返回列表中的单条记录类型。
 */
export interface ListResultDto<T> {
  /** 数据内容 */
  items: T[]
}

/**
 * 包含当前页记录和总记录数的分页响应。
 * @typeParam T 当前页中的单条业务记录类型。
 */
export interface PagedResultDto<T> extends ListResultDto<T> {
  /** 总记录数 */
  totalCount: number
}

/**
 * 基础实体接口（与后端 AuditedEntity：IEntity<long> + IAuditedEntity + ISoftDeletedEntity 对齐）
 */
export interface BaseEntity {
  /** 主键ID */
  id: EntityId
  /** 创建者主键；后台任务或匿名写入时为空 */
  creatorId?: EntityId | null
  /** 创建时间 */
  creationTime: string
  /** 最后修改者主键 */
  lastModifierId?: EntityId | null
  /** 最后修改时间 */
  lastModificationTime?: string | null
  /** 是否已删除 */
  isDeleted?: boolean
}

/**
 * 通用操作响应接口
 * @typeParam T 响应 data 字段承载的业务数据类型。
 */
export interface ApiResponse<T = never> {
  /** 与 HTTP 响应一致的操作状态码 */
  status: number
  /** 错误信息 */
  actionError: {
    /** 导入或校验发现的错误集合。 */
    errors: Record<string, string[]>
  } | null
  /** 编码。 */
  code?: string | null
  /** 消息参数。 */
  messageArguments?: Record<string, string | number> | null
  /** 服务端按字段返回的校验错误集合。 */
  validationErrors?: Array<{
    /** 属性名称。 */
    propertyName: string
    /** 字段键。 */
    fieldKey?: string | null
    /** 编码。 */
    code: string
    /** 消息。 */
    message: string
    /** 消息参数。 */
    messageArguments?: Record<string, string | number>
  }>
  /** 提示消息 */
  message: string
  /** 时间戳 */
  timestamp: string
  /** 请求路径 */
  path: string
  /** 响应数据 */
  data?: T
}

/**
 * 基础查询参数接口（本项目后端契约）
 * 各模块需要的特定查询字段（如时间范围、关键字等）请在各自的 QueryParams 中定义。
 */
export interface BaseQueryParams extends PagedAndSortedResultRequestDto {
  /** 占位：作为所有查询参数的基类。 */
}

/**
 * 基础分页响应接口（与后端 PagedResultDto 对齐：items / totalCount）
 * @typeParam T 返回列表中的单条记录类型。
 */
export type BasePageResponse<T> = PagedResultDto<T>

/** 增删改操作成功响应，与后端 OperateResult 对齐。 */
export interface OperationResult {
  isSuccess: boolean
  message: string
}

/** 编辑草稿不要求已经存在的审计字段。 */
export type EntityDraft<T extends { id: EntityId }> = Partial<Omit<T, keyof BaseEntity>> & { id?: EntityId }
