<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="作业调度"
      :data="data"
      :columns="tableColumns"
      :loading="loading"
      :error="crud.error.value"
      row-key="id"
      @selection-change="onSelectionChange"
      @sort-change="onSortChange"
      @refresh="refresh"
    >
      <template #intro>
        <div class="job-page-intro">
          <AppIcon icon="lucide:calendar-clock" :size="18" />
          <p>作业按计划触发；启动调度不会立即执行一次任务。暂停保留计划，停止会停用配置并移除调度。</p>
        </div>
      </template>
      <template #search>
        <SearchPanel :model="searchInfo" :loading="loading" @query="refresh" @reset="resetQuery">
          <el-form-item label="作业名称">
            <el-input v-model="searchInfo.jobName" clearable placeholder="搜索作业名称" />
          </el-form-item>
          <el-form-item label="分组">
            <el-input v-model="searchInfo.jobGroup" clearable placeholder="搜索分组" />
          </el-form-item>
          <template #advanced>
            <el-form-item label="启用配置">
              <el-select v-model="searchInfo.enabled" clearable placeholder="全部">
                <el-option label="已启用" :value="true" />
                <el-option label="已停用" :value="false" />
              </el-select>
            </el-form-item>
            <el-form-item label="描述">
              <el-input v-model="searchInfo.description" clearable />
            </el-form-item>
            <DateRangePicker v-model="searchInfo" />
          </template>
        </SearchPanel>
      </template>
      <template #toolbar-left>
        <CrudToolbarActions
          :submitting="crud.submitting.value || mutation.pending.value"
          :exporting="crud.exporting.value"
          :permissions="{ create: perms.add, remove: perms.del, export: perms.download }"
          :selected-count="crud.multipleSelection.value.length"
          @create="crud.toAdd()"
          @remove="crud.doBatchDelete()"
          @export="crud.doExport()"
        />
      </template>
      <template #column-jobName="{ row }">
        <div class="job-name">{{ row.jobName }}</div>
        <div class="job-secondary">{{ row.jobGroup }}</div>
      </template>
      <template #column-schedule="{ row }">
        <div class="job-rule">
          <el-tag size="small" type="info" effect="plain">{{ getJobSchedule(row).mode }}</el-tag>
          <span class="job-rule-text">{{ getJobSchedule(row).rule }}</span>
        </div>
        <div class="job-secondary">{{ getJobSchedule(row).repeat }}</div>
      </template>
      <template #column-enabled="{ row }">
        <el-tooltip content="启用配置决定服务重启后是否自动加载此作业" placement="top">
          <el-switch
            v-if="canEdit"
            :model-value="row.enabled"
            :loading="isActive(row, ['toggle', 'stop', 'restart'])"
            :disabled="mutation.pending.value || crud.submitting.value"
            inline-prompt
            active-text="启用"
            inactive-text="停用"
            @change="runEnabledChange(row, $event)"
          />
          <el-tag v-else :type="row.enabled ? 'success' : 'info'" effect="plain">
            {{ row.enabled ? '已启用' : '已停用' }}
          </el-tag>
        </el-tooltip>
      </template>
      <template #column-triggerState="{ row }">
        <el-tooltip :content="getJobState(row.triggerState).description" placement="top">
          <el-tag :type="getJobState(row.triggerState).type" effect="light" class="job-state">
            <AppIcon :icon="getJobState(row.triggerState).icon" />
            {{ getJobState(row.triggerState).label }}
          </el-tag>
        </el-tooltip>
        <div v-if="getJobWarning(row)" class="job-state-warning">{{ getJobWarning(row) }}</div>
      </template>
      <template #column-expand="{ row }">
        <div v-if="getJobDetails(row).length" class="job-details">
          <section v-for="group in getJobDetails(row)" :key="group.title">
            <h3>{{ group.title }}</h3>
            <dl>
              <div v-for="field in group.fields" :key="field.label">
                <dt>{{ field.label }}</dt>
                <dd>{{ field.value }}</dd>
              </div>
            </dl>
          </section>
        </div>
        <p v-else class="job-details-empty">暂无补充信息</p>
      </template>
      <template #column-actions="{ row }">
        <RowActions>
          <el-button
            v-for="action in rowActions(row)"
            :key="action"
            :icon="createIconifyIcon(actionInfo[action].icon)"
            :type="actionInfo[action].type"
            :loading="isActive(row, [action])"
            :disabled="mutation.pending.value || crud.submitting.value"
            link
            @click="runAction(row, action)"
          >
            {{ actionInfo[action].label }}
          </el-button>
          <el-button v-has-perm="perms.edit" :icon="createIconifyIcon('lucide:pencil')" type="primary" link :disabled="mutation.pending.value" @click="crud.toEdit(row)">
            编辑
          </el-button>
          <el-button v-has-perm="perms.del" :icon="createIconifyIcon('lucide:trash-2')" type="danger" link :disabled="mutation.pending.value" @click="crud.doDelete(row)">
            删除
          </el-button>
        </RowActions>
      </template>
      <template #pagination>
        <el-pagination
          v-bind="pagination"
          :layout="viewportWidth < 768 ? 'prev, pager, next, total' : 'total, sizes, prev, pager, next, jumper'"
          :pager-count="5"
        />
      </template>
    </TablePage>
    <formPanel />
  </div>
</template>

<script setup lang="ts">
  import { computed, ref } from 'vue'
  import { ElMessage } from 'element-plus'
  import { useWindowSize } from '@vueuse/core'
  import { add, del, edit, get, download, execution, pause, resume, stop, restart } from '@/api/system/job'
  import type { Job, JobQueryParams } from '@/api/system/types/job.types'
  import { useUserStore } from '@/pinia/modules/user'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useCrud } from '@/components/crud/UseCrud'
  import { useMutationAction } from '@/composables/useMutationAction'
  import { confirmAction } from '@/services/feedback/confirmation'
  import { getJobActions, getJobDetails, getJobSchedule, getJobState, getJobWarning, type JobAction } from './jobState'
  import formPanel from './module/formPanel.vue'

  defineOptions({ name: 'Job' })
  const userStore = useUserStore()
  const { width: viewportWidth } = useWindowSize()
  const canEdit = computed(() => userStore.userInfo.authCodes.includes('sys:job:edit'))
  const canControl = computed(() => userStore.userInfo.roleCodes.includes('admin'))
  const canDelete = computed(() => userStore.userInfo.authCodes.includes('sys:job:del'))
  const perms = { add: ['sys:job:add'], edit: ['sys:job:edit'], del: ['sys:job:del'], download: ['sys:job:download'] }
  const searchInfo = ref<JobQueryParams>({})
  const crud = useCrud({
    crudMethod: { list: get, del, add, edit, download },
    defaultForm: () => ({
      id: '0', jobName: '', jobGroup: '', assemblyName: '', className: '',
      description: '', principal: '', alertEmail: '', runParams: '',
      startTime: undefined, endTime: undefined, cron: '',
      triggerType: 1, intervalSecond: 60, cycleRunTimes: 0, pauseAfterFailure: false, enabled: false
    }),
    searchInfo
  })
  const { data, searchToggle, loading, onSelectionChange, pagination, onSortChange, refresh, resetQuery } = crud
  const tableColumns = computed<TableColumnSpec<Job>[]>(() => {
    const columns: TableColumnSpec<Job>[] = [
      { key: 'expand', label: '', type: 'expand', width: 44 },
      { key: 'jobName', label: '作业 / 分组', prop: 'jobName', sortable: 'custom', minWidth: 190 },
      { key: 'schedule', label: '调度规则', minWidth: 250 },
      { key: 'enabled', label: '启用配置', prop: 'enabled', sortable: 'custom', width: 110 },
      { key: 'triggerState', label: '调度状态', prop: 'triggerState', minWidth: 190 },
      { key: 'creationTime', label: '创建时间', prop: 'creationTime', sortable: 'custom', minWidth: 170, defaultVisible: false }
    ]
    if (canDelete.value) columns.splice(1, 0, { key: 'selection', label: '', type: 'selection', width: 48 })
    if (canEdit.value || canControl.value || canDelete.value) columns.push({ key: 'actions', label: '操作', width: 330, fixed: viewportWidth.value < 768 ? undefined : 'right', placement: 'end' })
    return columns
  })
  const actionInfo = {
    start: { label: '启动', icon: 'lucide:play', type: 'primary', success: '调度已启动' },
    pause: { label: '暂停', icon: 'lucide:pause', type: 'warning', success: '调度已暂停' },
    resume: { label: '恢复', icon: 'lucide:circle-play', type: 'success', success: '调度已恢复' },
    restart: { label: '重新调度', icon: 'lucide:rotate-cw', type: 'primary', success: '已按当前配置重新调度' },
    stop: { label: '停止', icon: 'lucide:square', type: 'danger', success: '调度已停止，配置已停用' }
  } as const
  const rowActions = (row: Job) => getJobActions(row).filter(action =>
    ['restart', 'stop'].includes(action) ? canEdit.value : canControl.value)
  type Mutation = { row: Job; action: JobAction | 'toggle'; enabled?: boolean }
  const activeMutation = ref<{ id: string; action: Mutation['action'] } | null>(null)
  const isActive = (row: Job, actions: Mutation['action'][]) => {
    const active = activeMutation.value
    return Boolean(active && active.id === row.id && actions.includes(active.action))
  }
  const mutation = useMutationAction<Mutation>(async ({ row, action, enabled }) => {
    if (action === 'toggle' ? !canEdit.value : !rowActions(row).includes(action)) return false
    activeMutation.value = { id: row.id, action }
    let attempted = false
    try {
      const stopping = action === 'stop' || (action === 'toggle' && !enabled)
      const label = action === 'toggle' ? (enabled ? '启用并调度' : '停用并停止调度') : actionInfo[action].label
      const message = stopping
        ? `停止“${row.jobName}”后不再触发，当前已开始的执行会继续完成。确定停止吗？`
        : action === 'pause' ? `暂停“${row.jobName}”的后续触发？当前已开始的执行不会中断。`
          : `确定${label}“${row.jobName}”？作业会按配置的计划触发。`
      if (!await confirmAction(message, label)) return false
      attempted = true
      if (action === 'start') await execution(row.id)
      else if (action === 'pause') await pause(row.id)
      else if (action === 'resume') await resume(row.id)
      else if (action === 'stop') await stop(row.id)
      else if (action === 'restart') await restart(row.id)
      else await edit({ ...row, enabled })
      ElMessage.success(action === 'toggle' ? '启用配置已更新' : actionInfo[action].success)
      return true
    } finally {
      try { if (attempted) await refresh() }
      finally { activeMutation.value = null }
    }
  })
  const runAction = (row: Job, action: Mutation['action'], enabled?: boolean) => mutation.run({ row, action, enabled })
  const runEnabledChange = (row: Job, value: string | number | boolean) => runAction(row, 'toggle', value === true)
</script>

<style scoped>
  .job-page-intro { display: flex; align-items: flex-start; gap: 10px; color: var(--el-text-color-secondary); font-size: 13px; line-height: 1.6; }
  .job-page-intro > .app-icon { margin-top: 2px; }
  .job-page-intro p { margin: 0; }
  .job-name { font-weight: 600; color: var(--el-text-color-primary); }
  .job-secondary { margin-top: 5px; font-size: 12px; color: var(--el-text-color-secondary); }
  .job-rule { display: flex; align-items: center; gap: 8px; }
  .job-rule-text { overflow-wrap: anywhere; font-family: ui-monospace, monospace; font-size: 12px; }
  .job-state :deep(.el-tag__content) { display: inline-flex; align-items: center; gap: 5px; }
  .job-state-warning { margin-top: 5px; color: var(--el-color-warning-dark-2); font-size: 12px; line-height: 1.5; }
  .job-details { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 28px; padding: 20px 28px; background: var(--el-fill-color-lighter); }
  .job-details h3 { margin: 0 0 12px; font-size: 13px; color: var(--el-text-color-primary); }
  .job-details dl { margin: 0; }
  .job-details dl > div { display: grid; grid-template-columns: 88px minmax(0, 1fr); gap: 12px; padding: 5px 0; line-height: 1.6; }
  .job-details dt { color: var(--el-text-color-secondary); }
  .job-details dd { margin: 0; white-space: pre-wrap; overflow-wrap: anywhere; max-height: 140px; overflow: auto; }
  .job-details-empty { padding: 16px 28px; color: var(--el-text-color-secondary); }
  @media (max-width: 767px) { .job-details { grid-template-columns: minmax(0, 1fr); gap: 20px; padding: 16px; max-width: calc(100vw - 72px); } }
</style>
