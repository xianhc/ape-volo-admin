import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity,
  EntityId
} from '../../types/base'

/**
 * 任务日志实体接口
 */
export interface JobLog extends BaseEntity {
  /** 作业ID */
  jobId?: EntityId
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
  /** 执行时长（毫秒）；后端 long 使用字符串传输。 */
  executionDuration?: string | number
  /** 是否执行成功 */
  isSuccess?: boolean
  /** 记录来源账号 */
  creatorName?: string
  /** 异常详情 */
  exceptionDetail?: string
}

/**
 * 任务日志查询参数接口
 */
export interface JobLogQueryParams extends BaseQueryParams {
  /** 作业ID */
  jobId?: EntityId
  /** 是否执行成功（对应后端 IsSuccess） */
  isSuccess?: boolean
}

/**
 * 任务日志分页响应接口
 */
export type JobLogPageResponse = BasePageResponse<JobLog>
