<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="异常日志"
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
          <el-form-item label="操作人">
            <el-input v-model="searchInfo.controller" placeholder="请输入" />
          </el-form-item>
          <el-form-item label="请求方式">
            <el-select v-model="searchInfo.method" clearable placeholder="请选择">
              <el-option
                v-for="item in httpMethodOption"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
          <template #advanced>
            <el-form-item label="描述">
              <el-input v-model="searchInfo.description" placeholder="请输入" />
            </el-form-item>
            <DateRangePicker v-model="searchInfo" />
          </template>
        </SearchPanel>
      </template>
      <template #column-actions="scope">
        <RowActions>
          <el-button
            :icon="createIconifyIcon('lucide:eye')"
            type="text"
            @click="showExceptionLogDetail(scope.row)"
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
      v-model="exceptionDetailDrawer"
      direction="rtl"
      :size="appStore.drawerSize"
    >
      <template #header>
        <div class="flex justify-between items-center">
          <span class="text-lg">{{ currentRow.description }}</span>
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
            <span class="text-emerald-500 min-w-[88px]">操作人：</span
            ><span class="break-all">{{ currentRow.creatorName }}</span>
            <span class="text-emerald-500 min-w-[88px]">请求时间：</span
            ><span class="break-all">{{ currentRow.creationTime }}</span>
            <span class="text-emerald-500 min-w-[88px]">请求地址：</span
            ><span class="break-all">{{ currentRow.requestUrl }}</span>
            <span class="text-emerald-500 min-w-[88px]">请求方式：</span
            ><span class="break-all">{{ currentRow.method }}</span>
            <span class="text-emerald-500 min-w-[88px]">Ip地址：</span
            ><span class="break-all">{{ currentRow.requestIp }}</span>
            <span class="text-emerald-500 min-w-[88px]">Ip归属地：</span
            ><span class="break-all">{{ currentRow.ipAddress }}</span>
            <span class="text-emerald-500 min-w-[88px]">操作系统：</span
            ><span class="break-all">{{ currentRow.operatingSystem }}</span>
            <span class="text-emerald-500 min-w-[88px]">设备类型：</span
            ><span class="break-all">{{ currentRow.deviceType }}</span>
            <span class="text-emerald-500 min-w-[88px]">浏览器：</span
            ><span class="break-all">{{ currentRow.browserName }}</span>
            <span class="text-emerald-500 min-w-[88px]">版本号：</span
            ><span class="break-all">{{ currentRow.version }}</span>
            <span class="text-emerald-500 min-w-[88px]">代理信息：</span
            ><span class="break-all">{{ currentRow.userAgent }}</span>
            <span class="text-emerald-500 min-w-[88px]">请求参数：</span>
            <pre class="break-all rounded">{{
              formatJson(currentRow.requestParameters)
            }}</pre>
            <span class="text-emerald-500 min-w-[88px]">异常信息：</span>
            <span class="break-all rounded">{{
              formatJson(currentRow.exceptionMessage)
            }}</span>
            <span class="text-emerald-500 min-w-[88px]">完整信息：</span>
            <span class="break-all rounded">{{
              formatJson(currentRow.exceptionMessageFull)
            }}</span>
            <span class="text-emerald-500 min-w-[88px]">堆栈信息：</span>
            <span class="break-all rounded">{{
              formatJson(currentRow.exceptionStack)
            }}</span>
          </div>
        </div>
      </div>
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
  import { computed, ref } from 'vue'
  import { get } from '@/api/log/exceptionLog'
  import type {
    ExceptionLog,
    ExceptionLogQueryParams
  } from '@/api/log/types/exceptionLog.types'
  import TablePage from '@/components/crud/TablePage.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useDictionary } from '@/composables/useDictionary'
  import { useTableController } from '@/composables/useTableController'
  import { useAppStore } from '@/pinia'

  defineOptions({
    name: 'ExceptionLog'
  })

  const appStore = useAppStore()

  const exceptionDetailDrawer = ref(false)
  const currentRow = ref<ExceptionLog>({} as ExceptionLog)

  /**
   * 创建初始筛选条件，避免共享可变对象。
   * @returns 与当前界面隔离的初始筛选条件。
   */
  const queryDefaults = (): ExceptionLogQueryParams => ({})
  const searchInfo = ref<ExceptionLogQueryParams>(queryDefaults())
  const { options: httpMethodOption } = useDictionary('http_method')

  const {
    data,
    loading,
    error: tableError,
    searchVisible: searchToggle,
    pagination,
    search,
    refresh,
    resetQuery,
    onSortChange
  } = useTableController({
    list: get,
    query: searchInfo,
    defaults: queryDefaults
  })

  /**
   * 打开异常日志详情抽屉。
   * @param row 当前行记录。
   */
  const showExceptionLogDetail = (row: ExceptionLog): void => {
    exceptionDetailDrawer.value = true
    currentRow.value = { ...row }
  }

  /**
   * 将结构化内容转换为便于阅读的 JSON 文本。
   * @param value 待格式化内容。
   * @returns 缩进后的 JSON 文本；无内容时返回空文本。
   */
  const formatJson = (value: unknown): string => {
    if (!value) return ''
    try {
      const obj = typeof value === 'string' ? JSON.parse(value) : value
      return JSON.stringify(obj, null, 2)
    } catch {
      return String(value)
    }
  }

  const tableColumns = computed<TableColumnSpec<ExceptionLog>[]>(() => [
    { key: 'creatorName', label: '账号', prop: 'creatorName', sortable: 'custom', minWidth: 120 },
    { key: 'creationTime', label: '创建时间', prop: 'creationTime', sortable: 'custom', minWidth: 170 },
    { key: 'requestUrl', label: '请求地址', prop: 'requestUrl', minWidth: 220, showOverflowTooltip: true },
    { key: 'description', label: '描述', prop: 'description', sortable: 'custom', minWidth: 160, showOverflowTooltip: true },
    { key: 'method', label: '请求方式', prop: 'method', sortable: 'custom', width: 110 },
    { key: 'requestIp', label: 'Ip地址', prop: 'requestIp', sortable: 'custom', width: 140 },
    { key: 'ipAddress', label: '归属地', prop: 'ipAddress', sortable: 'custom', width: 140 },
    { key: 'actions', label: '操作', width: 100, align: 'center' }
  ])
</script>
