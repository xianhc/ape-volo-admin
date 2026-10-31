import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'

/**
 * 任务日志实体接口
 */
export interface ScheduledJobLog extends BaseEntity {
  /** 任务名称 */
  jobName?: string
  /** 任务组 */
  jobGroup?: string
  /** 程序集名称 */
  assemblyName?: string
  /** 类名 */
  className?: string
  /** Cron表达式 */
  cron?: string
  /** 运行参数 */
  runParams?: string
  /** 执行时间 */
  executionTime?: string
  /** 执行时长 */
  executionDuration?: number
  /** 执行状态 */
  status?: string
  /** 执行结果 */
  result?: string
  /** 异常消息 */
  exceptionMessage?: string
  /** 异常详情 */
  exceptionDetail?: string
}

/**
 * 任务日志查询参数接口
 */
export interface ScheduledJobLogQueryParams extends BaseQueryParams {
  /** 任务名称 */
  jobName?: string
  /** 是否执行成功（对应后端 IsSuccess） */
  isSuccess?: boolean
}

/**
 * 任务日志分页响应接口
 */
export type ScheduledJobLogPageResponse = BasePageResponse<ScheduledJobLog>
