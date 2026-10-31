import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'

/**
 * 操作日志实体接口
 */
export interface OperateLog extends BaseEntity {
  /** 模块 */
  area?: string
  /** 控制器 */
  controller?: string
  /** 操作方法 */
  action?: string
  /** 请求方法 */
  method?: string
  /** 描述 */
  description?: string
  /** 请求URL */
  requestUrl?: string
  /** 请求参数 */
  requestParameters?: string
  /** 响应数据 */
  responseData?: string
  /** 执行时长 */
  executionDuration?: number
  /** IP地址 */
  ipAddress?: string
  /** 请求IP */
  requestIp?: string
  /** 位置 */
  location?: string
  /** 操作系统 */
  operatingSystem?: string
  /** 设备类型 */
  deviceType?: string
  /** 浏览器名称 */
  browserName?: string
  /** 浏览器版本 */
  version?: string
  /** 用户代理 */
  userAgent?: string
  /** 日志时间 */
  logDateTime?: string
  /** 用户名 */
  username?: string
  /** 创建者账号 */
  creatorName?: string
}

/**
 * 操作日志查询参数接口
 */
export interface OperateCurrentLogQueryParams extends BaseQueryParams {
  pageIndex?: number
  pageSize?: number
}

/**
 * 操作日志查询参数接口
 */
export interface OperateLogQueryParams extends BaseQueryParams {
  /** 用户名 */
  username?: string
  /** 描述 */
  description?: string
  /** 请求方法 */
  method?: string
}

/**
 * 操作日志分页响应接口
 */
export type OperateLogPageResponse = BasePageResponse<OperateLog>

/**
 * 访问趋势数据接口
 */
export interface VisitTrendData {
  /** 日期列表 */
  dateList: string[]
  /** 操作日志数量列表 */
  operateList: number[]
  /** 异常日志数量列表 */
  exceptionList: number[]
}
