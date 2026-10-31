<template>
  <div class="table-managed-view">
    <TablePage
      title="在线用户"
      :data="data"
      :columns="tableColumns"
      :error="tableError"
      :loading="loading"
      row-key="accessToken"
      @refresh="refresh"
    >
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
      <template #column-accessToken="scope">
        <span class="break-all">{{ scope.row.accessToken }}</span>
      </template>
      <template #column-actions="scope">
        <RowActions>
          <el-button
            v-has-role="'admin'"
            class="dropout-btn"
            :icon="createIconifyIcon('lucide:log-out')"
            type="danger"
            link
            @click="dropOut(scope.row)"
          >
            强退
          </el-button>
        </RowActions>
      </template>
      <template #pagination>
        <el-pagination v-bind="pagination" />
      </template>
    </TablePage>
  </div>
</template>

<script setup lang="ts">
  import { computed, ref } from 'vue'
  import { ElMessage } from 'element-plus'
  import { get, del, download } from '@/api/onlineUser'
  import TablePage from '@/components/crud/TablePage.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useTableController } from '@/composables/useTableController'
  import type { PagedAndSortedResultRequestDto } from '@/api/types/base'
  import { useExportAction } from '@/composables/useExportAction'
  import { confirmAction } from '@/services/feedback/confirmation'

  defineOptions({
    name: 'OnlineUser'
  })

  interface OnlineUser {
    id: number | string
    account?: string
    accessToken?: string
    nickName?: string
    deptName?: string
    ip?: string
    address?: string
    operatingSystem?: string
    deviceType?: string
    browserName?: string
    version?: string
    loginTime?: string
  }

  /**
   * 创建初始筛选条件。
   * @returns 空查询条件。
   */
  const queryDefaults = (): PagedAndSortedResultRequestDto => ({})
  const searchInfo = ref<PagedAndSortedResultRequestDto>(queryDefaults())

  const {
    data,
    loading,
    error: tableError,
    pagination,
    refresh,
    currentQuery
  } = useTableController<PagedAndSortedResultRequestDto, OnlineUser>({
    list: get,
    query: searchInfo,
    defaults: queryDefaults
  })

  const { exporting, run: runExport } = useExportAction({
    export: download,
    currentQuery: () => currentQuery(),
    fallbackFileName: '在线用户'
  })

  /**
   * 强制指定用户下线。
   * @param row 当前行记录。
   */
  const dropOut = async (row: OnlineUser): Promise<void> => {
    const confirmed = await confirmAction(
      `确定要强退${row.account ?? ''}吗，是否继续？`,
      '提示'
    )
    if (!confirmed) return
    try {
      await del([String(row.accessToken ?? '')])
      ElMessage.success('登出成功!')
      await refresh()
    } catch {
      // 错误提示由 request 层统一处理
    }
  }

  const tableColumns = computed<TableColumnSpec<OnlineUser>[]>(() => [
    { key: 'account', label: '登录账号', prop: 'account', minWidth: 130 },
    { key: 'accessToken', label: '会话', prop: 'accessToken', minWidth: 220, showOverflowTooltip: true },
    { key: 'nickName', label: '昵称', prop: 'nickName', minWidth: 120 },
    { key: 'deptName', label: '部门', prop: 'deptName', minWidth: 130 },
    { key: 'ip', label: 'Ip地址', prop: 'ip', width: 140 },
    { key: 'address', label: 'Ip归属地', prop: 'address', width: 140 },
    { key: 'operatingSystem', label: '操作系统', prop: 'operatingSystem', width: 130 },
    { key: 'deviceType', label: '设备类型', prop: 'deviceType', width: 120 },
    { key: 'browserName', label: '浏览器', prop: 'browserName', width: 130 },
    { key: 'version', label: '版本号', prop: 'version', width: 110 },
    { key: 'loginTime', label: '登录时间', prop: 'loginTime', minWidth: 170 },
    { key: 'actions', label: '操作', width: 100, align: 'center' }
  ])
</script>

<style scoped>
  /* link 模式下强制文字/图标走 danger 红色 */
  .dropout-btn,
  .dropout-btn :deep(.el-icon),
  .dropout-btn :deep(.el-button__icon) {
    color: var(--el-color-danger) !important;
  }
</style>
