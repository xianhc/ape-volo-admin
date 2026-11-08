<template>
  <div class="table-managed-view job-log-page">
    <TablePage
      v-model:search-visible="searchToggle"
      title="作业日志"
      :data="data"
      :columns="tableColumns"
      :error="tableError"
      :loading="loading"
      row-key="id"
      @refresh="refresh"
      @sort-change="onSortChange"
    >
      <template #search>
        <SearchPanel :model="searchInfo" :loading="loading" @query="search" @reset="resetQuery">
          <el-form-item label="作业名称">
            <el-select v-model="searchInfo.jobId" filterable clearable placeholder="全部作业">
              <el-option v-for="item in allJobData" :key="item.id" :label="item.jobName" :value="item.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="执行结果">
            <el-select v-model="searchInfo.isSuccess" clearable placeholder="全部结果">
              <el-option label="成功" :value="true" />
              <el-option label="失败" :value="false" />
            </el-select>
          </el-form-item>
          <template #advanced><DateRangePicker v-model="searchInfo" /></template>
        </SearchPanel>
      </template>
      <template #toolbar-left>
        <el-button v-has-role="['admin']" type="warning" :icon="createIconifyIcon('lucide:download')" :loading="exporting" @click="runExport">
          导出
        </el-button>
      </template>
      <template #column-jobName="{ row }">
        <div class="job-log-name">{{ row.jobName || '未命名作业' }}</div>
        <div class="job-log-secondary">{{ row.jobGroup || '未记录分组' }}</div>
      </template>
      <template #column-isSuccess="{ row }">
        <el-tag :type="executionState(row.isSuccess).type" effect="light" class="job-log-state">
          <AppIcon :icon="executionState(row.isSuccess).icon" :size="14" />
          {{ executionState(row.isSuccess).label }}
        </el-tag>
      </template>
      <template #column-executionDuration="{ row }">
        <span class="job-log-duration" :title="durationInMilliseconds(row.executionDuration)">
          {{ formatDuration(row.executionDuration) }}
        </span>
      </template>
      <template #column-executionTarget="{ row }">
        <div class="job-log-code job-log-truncate" :title="row.className || undefined">{{ row.className || '未记录执行类' }}</div>
        <div class="job-log-secondary job-log-truncate" :title="row.assemblyName || undefined">{{ row.assemblyName || '未记录程序集' }}</div>
      </template>
      <template #column-exceptionDetail="{ row }">
        <div class="job-log-summary" :class="{ 'is-failure': row.isSuccess === false }" :title="resultSummary(row)">
          {{ resultSummary(row) }}
        </div>
        <div v-if="hasText(row.runParams)" class="job-log-secondary job-log-truncate" :title="row.runParams">参数：{{ row.runParams }}</div>
      </template>
      <template #column-runParams="{ row }"><span class="job-log-code job-log-truncate">{{ row.runParams || '未传入参数' }}</span></template>
      <template #column-cron="{ row }"><span class="job-log-code">{{ row.cron || '未记录' }}</span></template>
      <template #column-actions="{ row }">
        <RowActions>
          <el-button :icon="createIconifyIcon('lucide:eye')" type="primary" link @click="showJobLogDetail(row)">详情</el-button>
        </RowActions>
      </template>
      <template #pagination><el-pagination v-bind="pagination" /></template>
    </TablePage>

    <el-drawer
      v-model="jobLogDetailDrawer"
      direction="rtl"
      :size="viewportWidth < 768 ? '100%' : 'min(860px, 92vw)'"
      class="job-log-drawer"
    >
      <template #header><span class="job-log-drawer-title">作业执行详情</span></template>
      <div v-if="currentRow" class="job-log-detail">
        <section class="job-log-overview" :class="{ 'is-success': currentRow.isSuccess === true, 'is-failure': currentRow.isSuccess === false }" aria-label="执行概览">
          <div class="job-log-overview-heading">
            <div>
              <h2>{{ currentRow.jobName || '未命名作业' }}</h2>
              <p>{{ currentRow.jobGroup || '未记录分组' }}</p>
            </div>
            <el-tag :type="executionState(currentRow.isSuccess).type" effect="light" class="job-log-state">
              <AppIcon :icon="executionState(currentRow.isSuccess).icon" :size="15" />
              {{ executionState(currentRow.isSuccess).label }}
            </el-tag>
          </div>
          <div class="job-log-overview-meta">
            <div><span>记录时间</span><strong>{{ currentRow.creationTime || '未记录' }}</strong></div>
            <div><span>执行耗时</span><strong :title="durationInMilliseconds(currentRow.executionDuration)">{{ formatDuration(currentRow.executionDuration) }}</strong></div>
          </div>
        </section>

        <section class="job-log-section">
          <h3><AppIcon icon="lucide:workflow" :size="17" />执行信息</h3>
          <dl class="job-log-fields">
            <div><dt>作业 ID</dt><dd class="job-log-code">{{ currentRow.jobId || '未记录' }}</dd></div>
            <div><dt>记录来源</dt><dd>{{ currentRow.creatorName || '未记录' }}</dd></div>
            <div class="is-wide"><dt>程序集</dt><dd class="job-log-code">{{ currentRow.assemblyName || '未记录' }}</dd></div>
            <div class="is-wide"><dt>执行类</dt><dd class="job-log-code">{{ currentRow.className || '未记录' }}</dd></div>
            <div class="is-wide"><dt>Cron 表达式</dt><dd class="job-log-code">{{ currentRow.cron || '本条日志未记录 Cron 表达式' }}</dd></div>
          </dl>
        </section>

        <section class="job-log-section">
          <h3><AppIcon icon="lucide:braces" :size="17" />执行参数</h3>
          <pre v-if="hasText(currentRow.runParams)" class="job-log-pre">{{ formatParameters(currentRow.runParams) }}</pre>
          <p v-else class="job-log-empty">本次执行未传入参数。</p>
        </section>

        <section class="job-log-section" :class="{ 'job-log-error-section': currentRow.isSuccess === false }">
          <h3><AppIcon :icon="hasText(currentRow.exceptionDetail) ? 'lucide:triangle-alert' : 'lucide:file-check-2'" :size="17" />异常详情</h3>
          <pre v-if="hasText(currentRow.exceptionDetail)" class="job-log-pre job-log-exception">{{ currentRow.exceptionDetail }}</pre>
          <p v-else class="job-log-empty">{{ currentRow.isSuccess === false ? '本次执行失败，未记录异常详情。' : '本条日志未记录异常。' }}</p>
        </section>
        <div class="job-log-record-id">日志 ID：<span class="job-log-code">{{ currentRow.id }}</span></div>
      </div>
      <template #footer><el-button @click="jobLogDetailDrawer = false">关闭</el-button></template>
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import { useWindowSize } from '@vueuse/core'
  import { get, download } from '@/api/log/jobLog'
  import type { JobLog, JobLogQueryParams } from '@/api/log/types/jobLog.types'
  import { getAll, type Job } from '@/api/system/job'
  import AppIcon from '@/components/AppIcon.vue'
  import TablePage from '@/components/crud/TablePage.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useTableController } from '@/composables/useTableController'
  import { useExportAction } from '@/composables/useExportAction'

  defineOptions({ name: 'JobLog' })
  const { width: viewportWidth } = useWindowSize()
  const jobLogDetailDrawer = ref(false)
  const currentRow = ref<JobLog | null>(null)
  const allJobData = ref<Job[]>([])
  const queryDefaults = (): JobLogQueryParams => ({})
  const searchInfo = ref<JobLogQueryParams>(queryDefaults())
  const { data, loading, error: tableError, searchVisible: searchToggle, pagination, search, refresh, resetQuery, onSortChange, currentQuery } = useTableController({
    list: get, query: searchInfo, defaults: queryDefaults,
    defaultSort: { prop: 'creationTime', order: 'descending' }
  })
  const { exporting, run: runExport } = useExportAction({
    export: download, currentQuery: () => currentQuery(), fallbackFileName: '作业日志'
  })

  onMounted(async () => {
    try { allJobData.value = (await getAll()).data ?? [] }
    catch { allJobData.value = [] }
  })

  const showJobLogDetail = (row: JobLog): void => {
    currentRow.value = { ...row }
    jobLogDetailDrawer.value = true
  }
  const states = {
    success: { label: '成功', type: 'success', icon: 'lucide:circle-check' },
    failure: { label: '失败', type: 'danger', icon: 'lucide:circle-x' },
    unknown: { label: '未记录', type: 'info', icon: 'lucide:circle-help' }
  } as const
  const executionState = (success?: boolean) => success === true ? states.success : success === false ? states.failure : states.unknown
  const hasText = (value?: string | null): value is string => typeof value === 'string' && value.trim() !== ''
  const durationValue = (value?: string | number | null) => value === undefined || value === null || value === '' ? NaN : Number(value)
  const durationInMilliseconds = (value?: string | number | null): string => {
    const duration = durationValue(value)
    return Number.isFinite(duration) && duration >= 0 ? duration.toLocaleString('zh-CN') + ' ms' : '未记录'
  }
  const formatDuration = (value?: string | number | null): string => {
    const duration = durationValue(value)
    if (!Number.isFinite(duration) || duration < 0) return '未记录'
    return duration < 1000 ? duration + ' ms' : (duration / 1000).toLocaleString('zh-CN', { maximumFractionDigits: 2 }) + ' s'
  }
  const resultSummary = (row: JobLog): string => {
    if (hasText(row.exceptionDetail)) return row.exceptionDetail.trim().split(/\r?\n/)[0]!.slice(0, 200)
    return row.isSuccess === true ? '执行成功，未记录异常' : row.isSuccess === false ? '执行失败，未记录异常详情' : '未记录执行结果'
  }
  const formatParameters = (value: string): string => {
    try { JSON.parse(value) }
    catch { return value }
    // 只整理原始 JSON 的空白，避免数字参数在解析后丢失长整型精度。
    const tokens = value.match(/"(?:\\.|[^"\\])*"|[{}[\],:]|[^{}[\],:\s]+/g) ?? []
    let depth = 0
    let formatted = ''
    const newline = () => '\n' + '  '.repeat(depth)
    tokens.forEach((token, index) => {
      if (token === '{' || token === '[') {
        formatted += token
        depth++
        if (tokens[index + 1] !== '}' && tokens[index + 1] !== ']') formatted += newline()
      } else if (token === '}' || token === ']') {
        depth--
        if (tokens[index - 1] !== '{' && tokens[index - 1] !== '[') formatted += newline()
        formatted += token
      } else if (token === ',') formatted += ',' + newline()
      else if (token === ':') formatted += ': '
      else formatted += token
    })
    return formatted
  }

  const tableColumns = computed<TableColumnSpec<JobLog>[]>(() => [
    { key: 'jobName', label: '作业 / 分组', prop: 'jobName', sortable: 'custom', minWidth: 185 },
    { key: 'isSuccess', label: '执行结果', prop: 'isSuccess', width: 105 },
    { key: 'executionDuration', label: '执行耗时', prop: 'executionDuration', sortable: 'custom', width: 125 },
    { key: 'creationTime', label: '记录时间', prop: 'creationTime', sortable: 'custom', minWidth: 175 },
    { key: 'executionTarget', label: '执行目标', minWidth: 235 },
    { key: 'exceptionDetail', label: '结果摘要 / 参数', minWidth: 260 },
    { key: 'jobGroup', label: '作业分组', prop: 'jobGroup', sortable: 'custom', minWidth: 130, defaultVisible: false },
    { key: 'cron', label: 'Cron 表达式', prop: 'cron', minWidth: 170, defaultVisible: false },
    { key: 'runParams', label: '执行参数', prop: 'runParams', minWidth: 220, defaultVisible: false },
    { key: 'creatorName', label: '记录来源', prop: 'creatorName', minWidth: 120, defaultVisible: false },
    { key: 'id', label: '日志 ID', prop: 'id', minWidth: 180, defaultVisible: false },
    { key: 'actions', label: '操作', width: 90, fixed: viewportWidth.value < 768 ? undefined : 'right', placement: 'end' }
  ])
</script>

<style scoped>
  .job-log-name { font-weight: 600; color: var(--app-text-primary); overflow-wrap: anywhere; }
  .job-log-secondary { margin-top: 5px; color: var(--app-text-muted); font-size: 12px; line-height: 1.5; }
  .job-log-code { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 12px; }
  .job-log-truncate { display: block; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
  .job-log-state :deep(.el-tag__content) { display: inline-flex; align-items: center; gap: 5px; }
  .job-log-duration { color: var(--app-text-primary); font-weight: 600; font-variant-numeric: tabular-nums; white-space: nowrap; }
  .job-log-summary { display: -webkit-box; -webkit-box-orient: vertical; -webkit-line-clamp: 2; overflow: hidden; line-height: 1.6; overflow-wrap: anywhere; color: var(--app-text-secondary); }
  .job-log-summary.is-failure { color: var(--el-color-danger); }
  .job-log-drawer-title { color: var(--app-text-primary); font-size: 16px; font-weight: 600; }
  .job-log-drawer :deep(.el-drawer__header) { margin-bottom: 0; padding-bottom: 20px; border-bottom: 1px solid var(--app-border); }
  .job-log-detail { min-width: 0; }
  .job-log-overview { padding: 22px; background: var(--app-surface-subtle); border: 1px solid var(--app-border); border-left: 3px solid var(--app-border-strong); border-radius: 8px; }
  .job-log-overview.is-success { border-left-color: var(--el-color-success); }
  .job-log-overview.is-failure { border-left-color: var(--el-color-danger); }
  .job-log-overview-heading { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; }
  .job-log-overview-heading > div { min-width: 0; }
  .job-log-overview h2 { margin: 0; color: var(--app-text-primary); font-size: 20px; line-height: 1.5; font-weight: 600; overflow-wrap: anywhere; }
  .job-log-overview p { margin: 5px 0 0; color: var(--app-text-muted); font-size: 13px; }
  .job-log-overview-meta { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-top: 22px; }
  .job-log-overview-meta > div { display: flex; flex-direction: column; gap: 6px; }
  .job-log-overview-meta span { color: var(--app-text-muted); font-size: 12px; }
  .job-log-overview-meta strong { color: var(--app-text-primary); font-size: 15px; font-weight: 500; font-variant-numeric: tabular-nums; overflow-wrap: anywhere; }
  .job-log-section { padding: 25px 0; border-bottom: 1px solid var(--app-border); }
  .job-log-section h3 { display: flex; align-items: center; gap: 8px; margin: 0 0 16px; color: var(--app-text-primary); font-size: 14px; font-weight: 600; }
  .job-log-fields { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px 24px; margin: 0; }
  .job-log-fields > div { min-width: 0; }
  .job-log-fields .is-wide { grid-column: 1 / -1; }
  .job-log-fields dt { margin-bottom: 5px; color: var(--app-text-muted); font-size: 12px; }
  .job-log-fields dd { margin: 0; color: var(--app-text-primary); font-size: 13px; line-height: 1.7; overflow-wrap: anywhere; }
  .job-log-pre { margin: 0; padding: 16px; background: var(--app-surface-subtle); border: 1px solid var(--app-border); border-radius: 6px; color: var(--app-text-secondary); font: 12px/1.8 ui-monospace, SFMono-Regular, Consolas, monospace; white-space: pre-wrap; overflow-wrap: anywhere; tab-size: 2; }
  .job-log-error-section h3 { color: var(--el-color-danger); }
  .job-log-exception { border-left: 2px solid var(--el-color-danger); }
  .job-log-empty { margin: 0; color: var(--app-text-muted); font-size: 13px; line-height: 1.7; }
  .job-log-record-id { padding-top: 18px; color: var(--app-text-muted); font-size: 12px; overflow-wrap: anywhere; }
  @media (max-width: 767px) {
    .job-log-overview { padding: 16px; }
    .job-log-overview h2 { font-size: 18px; }
    .job-log-overview-meta { grid-template-columns: 1fr; gap: 14px; margin-top: 18px; }
    .job-log-fields { grid-template-columns: 1fr; }
    .job-log-section { padding: 20px 0; }
    .job-log-pre { padding: 12px; }
  }
</style>
