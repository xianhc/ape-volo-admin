import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'

/**
 * 异常日志实体接口
 */
export interface ExceptionLog extends BaseEntity {
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
  /** 请求IP */
  requestIp?: string
  /** IP地址 */
  ipAddress?: string
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
  /** 异常消息 */
  exceptionMessage?: string
  /** 完整异常消息 */
  exceptionMessageFull?: string
  /** 异常堆栈 */
  exceptionStack?: string
  /** 日志时间 */
  logDateTime?: string
  /** 创建者账号 */
  creatorName?: string
}

/**
 * 异常日志查询参数接口
 */
export interface ExceptionLogQueryParams extends BaseQueryParams {
  /** 控制器 */
  controller?: string
  /** 操作方法 */
  action?: string
  /** 请求方法 */
  method?: string
  /** 描述 */
  description?: string
  /** 创建者账号 */
  creatorName?: string
}

/**
 * 异常日志分页响应接口
 */
export type ExceptionLogPageResponse = BasePageResponse<ExceptionLog>
