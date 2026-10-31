<template>
  <div class="table-managed-view">
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
        <SearchPanel
          :model="searchInfo"
          :loading="loading"
          @query="search"
          @reset="resetQuery"
        >
          <el-form-item label="作业名称">
            <el-select
              v-model="searchInfo.jobName"
              filterable
              clearable
              placeholder="请选择"
            >
              <el-option
                v-for="item in allTaskData"
                :key="item.id"
                :label="item.jobName"
                :value="Number(item.id)"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="作业结果">
            <el-select v-model="searchInfo.isSuccess" clearable placeholder="请选择">
              <el-option label="成功" :value="true" />
              <el-option label="失败" :value="false" />
            </el-select>
          </el-form-item>
          <template #advanced>
            <DateRangePicker v-model="searchInfo" />
          </template>
        </SearchPanel>
      </template>
      <template #toolbar-left>
        <el-button
          v-has-role="['admin']"
          type="warning"
          :icon="createIconifyIcon('lucide:download')"
          :loading="exporting"
          @click="runExport"
        >
          导出
        </el-button>
      </template>
      <template #column-isSuccess="scope">
        <el-tag :type="scope.row.isSuccess ? 'success' : 'danger'">
          {{ scope.row.isSuccess ? '成功' : '失败' }}
        </el-tag>
      </template>
      <template #column-executionDuration="scope">
        <el-tag v-if="scope.row.executionDuration <= 200" type="success">
          {{ scope.row.executionDuration }}ms
        </el-tag>
        <el-tag v-else-if="scope.row.executionDuration <= 500">
          {{ scope.row.executionDuration }}ms
        </el-tag>
        <el-tag v-else-if="scope.row.executionDuration <= 1000" type="warning">
          {{ scope.row.executionDuration }}ms
        </el-tag>
        <el-tag v-else type="danger">
          {{ scope.row.executionDuration }}ms
        </el-tag>
      </template>
      <template #column-actions="scope">
        <RowActions>
          <el-button
            :icon="createIconifyIcon('lucide:eye')"
            type="text"
            @click="showScheduledJobLogDetail(scope.row)"
          >
            详情
          </el-button>
        </RowActions>
      </template>
      <template #pagination>
        <el-pagination v-bind="pagination" />
      </template>
    </TablePage>
    <el-drawer
      v-model="scheduledJobLogDetailDrawer"
      direction="rtl"
      :size="appStore.drawerSize"
    >
      <template #header>
        <div class="flex justify-between items-center">
          <span class="text-lg">{{ currentRow.jobName }}</span>
        </div>
      </template>
      <div class="p-2">
        <div class="ape-volo-bg rounded-lg shadow-mdp-3">
          <div class="flex items-center mb-2">
            <div class="flex gap-2">
              <span
                class="w-3.5 h-3.5 rounded-full inline-block border border-[#e0443e] bg-[#ff5f56] shadow"
              ></span>
              <span
                class="w-3.5 h-3.5 rounded-full inline-block border border-[#dea123] bg-[#ffbd2e] shadow"
              ></span>
              <span
                class="w-3.5 h-3.5 rounded-full inline-block border border-[#13a10e] bg-[#27c93f] shadow"
              ></span>
            </div>
          </div>
          <div
            class="grid gap-y-2 gap-x-4"
            style="grid-template-columns: 88px 1fr"
          >
            <span class="text-emerald-500 min-w-[88px]">作业时间：</span
            ><span class="break-all">{{ currentRow.creationTime }}</span>
            <span class="text-emerald-500 min-w-[88px]">任务组：</span
            ><span class="break-all">{{ currentRow.jobGroup }}</span>
            <span class="text-emerald-500 min-w-[88px]">任务名称：</span
            ><span class="break-all">{{ currentRow.jobName }}</span>
            <span class="text-emerald-500 min-w-[88px]">作业耗时：</span
            ><span class="break-all">{{ currentRow.executionDuration }}</span>
            <span class="text-emerald-500 min-w-[88px]">程序集：</span
            ><span class="break-all">{{ currentRow.assemblyName }}</span>
            <span class="text-emerald-500 min-w-[88px]">执行类：</span
            ><span class="break-all">{{ currentRow.className }}</span>
            <span class="text-emerald-500 min-w-[88px]">Cron：</span
            ><span class="break-all">{{ currentRow.cron }}</span>
            <span class="text-emerald-500 min-w-[88px]">参数：</span
            ><span class="break-all">{{ currentRow.runParams }}</span>
            <span class="text-emerald-500 min-w-[88px]">异常信息：</span
            ><span class="break-all">{{ currentRow.exceptionDetail }}</span>
          </div>
        </div>
      </div>
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import { get, download } from '@/api/log/scheduledJobLog'
  import type {
    ScheduledJobLog,
    ScheduledJobLogQueryParams
  } from '@/api/log/types/scheduledJobLog.types'
  import { getAll, type ScheduledJob } from '@/api/operations/scheduledJobs'
  import TablePage from '@/components/crud/TablePage.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useTableController } from '@/composables/useTableController'
  import { useExportAction } from '@/composables/useExportAction'
  import { useAppStore } from '@/pinia'

  defineOptions({
    name: 'ScheduledJobLog'
  })

  const appStore = useAppStore()

  const scheduledJobLogDetailDrawer = ref(false)
  const currentRow = ref<ScheduledJobLog>({} as ScheduledJobLog)
  const allTaskData = ref<ScheduledJob[]>([])

  /**
   * 创建初始筛选条件。
   * @returns 与界面隔离的初始条件对象。
   */
  const queryDefaults = (): ScheduledJobLogQueryParams => ({})
  const searchInfo = ref<ScheduledJobLogQueryParams>(queryDefaults())

  const {
    data,
    loading,
    error: tableError,
    searchVisible: searchToggle,
    pagination,
    search,
    refresh,
    resetQuery,
    onSortChange,
    currentQuery
  } = useTableController({
    list: get,
    query: searchInfo,
    defaults: queryDefaults
  })

  const { exporting, run: runExport } = useExportAction({
    export: download,
    currentQuery: () => currentQuery(),
    fallbackFileName: '作业日志'
  })

  onMounted(async () => {
    try {
      const res = await getAll()
      allTaskData.value = res.data ?? []
    } catch {
      allTaskData.value = []
    }
  })

  /**
   * 打开作业日志详情抽屉。
   * @param row 当前行记录。
   */
  const showScheduledJobLogDetail = (row: ScheduledJobLog): void => {
    scheduledJobLogDetailDrawer.value = true
    currentRow.value = { ...row }
  }

  const tableColumns = computed<TableColumnSpec<ScheduledJobLog>[]>(() => [
    { key: 'creationTime', label: '作业时间', prop: 'creationTime', sortable: 'custom', minWidth: 170 },
    { key: 'jobGroup', label: '任务组', prop: 'jobGroup', sortable: 'custom', minWidth: 140 },
    { key: 'jobName', label: '任务名称', prop: 'jobName', sortable: 'custom', minWidth: 160 },
    { key: 'isSuccess', label: '执行结果', prop: 'isSuccess', width: 110 },
    { key: 'executionDuration', label: '请求耗时', prop: 'executionDuration', sortable: 'custom', width: 110 },
    { key: 'actions', label: '操作', width: 100, align: 'center' }
  ])
</script>
