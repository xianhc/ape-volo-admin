<!-- 双栏使用 flex 面板，不再额外套 el-card（卡片样式由 TablePage 自带）。 -->

<template>
  <div ref="workspaceRef" class="table-managed-view dictionary-workspace">
    <!-- 窄屏下两个面板互斥显示，避免双栏互相挤压 -->
    <el-radio-group
      v-if="compact"
      v-model="compactPanel"
      class="dictionary-navigation"
    >
      <el-radio-button value="dictionaries">字典列表</el-radio-button>
      <el-radio-button value="items" :disabled="!selectedDictionaryId">
        字典项
      </el-radio-button>
    </el-radio-group>

    <el-container class="dictionary-layout" :class="{ 'is-compact': compact }">
      <!-- 左侧字典列表 -->
      <el-aside
        v-show="!compact || compactPanel === 'dictionaries'"
        class="dictionary-panel dictionary-master"
        width="auto"
      >
        <TablePage
          v-model:search-visible="searchToggle"
          title="字典管理"
          :data="data"
          :columns="tableColumns"
          :loading="loading"
      :error="crud.error.value"
          row-key="id"
          highlight-current-row
          :current-row-key="selectedDictionaryId"
          style="width: 100%"
          @selection-change="onSelectionChange"
          @sort-change="onSortChange"
          @row-click="handleDictionaryRowClick"
          @refresh="refresh"
        >
          <template #search>
            <SearchPanel
              :model="searchInfo"
              :loading="loading"
              @query="refresh"
              @reset="resetQuery"
            >
              <el-form-item label="名称">
                <el-input v-model="searchInfo.name" placeholder="请输入" />
              </el-form-item>
              <el-form-item label="描述">
                <el-input
                  v-model="searchInfo.description"
                  placeholder="请输入"
                />
              </el-form-item>
              <template #advanced>
                <el-form-item label="字典类型">
                  <el-select
                    v-model="searchInfo.dictType"
                    clearable
                    placeholder="请选择"
                  >
                    <el-option
                      v-for="item in dictTypeOption"
                      :key="item.value"
                      :label="item.label"
                      :value="item.value"
                    />
                  </el-select>
                </el-form-item>
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
          <template #column-name="scope">
            <button
              class="dictionary-choice"
              type="button"
              :aria-pressed="scope.row.id === selectedDictionaryId"
              @click.stop="selectDictionary(scope.row)"
            >
              <span class="dictionary-choice__name">{{ scope.row.name }}</span>
              <span
                v-if="scope.row.description"
                class="dictionary-choice__description"
              >
                {{ scope.row.description }}
              </span>
            </button>
          </template>
          <template #column-actions="scope">
            <div @click.stop>
              <RowActions>
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
            </div>
          </template>
          <template #pagination>
            <el-pagination v-bind="pagination" />
          </template>
        </TablePage>
      </el-aside>

      <!-- 右侧字典详情 -->
      <el-main
        v-show="!compact || compactPanel === 'items'"
        class="dictionary-panel dictionary-items"
      >
        <detailPanel
          :selected-dictionary-id="selectedDictionaryId"
          :selected-dictionary-name="selectedDictionaryName"
        />
      </el-main>
    </el-container>

    <!--表单渲染-->
    <dictionaryFormPanel :dict-type-option="dictTypeOption" />
  </div>
</template>

<script setup lang="ts">
  import type { Dictionary } from '@/api/system/types/dictionary.types'

  import { computed, ref } from 'vue'
  import { useElementSize } from '@vueuse/core'
  import { getDict, showDictLabel, type DictOption } from '@/composables/useDictionary'
  import { get, del, add, edit, download } from '@/api/system/dictionary'
  import type { DictionaryQueryParams } from '@/api/system/types/dictionary.types'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useCrud } from '@/components/crud/UseCrud'
  import detailPanel from './detail.vue'
  import dictionaryFormPanel from './module/dictionaryFormPanel.vue'

  defineOptions({
    name: 'Dictionary'
  })

  const perms = {
    add: ['sys:dict:add'],
    edit: ['sys:dict:edit'],
    del: ['sys:dict:del'],
    download: ['sys:dict:download']
  }

  const selectedDictionaryId = ref('')
  const selectedDictionaryName = ref('')

  const searchInfo = ref<DictionaryQueryParams>({})

  const dictTypeOption = ref<DictOption[]>([])

  // 工作区宽度：窄屏时左右面板互斥显示
  const workspaceRef = ref<HTMLElement>()
  const { width: workspaceWidth } = useElementSize(workspaceRef)
  const compact = computed(
    () => workspaceWidth.value > 0 && workspaceWidth.value < 1040
  )
  const compactPanel = ref<'dictionaries' | 'items'>('dictionaries')

  // 字典类型 formatter
  const dictTypeFormatter = (_row: any, _column: any, cellValue: any) => {
    return showDictLabel(dictTypeOption.value, cellValue)
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
      dictType: 2,
      name: '',
      description: ''
    }),
    searchInfo
  })
  const {
    data,
    searchToggle,
    loading,
    refresh,
    resetQuery,
    onSelectionChange,
    pagination,
    onSortChange
  } = crud

  const tableColumns = computed<TableColumnSpec<Dictionary>[]>(
    () => [
      {
        key: 'selection',
        label: '',
        type: 'selection',
        width: 44,
        align: 'left',
        placement: 'start'
      },
      {
        key: 'dictType',
        label: '字典类型',
        prop: 'dictType',
        sortable: 'custom',
        minWidth: 120,
        align: 'left',
        formatter: dictTypeFormatter,
        defaultVisible: false
      },
      {
        key: 'name',
        label: '名称',
        prop: 'name',
        sortable: 'custom',
        minWidth: 160,
        align: 'left',
        required: true
      },
      {
        key: 'description',
        label: '描述',
        prop: 'description',
        sortable: 'custom',
        minWidth: 160,
        align: 'left',
        showOverflowTooltip: true,
        defaultVisible: false
      },
      {
        key: 'actions',
        label: '操作',
        minWidth: 50,
        align: 'left',
        placement: 'end',
        required: true
      }
    ]
  )

  // 进入页面时搜索条件默认收起
  searchToggle.value = false

  getDict('dict_type')
    .then((res) => {
      dictTypeOption.value = res
    })
    .catch((err) => {
      console.error('获取 dict_type 失败:', err)
    })

  // 点击字典行事件
  const selectDictionary = (row: any) => {
    selectedDictionaryId.value = row.id
    selectedDictionaryName.value = row.name
    compactPanel.value = 'items'
  }

  const handleDictionaryRowClick = (
    row: any,
    _column: any,
    event: MouseEvent
  ) => {
    // 勾选与行内操作不触发切换，避免窄屏下误跳到字典项
    if (
      event?.target instanceof Element &&
      event.target.closest('button, .el-checkbox')
    )
      return
    selectDictionary(row)
  }
</script>

<style scoped>
  .dictionary-layout {
    flex: 1;
    min-height: 0;
    gap: 12px;
  }
  .dictionary-navigation {
    flex: none;
    margin: 8px 0;
  }
  .dictionary-panel {
    flex: 1;
    min-width: 0;
    min-height: 0;
    padding: 0;
  }
  .dictionary-master {
    flex: 0 0 30%;
    min-width: 360px;
  }
  .dictionary-items {
    flex: 1;
  }
  .dictionary-master :deep(.search-panel__grid) {
    grid-template-columns: minmax(0, 1fr);
  }
  .dictionary-master :deep(.search-panel__actions) {
    grid-column: 1;
  }
  .dictionary-master :deep(.table-row-actions) {
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
  }
  /* 名称列的选择按钮 */
  .dictionary-choice {
    display: flex;
    flex-direction: column;
    gap: 4px;
    width: 100%;
    padding: 6px 0;
    border: 0;
    background: transparent;
    color: var(--app-text-primary);
    font: inherit;
    text-align: left;
    cursor: pointer;
  }
  .dictionary-choice:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 2px;
  }
  .dictionary-choice__name {
    font-weight: 500;
  }
  .dictionary-choice__description {
    color: var(--app-text-secondary);
    font-size: 12px;
    overflow-wrap: anywhere;
    line-height: 1.4;
  }
  .dictionary-choice[aria-pressed='true'] .dictionary-choice__name {
    color: var(--el-color-primary);
  }
  .dictionary-layout.is-compact .dictionary-panel {
    width: 100%;
    min-width: 0;
    flex: 1;
  }
</style>
<style>
  html.dark .el-card {
    --el-card-bg-color: transparent !important;
  }
</style>
