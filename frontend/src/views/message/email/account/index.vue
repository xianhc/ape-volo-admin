<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="邮箱账户"
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
          <el-form-item label="邮箱">
            <el-input v-model="searchInfo.email" placeholder="请输入" />
          </el-form-item>
          <el-form-item label="显示名称">
            <el-input v-model="searchInfo.displayName" placeholder="请输入" />
          </el-form-item>
          <template #advanced>
            <el-form-item label="用户名称">
              <el-input v-model="searchInfo.userName" placeholder="请输入" />
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
        />
      </template>
      <template #column-enableSsl="scope">
        <el-switch v-model="scope.row.enableSsl" :disabled="true" />
      </template>
      <template #column-useDefaultCredentials="scope">
        <el-switch v-model="scope.row.useDefaultCredentials" :disabled="true" />
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
  import type { EmailAccount } from '@/api/message/email/emailAccount'

  import { computed, ref } from 'vue'
  import { del, edit, get, add } from '@/api/message/email/emailAccount'
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
    name: 'EmailAccount'
  })

  const perms = {
    add: ['sys:emailAccount:add'],
    edit: ['sys:emailAccount:edit'],
    del: ['sys:emailAccount:del'],
    download: ['sys:emailAccount:download']
  }

  const searchInfo = ref<Record<string, string | number | null>>({
    email: null,
    displayName: null,
    userName: null
  })

  const crud = useCrud({
    crudMethod: {
      list: get,
      del: del,
      add: add,
      edit: edit,
      download: () => Promise.resolve({} as any)
    },
    defaultForm: () => ({
      id: '0',
      email: undefined,
      displayName: undefined,
      host: undefined,
      port: undefined,
      userName: undefined,
      password: undefined,
      enableSsl: false,
      useDefaultCredentials: false
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

  const tableColumns = computed<TableColumnSpec<EmailAccount>[]>(
    () => [
      { key: 'selection', label: '', type: 'selection', width: 55 },
      { key: 'id', label: '账号Id', prop: 'id', width: 160, showOverflowTooltip: true },
      {
        key: 'email',
        label: '邮箱',
        prop: 'email',
        sortable: 'custom',
        minWidth: 200
      },
      {
        key: 'displayName',
        label: '显示名称',
        prop: 'displayName',
        sortable: 'custom',
        minWidth: 150
      },
      {
        key: 'host',
        label: '主机',
        prop: 'host',
        sortable: 'custom',
        minWidth: 160
      },
      {
        key: 'port',
        label: '端口',
        prop: 'port',
        sortable: 'custom',
        width: 100
      },
      {
        key: 'userName',
        label: '用户名称',
        prop: 'userName',
        sortable: 'custom',
        minWidth: 150
      },
      {
        key: 'enableSsl',
        label: 'Ssl',
        prop: 'enableSsl',
        sortable: 'custom',
        width: 90
      },
      {
        key: 'useDefaultCredentials',
        label: '系统凭据',
        prop: 'useDefaultCredentials',
        sortable: 'custom',
        width: 110
      },
      {
        key: 'creationTime',
        label: '创建时间',
        prop: 'creationTime',
        sortable: 'custom',
        minWidth: 170
      },
      { key: 'actions', label: '操作', width: 180 }
    ]
  )
</script>
