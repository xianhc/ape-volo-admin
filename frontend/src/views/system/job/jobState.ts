import type { Job } from '@/api/system/types/job.types'

export type JobAction = 'start' | 'pause' | 'resume' | 'restart' | 'stop'
type TagType = 'info' | 'success' | 'warning' | 'danger'
interface StateInfo { label: string; type: TagType; icon: string; description: string }

const states: Record<number, StateInfo> = {
  0: { label: '未调度', type: 'info', icon: 'lucide:circle-dashed', description: '当前没有调度计划，可启动调度。' },
  1: { label: '调度中', type: 'success', icon: 'lucide:clock', description: '按计划触发，不代表此刻正在执行。' },
  2: { label: '已暂停', type: 'warning', icon: 'lucide:pause', description: '调度计划保留，恢复后继续按计划触发。' },
  3: { label: '已完成', type: 'info', icon: 'lucide:circle-check', description: '触发计划已结束，可编辑配置后重新调度。' },
  4: { label: '调度异常', type: 'danger', icon: 'lucide:triangle-alert', description: '请检查执行类与调度配置，再重新调度。' },
  5: { label: '阻塞', type: 'warning', icon: 'lucide:hourglass', description: '触发器暂被阻塞，等待调度器释放。' }
}

/** 状态缺失时显式显示未知，避免误报为未调度。 */
export function getJobState(state?: number): StateInfo {
  return states[state ?? -1] ?? { label: '状态未知', type: 'info', icon: 'lucide:circle-help', description: '请刷新列表后再操作。' }
}

/** 按运行时状态提供可执行操作；启用配置与运行状态可能暂时不一致。 */
export function getJobActions(job: Partial<Job>): JobAction[] {
  switch (job.triggerState) {
    case 0: return ['start']
    case 1: return ['pause', 'stop']
    case 2: return ['resume', 'stop']
    case 3:
    case 4: return ['restart', 'stop']
    case 5: return ['stop']
    default: return []
  }
}

export function getJobWarning(job: Partial<Job>): string {
  if (job.enabled && job.triggerState === 0) return '配置已启用，当前尚未进入调度'
  if (!job.enabled && job.triggerState !== undefined && job.triggerState >= 1 && job.triggerState <= 5) return '配置已停用，当前仍有调度计划'
  return ''
}

export function getJobSchedule(job: Partial<Job>) {
  if (job.triggerType === 1) return { mode: 'Cron', rule: job.cron || '未配置表达式', repeat: '按表达式触发' }
  if (job.triggerType === 2) return {
    mode: '间隔',
    rule: job.intervalSecond ? `每 ${job.intervalSecond} 秒` : '未配置执行间隔',
    repeat: job.cycleRunTimes && job.cycleRunTimes > 0 ? `共 ${job.cycleRunTimes} 次` : '持续触发'
  }
  return { mode: '未知模式', rule: '请编辑调度配置', repeat: '' }
}

/** 详情仅保留有值的字段，未设置的起止时间不占用页面空间。 */
export function getJobDetails(job: Partial<Job>) {
  const present = (value: unknown) => value !== undefined && value !== null && String(value).trim() !== ''
  const groups = [
    { title: '执行配置', fields: [
      { label: '程序集', value: job.assemblyName }, { label: '执行类', value: job.className },
      { label: '执行参数', value: job.runParams },
      { label: '失败后暂停', value: job.pauseAfterFailure === undefined ? undefined : job.pauseAfterFailure ? '是' : '否' }
    ] },
    { title: '补充信息', fields: [
      { label: '负责人', value: job.principal }, { label: '告警邮箱', value: job.alertEmail },
      { label: '开始时间', value: job.startTime }, { label: '结束时间', value: job.endTime },
      { label: '创建时间', value: job.creationTime }, { label: '描述', value: job.description }
    ] }
  ]
  return groups.map(group => ({ ...group, fields: group.fields.filter(field => present(field.value)) }))
    .filter(group => group.fields.length > 0)
}
