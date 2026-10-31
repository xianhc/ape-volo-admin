<template>
  <section v-loading="blocking" class="table-page" :aria-busy="loading">
    <div v-if="$slots.intro" class="table-page-intro">
      <slot name="intro" />
    </div>
    <div v-if="$slots.context" class="table-page-context">
      <slot name="context" />
    </div>
    <el-alert
      v-if="error"
      :title="errorMessage"
      type="error"
      show-icon
      :closable="false"
      class="table-page-notice"
    >
      <el-button link :disabled="loading" @click="requestRefresh">{{ '刷新' }}</el-button>
    </el-alert>
    <div
      v-if="$slots.search && hasOpenedSearch"
      v-show="searchVisible"
      :id="searchId"
      class="table-page-search"
    >
      <slot name="search" />
    </div>
    <div class="ape-volo-table table-page-card">
      <div class="table-page-toolbar">
        <div class="table-page-toolbar__left">
          <span v-if="title" class="table-page-title">{{ title }}</span>
        </div>
        <div class="table-page-toolbar__tools">
          <slot name="toolbar-left" />
          <el-button
            v-if="$slots.search"
            :icon="createIconifyIcon('lucide:search')"
            :type="searchVisible ? 'primary' : undefined"
            circle
            :title="'查询'"
            :aria-label="'查询'"
            :aria-expanded="searchVisible"
            :aria-controls="searchId"
            @click="searchVisible = !searchVisible"
          />
          <el-button
            :icon="createIconifyIcon('lucide:rotate-cw')"
            :loading="refreshing"
            :disabled="loading || refreshDisabled"
            circle
            :title="'刷新'"
            :aria-label="'刷新'"
            @click="requestRefresh"
          />
          <TableColumnSettings
            v-model="preferences"
            :columns="columns"
            @reset="reset"
          />
        </div>
      </div>
      <div v-if="$slots.notice" class="table-page-notice">
        <slot name="notice" />
      </div>
      <div ref="body" class="table-page-body">
        <el-table
          ref="tableRef"
          v-bind="$attrs"
          :data="data"
          :height="tableHeight"
          scrollbar-always-on
          class="table-page-table"
        >
          <el-table-column
            v-for="column in visibleColumns"
            :key="column.key"
            v-bind="bindings(column)"
          >
            <template v-if="$slots[`column-${column.key}`]" #default="scope">
              <!-- 子列探测使用空行和 -1 索引，只向业务插槽传递真实数据行。 -->
              <slot
                v-if="scope.$index >= 0"
                :name="`column-${column.key}`"
                v-bind="scope"
              />
            </template>
          </el-table-column>
        </el-table>
      </div>
      <div v-if="$slots.pagination" class="table-page-pagination">
        <slot name="pagination" />
      </div>
    </div>
  </section>
</template>
<script setup lang="ts" generic="T extends object">
  import { createIconifyIcon } from '@/components/icons'
  import { apiErrorBody } from '@/utils/apiError'
  import {
    computed,
    ref,
    provide,
    watch,
    nextTick,
    onMounted,
    onBeforeUnmount,
    onActivated,
    onDeactivated,
    useId
  } from 'vue'
  import type { TableInstance } from 'element-plus'
  import TableColumnSettings from './TableColumnSettings.vue'
  import { useTableColumns } from './useTableColumns'
  import { rowActionMeasurementsKey, tableLoadingKey } from './tableContext'
  import type { TableColumnSpec } from './tableColumns'
  defineOptions({ inheritAttrs: false })
  const props = withDefaults(
    defineProps<{
      /** 当前内容或界面的展示标题。 */
      title?: string
      /** 当前记录或响应内容。 */
      data: T[]
      /** 表格展示使用的业务列定义集合。 */
      columns: readonly TableColumnSpec<T>[]
      /** 是否正在加载数据。 */
      loading?: boolean
      /** 当前操作保留的错误，供界面反馈或重试。 */
      error?: unknown
      /** 当前是否禁止刷新。 */
      refreshDisabled?: boolean
    }>(),
    {
      title: undefined,
      loading: false,
      error: undefined,
      refreshDisabled: false
    }
  )
  /**
   * 本次加载是否由工具栏的刷新按钮发起。
   *
   * 刷新按钮自己会转圈，此时保留当前数据、不再盖遮罩；
   * 其余来源（首屏、翻页、改每页条数、排序、查询）一律用表格遮罩反馈，
   * 否则点分页器时页面上看不到任何加载状态。
   */
  const refreshedByButton = ref(false)
  watch(
    () => props.loading,
    (loading) => {
      if (!loading) refreshedByButton.value = false
    }
  )
  /** 需要占住内容区的加载：首屏、翻页、改每页条数、排序、查询。 */
  const blocking = computed(() => props.loading && !refreshedByButton.value)
  /** 工具栏刷新按钮发起的加载：只让按钮自己转圈。 */
  const refreshing = computed(() => props.loading && refreshedByButton.value)
  provide(tableLoadingKey, { blocking, refreshing })
  const errorMessage = computed(() => {
    const body = apiErrorBody(props.error) as { message?: string } | undefined
    return body?.message || '数据加载失败，请稍后重试'
  })
  const emit = defineEmits<{
    /** 重新加载当前数据的操作。 */
    refresh: []
  }>()
  let refreshRequested = false
  /**
   * 重新加载表格当前上下文的数据。
   */
  async function requestRefresh() {
    // loading 的 prop 下一次渲染才更新，先挡住同一帧的重复点击。
    if (refreshRequested || props.loading || props.refreshDisabled) return
    refreshRequested = true
    refreshedByButton.value = true
    emit('refresh')
    await nextTick()
    refreshRequested = false
    // 父级没真正发起请求时（被禁用或上下文不允许），把标记收回来。
    if (!props.loading) refreshedByButton.value = false
  }
  const searchVisible = defineModel<boolean>('searchVisible', {
    default: true
  })
  /**
   * 第一次展开后保留日期等有内部状态的输入，收起只改变可见性。
   */
  const hasOpenedSearch = ref(searchVisible.value)
  watch(searchVisible, (open) => {
    if (open) hasOpenedSearch.value = true
  })
  const searchId = `table-search-${useId()}`
  const { preferences, visibleColumns, reset } = useTableColumns(
    () => props.columns
  )
  const body = ref<HTMLElement>()
  const tableRef = ref<TableInstance>()
  const tableHeight = ref<number | string>('100%')
  const bodyWidth = ref(0)
  const actionWidths = ref(new Map<symbol, number>())
  const actionWidth = computed(() =>
    Math.max(80, ...actionWidths.value.values())
  )
  provide(rowActionMeasurementsKey, {
    /**
     * 取得或应用当前上报行操作区域的测量宽度。
     * @param id 目标业务记录的标识。
     * @param width 当前组件对应的展示尺寸。
     */
    report: (id, width) => {
      if (actionWidths.value.get(id) !== width)
        actionWidths.value.set(id, width)
    },
    /**
     * 移除表格选中的目标记录。
     * @param id 目标业务记录的标识。
     */
    remove: (id) => {
      actionWidths.value.delete(id)
    }
  })
  /**
   * 组合当前表格或表单控件需要的属性与事件绑定。
   * @param column 当前表格列或数据库列定义。
   * @returns 按列角色整理后的表格原生属性与展示配置。
   */
  function bindings(column: TableColumnSpec<T>) {
    const { key, placement } = column
    const native = {
      label: column.label,
      prop: column.prop,
      type: column.type,
      width: column.width,
      minWidth: column.minWidth,
      align: column.align,
      sortable: column.sortable,
      formatter: column.formatter,
      selectable: column.selectable,
      fixed: column.fixed,
      showOverflowTooltip: column.showOverflowTooltip
    }
    if (placement === 'end') {
      const width = Math.max(
        actionWidth.value,
        Number(column.width) || Number(column.minWidth) || 80
      )
      return {
        ...native,
        columnKey: key,
        width,
        fixed: bodyWidth.value - width >= 200 ? 'right' : false,
        showOverflowTooltip: false,
        className: 'table-page-actions-cell'
      }
    }
    return {
      ...native,
      minWidth:
        column.type || column.width
          ? column.minWidth
          : Math.max(
            column.prop?.endsWith('Time') ? 180 : 100,
            Number(column.minWidth) || 120
          ),
      columnKey: key
    }
  }
  let observer: ResizeObserver | undefined
  let active = true
  /**
   * 按容器测量结果更新当前展示尺寸。
   */
  function updateSize() {
    if (!active || !body.value || !body.value.getClientRects().length) return
    const bounds = body.value.getBoundingClientRect()
    tableHeight.value = Math.max(80, Math.floor(bounds.height))
    bodyWidth.value = bounds.width
    void nextTick(() => {
      if (active) tableRef.value?.doLayout()
    })
  }
  /**
   * 注册表格所需的状态变化监听。
   */
  function observe() {
    observer?.disconnect()
    if (body.value) {
      observer = new ResizeObserver(updateSize)
      observer.observe(body.value)
      updateSize()
    }
  }
  watch(
    () => visibleColumns.value.map((c) => c.key).join('|'),
    async (next, previous) => {
      const selectionKeys = props.columns
        .filter((c) => c.type === 'selection')
        .map((c) => c.key)
      if (
        selectionKeys.some(
          (k) =>
            previous?.split('|').includes(k) && !next.split('|').includes(k)
        )
      )
        tableRef.value?.clearSelection()
      await nextTick()
      if (active) tableRef.value?.doLayout()
    }
  )
  watch(actionWidth, async () => {
    await nextTick()
    if (active) tableRef.value?.doLayout()
  })
  onMounted(async () => {
    await nextTick()
    observe()
  })
  onActivated(async () => {
    active = true
    await nextTick()
    observe()
  })
  onDeactivated(() => {
    active = false
    observer?.disconnect()
  })
  onBeforeUnmount(() => {
    active = false
    observer?.disconnect()
  })
</script>
<style scoped>
  .table-page {
    display: flex;
    flex-direction: column;
    height: 100%;
    min-height: 0;
    min-width: 0;
    gap: 12px;
    padding: 8px 0;
    box-sizing: border-box;
  }
  .table-page-intro,
  .table-page-context {
    flex: none;
  }
  .table-page-search {
    flex: none;
    max-height: 35%;
    padding: var(--app-space-4);
    overflow: auto;
    border-radius: var(--app-radius-surface);
    background: var(--app-surface);
  }
  .table-page-card {
    display: flex;
    flex-direction: column;
    flex: 1;
    min-height: 0;
    min-width: 0;
    margin: 0;
  }
  .table-page-toolbar {
    display: flex;
    flex: none;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 8px;
    margin-bottom: 12px;
  }
  .table-page-toolbar__left {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    min-width: 0;
    gap: 12px;
  }
  .table-page-title {
    font-weight: 500;
    font-size: 15px;
    white-space: nowrap;
  }
  .table-page-toolbar__tools {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;
  }
  .table-page-toolbar__tools :deep(.el-button + .el-button) {
    margin-left: 0;
  }
  .table-page-toolbar__tools :deep(.el-button.is-circle) {
    border-radius: 50% !important;
  }
  .table-page-toolbar__tools :deep(.ape-volo-btn-list) {
    margin-bottom: 0;
  }
  .table-page-notice {
    flex: none;
  }
  .table-page-body {
    flex: 1;
    min-height: 80px;
    min-width: 0;
    overflow: hidden;
  }
  .table-page-pagination {
    flex: none;
    margin-top: 12px;
    display: flex;
    padding: 2px 0;
    overflow-x: auto;
  }
  .table-page-pagination :deep(.el-pagination) {
    flex: none;
    margin-top: 0;
    margin-left: auto;
  }
  .table-page-table :deep(th .cell) {
    white-space: nowrap;
  }
  .table-page-table :deep(.table-page-actions-cell .cell) {
    white-space: nowrap;
    overflow: visible;
  }
  @media (max-width: 640px) {
    .table-page {
      height: auto;
      min-height: 100%;
    }
    .table-page-search {
      max-height: none;
      overflow: visible;
    }
    .table-page-card {
      flex: none;
      height: max(360px, 60dvh);
      padding: 10px;
    }
    .table-page-toolbar__tools {
      gap: 6px;
    }
    .table-page-toolbar__tools :deep(.el-button) {
      padding: 8px;
    }
  }
</style>
