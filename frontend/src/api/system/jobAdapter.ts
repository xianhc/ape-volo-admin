import type { Job } from './types/job.types'

/** 按写入契约取字段，并清理另一种触发方式的配置。 */
export function toJobRequest(source: Partial<Job>) {
  return {
    id: source.id,
    jobName: source.jobName,
    jobGroup: source.jobGroup,
    assemblyName: source.assemblyName,
    className: source.className,
    description: source.description,
    principal: source.principal,
    alertEmail: source.alertEmail,
    pauseAfterFailure: source.pauseAfterFailure,
    startTime: source.startTime || null,
    endTime: source.endTime || null,
    triggerType: source.triggerType,
    cron: source.triggerType === 1 ? source.cron : null,
    intervalSecond: source.triggerType === 2 ? source.intervalSecond : null,
    cycleRunTimes: source.triggerType === 2 ? (source.cycleRunTimes ?? 0) : null,
    enabled: source.enabled,
    runParams: source.runParams
  }
}
