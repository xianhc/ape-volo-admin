<template>
  <div class="table-managed-view">
    <!-- 未选择字典时的提示 -->
    <el-empty
      v-if="!props.selectedDictionaryId"
      description="请在左侧选择一个字典查看详情"
      :image-size="120"
    />

    <!-- 字典详情列表 -->
    <TablePage
      v-else
      title="字典详情"
      :data="data"
      :columns="tableColumns"
      :loading="loading"
      :error="crud.error.value"
      row-key="id"
      @selection-change="onSelectionChange"
      @sort-change="onSortChange"
      @refresh="refresh"
    >
      <template #search>
        <SearchPanel
          :model="detailSearchInfo"
          :loading="loading"
          @query="refresh"
          @reset="resetQuery"
        >
          <el-form-item label="标签名">
            <el-input v-model="detailSearchInfo.label" placeholder="请输入" />
          </el-form-item>
          <el-form-item label="数据值">
            <el-input v-model="detailSearchInfo.value" placeholder="请输入" />
          </el-form-item>
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
          :selected-count="multipleSelection.length"
          @create="toAdd()"
          @remove="doBatchDelete()"
        />
      </template>
      <template #column-actions="scope">
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
      </template>
      <template #pagination>
        <el-pagination v-bind="pagination" />
      </template>
    </TablePage>

    <!--表单渲染-->
    <dictionaryDetailFormPanel :dict-name="selectedDictionaryName" />
  </div>
</template>

<script setup lang="ts">
  import type { DictionaryDetail } from '@/api/system/types/dictionaryDetail.types'

  import { computed, ref, watch } from 'vue'
  import { get, del, add, edit } from '@/api/system/dictionaryDetail'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useCrud } from '@/components/crud/UseCrud'
  import dictionaryDetailFormPanel from './module/dictionaryDetailFormPanel.vue'

  interface DetailSearchInfo {
    label?: string
    value?: string
    dictId?: string
  }

  defineOptions({
    name: 'DictionaryDetail'
  })

  const perms = {
    add: ['sys:dict:add'],
    edit: ['sys:dict:edit'],
    del: ['sys:dict:del'],
    download: ['sys:dict:download']
  }

  const props = defineProps({
    selectedDictionaryId: {
      type: String,
      default: ''
    },
    selectedDictionaryName: {
      type: String,
      default: ''
    }
  })
  const dictName = ref('')

  // 使用不同的变量名避免冲突
  const detailSearchInfo = ref<DetailSearchInfo>({
    label: '',
    value: '',
    dictId: undefined
  })
  const crud = useCrud({
    crudMethod: {
      list: get,
      del: del,
      add: add,
      edit: edit
    },
    defaultForm: () => ({
      id: '0',
      dictId: props.selectedDictionaryId,
      label: '',
      value: '',
      dictSort: 999
    }),
    searchInfo: detailSearchInfo,
    sortProp: 'dictSort',
    sortOrder: 'ascending',
    refreshOnInit: false
  })
  const {
    data,
    loading,
    refresh,
    resetQuery,
    pagination,
    onSelectionChange,
    toAdd,
    doBatchDelete,
    multipleSelection,
    onSortChange
  } = crud

  const tableColumns = computed<TableColumnSpec<DictionaryDetail>[]>(
    () => [
      {
        key: 'selection',
        label: '',
        type: 'selection',
        width: 55,
        align: 'left'
      },
      {
        key: 'label',
        label: '标签名',
        prop: 'label',
        sortable: 'custom',
        minWidth: 160,
        align: 'left'
      },
      {
        key: 'value',
        label: '数据值',
        prop: 'value',
        sortable: 'custom',
        minWidth: 160,
        align: 'left'
      },
      {
        key: 'dictSort',
        label: '排序',
        prop: 'dictSort',
        sortable: 'custom',
        width: 110,
        align: 'left'
      },
      {
        key: 'creationTime',
        label: '创建时间',
        prop: 'creationTime',
        sortable: 'custom',
        minWidth: 170,
        align: 'left'
      },
      {
        key: 'actions',
        label: '操作',
        width: 180,
        align: 'left',
        placement: 'end'
      }
    ]
  )

  // 切换左侧字典时同步查询条件并重新加载字典项
  watch(
    () => [props.selectedDictionaryId, props.selectedDictionaryName] as const,
    ([id, name]) => {
      detailSearchInfo.value.dictId = id || undefined
      dictName.value = name || ''
      if (id) refresh()
    }
  )
</script>
