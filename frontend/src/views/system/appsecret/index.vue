<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="应用密钥"
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
          :model="searchInfo"
          :loading="loading"
          @query="refresh"
          @reset="resetQuery"
        >
          <el-form-item label="应用Id">
            <el-input
              v-model="searchInfo.appId"
              oninput="value = value.replace(/[^0-9]/g, '')"
              placeholder="请输入"
            />
          </el-form-item>
          <el-form-item label="应用名称">
            <el-input v-model="searchInfo.appName" placeholder="请输入" />
          </el-form-item>
          <template #advanced>
            <el-form-item label="备注">
              <el-input v-model="searchInfo.remark" placeholder="请输入" />
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
    <formPanel />
  </div>
</template>

<script setup lang="ts">
  import type { AppSecret } from '@/api/system/types/appSecret.types'

  import { computed, ref } from 'vue'
  import { del, edit, get, add, download } from '@/api/system/appSecret'
  import type { AppSecretQueryParams } from '@/api/system/types/appSecret.types'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useCrud } from '@/components/crud/UseCrud'
  import formPanel from './module/formPanel.vue'

  defineOptions({
    name: 'AppSecret'
  })

  const perms = {
    add: ['sys:appSecret:add'],
    edit: ['sys:appSecret:edit'],
    del: ['sys:appSecret:del'],
    download: ['sys:appSecret:download']
  }

  const searchInfo = ref<AppSecretQueryParams>({})

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
      appId: '',
      appSecretKey: '',
      appName: '',
      remark: ''
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

  const tableColumns = computed<TableColumnSpec<AppSecret>[]>(
    () => [
      { key: 'selection', label: '', type: 'selection', width: 55 },
      {
        key: 'appId',
        label: '应用Id',
        prop: 'appId',
        sortable: 'custom',
        minWidth: 160,
        showOverflowTooltip: true
      },
      {
        key: 'appName',
        label: '应用名称',
        prop: 'appName',
        sortable: 'custom',
        minWidth: 160
      },
      {
        key: 'appSecretKey',
        label: '应用密钥',
        prop: 'appSecretKey',
        sortable: 'custom',
        minWidth: 260,
        showOverflowTooltip: true
      },
      {
        key: 'remark',
        label: '备注',
        prop: 'remark',
        minWidth: 160,
        showOverflowTooltip: true
      },
      {
        key: 'creationTime',
        label: '创建时间',
        prop: 'creationTime',
        sortable: 'custom',
        minWidth: 170
      },
      {
        key: 'creatorId',
        label: '创建人ID',
        prop: 'creatorId',
        sortable: 'custom',
        minWidth: 130
      },
      { key: 'actions', label: '操作', width: 180 }
    ]
  )
</script>
