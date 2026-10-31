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
      :preserve-expanded-content="true"
      @selection-change="onSelectionChange"
      @sort-change="onSortChange"
      @refresh="refresh"
    >
      <template #search>
        <SearchPanel
          :model="searchInfo"
          :loading="loading"
          @query="refresh"
          @reset="resetQuery"
        >
          <el-form-item label="任务名称">
            <el-input v-model="searchInfo.jobName" placeholder="请输入" />
          </el-form-item>
          <el-form-item label="分组">
            <el-input v-model="searchInfo.jobGroup" placeholder="请输入" />
          </el-form-item>
          <template #advanced>
            <el-form-item label="描述">
              <el-input v-model="searchInfo.description" placeholder="请输入" />
            </el-form-item>
            <el-form-item label="状态">
              <el-select
                v-model="searchInfo.enabled"
                clearable
                placeholder="请选择"
              >
                <el-option
                  v-for="item in statusTypeOption"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                />
              </el-select>
            </el-form-item>
            <DateRangePicker v-model="searchInfo" />
          </template>
        </SearchPanel>
      </template>
      <template #toolbar-left>
        <CrudToolbarActions
          :submitting="crud.submitting.value"
          :exporting="crud.exporting.value"
          :permissions="{
            create: perms.add,
            remove: perms.del,
            export: perms.download
          }"
          :selected-count="crud.multipleSelection.value.length"
          @create="crud.toAdd()"
          @remove="crud.doBatchDelete()"
          @export="crud.doExport()"
        />
      </template>
      <template #column-expand="scope">
        <div class="expand-detail">
          <p style="margin: 6px 0; padding: 2px 12px">
            分组: {{ scope.row.jobGroup }}
          </p>
          <p style="margin: 6px 0; padding: 2px 12px">
            程序集: {{ scope.row.assemblyName }}
          </p>
          <p style="margin: 6px 0; padding: 2px 12px">
            执行类: {{ scope.row.className }}
          </p>
          <p style="margin: 6px 0; padding: 2px 12px">
            执行间隔(秒): {{ scope.row.intervalSecond }}
          </p>
          <p style="margin: 6px 0; padding: 2px 12px">
            执行参数: {{ scope.row.runParams }}
          </p>
          <p style="margin: 6px 0; padding: 2px 12px">
            告警邮箱: {{ scope.row.alertEmail }}
          </p>
          <p style="margin: 6px 0; padding: 2px 12px">
            描述: {{ scope.row.description }}
          </p>
        </div>
      </template>
      <template #column-enabled="scope">
        <el-switch
          v-model="scope.row.enabled"
          inline-prompt
          :loading="loadingMap[scope.row.id]"
          :active-text="showDictLabel(statusTypeOption, 'true')"
          :inactive-text="showDictLabel(statusTypeOption, 'false')"
          @change="changeEnabled(scope.row, scope.row.enabled)"
        />
      </template>
      <template #column-actions="scope">
        <RowActions>
          <el-button
            v-show="scope.row.triggerState === 5"
            v-has-role="'admin'"
            :icon="createIconifyIcon('lucide:play')"
            type="primary"
            link
            @click="runningTask(scope.row.id)"
          >
            执行
          </el-button>
          <el-button
            v-show="scope.row.triggerState === 0"
            v-has-role="'admin'"
            :icon="createIconifyIcon('lucide:pause')"
            type="warning"
            link
            @click="pauseTask(scope.row)"
          >
            暂停
          </el-button>
          <el-button
            v-show="scope.row.triggerState === 1"
            v-has-role="'admin'"
            :icon="createIconifyIcon('lucide:play-circle')"
            type="success"
            link
            @click="resumeTask(scope.row.id)"
          >
            恢复
          </el-button>
          <el-button
            v-has-perm="perms.edit"
            :icon="createIconifyIcon('lucide:pencil')"
            type="primary"
            link
            @click="crud.toEdit(scope.row)"
          >
            编辑
          </el-button>
          <el-button
            v-has-perm="perms.del"
            :icon="createIconifyIcon('lucide:trash-2')"
            type="danger"
            link
            @click="crud.doDelete(scope.row)"
          >
            删除
          </el-button>
        </RowActions>
      </template>
      <template #pagination>
        <el-pagination v-bind="pagination" />
      </template>
    </TablePage>
    <!--表单渲染-->
    <formPanel
      :status-type-option="statusTypeOption"
      :task-trigger-type-option="taskTriggerTypeOption"
    />
  </div>
</template>

<script setup lang="ts">
  import type { ScheduledJob } from '@/api/operations/types/scheduledJob.types'

  import { computed, reactive, ref } from 'vue'
  import { ElMessage, ElMessageBox } from 'element-plus'
  import {
    del,
    edit,
    get,
    add,
    execution,
    pause,
    resume,
    download
  } from '@/api/operations/scheduledJobs'
  import type { ScheduledJobQueryParams } from '@/api/operations/types/scheduledJob.types'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useCrud } from '@/components/crud/UseCrud'
  import { getDict, showDictLabel, type DictOption } from '@/composables/useDictionary'
  import formPanel from './module/formPanel.vue'

  defineOptions({
    name: 'ScheduledJob'
  })

  const perms = {
    add: ['sys:scheduledJob:add'],
    edit: ['sys:scheduledJob:edit'],
    del: ['sys:scheduledJob:del'],
    download: ['sys:scheduledJob:download']
  }

  const searchInfo = ref<ScheduledJobQueryParams>({})

  // 状态
  const statusTypeOption = ref<DictOption[]>([])
  // 作业触发器类型
  const taskTriggerTypeOption = ref<DictOption[]>([])
  // 作业触发器状态
  const taskTriggerStateOption = ref<DictOption[]>([])

  // 触发器类型 formatter
  const triggerTypeFormatter = (_row: any, _column: any, cellValue: any) => {
    return showDictLabel(taskTriggerTypeOption.value, cellValue)
  }

  // 触发器状态 formatter
  const triggerStateFormatter = (_row: any, _column: any, cellValue: any) => {
    return showDictLabel(taskTriggerStateOption.value, cellValue)
  }

  const crud = useCrud({
    crudMethod: {
      list: get,
      del: del,
      add: add,
      edit: edit,
      download: download
    },
    defaultForm: () => ({
      id: '0',
      jobGroup: undefined,
      assemblyName: undefined,
      jobName: undefined,
      description: undefined,
      className: undefined,
      cron: undefined,
      principal: undefined,
      alertEmail: undefined,
      startTime: undefined,
      endTime: undefined,
      triggerType: 1,
      pauseAfterFailure: false,
      enabled: true,
      runParams: undefined,
      intervalSecond: undefined,
      cycleRunTimes: undefined
    }),
    searchInfo
  })
  const {
    data,
    searchToggle,
    loading,
    onSelectionChange,
    pagination,
    onSortChange,
    refresh,
    resetQuery
  } = crud

  const tableColumns = computed<TableColumnSpec<ScheduledJob>[]>(
    () => [
      { key: 'expand', label: '', type: 'expand', width: 50 },
      { key: 'selection', label: '', type: 'selection', width: 55 },
      {
        key: 'jobName',
        label: '任务名称',
        prop: 'jobName',
        sortable: 'custom',
        minWidth: 160
      },
      {
        key: 'principal',
        label: '任务负责人',
        prop: 'principal',
        minWidth: 130
      },
      {
        key: 'triggerType',
        label: '类型',
        prop: 'triggerType',
        sortable: 'custom',
        width: 120,
        formatter: triggerTypeFormatter
      },
      { key: 'corn', label: 'corn', prop: 'corn', minWidth: 160 },
      {
        key: 'enabled',
        label: 'DB状态',
        prop: 'enabled',
        sortable: 'custom',
        width: 110
      },
      {
        key: 'triggerState',
        label: 'RAM状态',
        prop: 'triggerState',
        width: 120,
        formatter: triggerStateFormatter
      },
      {
        key: 'startTime',
        label: '开始时间',
        prop: 'creationTime',
        sortable: 'custom',
        minWidth: 170
      },
      {
        key: 'endTime',
        label: '结束时间',
        prop: 'creationTime',
        sortable: 'custom',
        minWidth: 170
      },
      {
        key: 'creationTime',
        label: '创建时间',
        prop: 'creationTime',
        sortable: 'custom',
        minWidth: 170
      },
      { key: 'actions', label: '操作', width: 300 }
    ]
  )

  const init = async () => {
    statusTypeOption.value = await getDict('status_type')
    taskTriggerTypeOption.value = await getDict('task_trigger_type')
    taskTriggerStateOption.value = await getDict('task_trigger_state')
  }

  init()

  const loadingMap = reactive<Record<string | number, boolean>>({})
  const changeEnabled = async (row: any, val: boolean) => {
    loadingMap[row.id] = true
    ElMessageBox.confirm(
      `你要将${row.jobName}的状态切换为【${val ? '启用' : '禁用'}】吗？`,
      '切换状态',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
      .then(async () => {
        await edit(row).then(() => {
          ElMessage({
            type: 'success',
            message: '修改成功'
          })
        })
        loadingMap[row.id] = false
      })
      .catch(() => {
        row.enabled = !row.enabled
        loadingMap[row.id] = false
      })
  }

  const runningTask = async (id: string) => {
    execution(id)
      .then(() => {
        ElMessage({
          type: 'success',
          message: '运行成功'
        })
        refresh()
      })
      .catch(() => {})
  }

  const pauseTask = async (row: any) => {
    ElMessageBox.confirm(`你要将${row.jobName}的暂停吗？`, '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
      .then(async () => {
        pause(row.id).then(() => {
          ElMessage({
            type: 'success',
            message: '暂停成功'
          })
          refresh()
        })
      })
      .catch(() => {})
  }

  const resumeTask = async (id: string) => {
    resume(id)
      .then(() => {
        ElMessage({
          type: 'success',
          message: '恢复成功'
        })
        refresh()
      })
      .catch(() => {})
  }
</script>
<style>
  .expand-detail {
    padding: 8px 16px;
  }

  .expand-detail p {
    margin: 8px 0;
    padding: 2px 8px;
    border-radius: 4px;
  }
</style>
