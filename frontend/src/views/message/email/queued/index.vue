<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="邮件队列"
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
                  :value="strToBool(String(item.value))"
                />
              </el-select>
            </el-form-item>
            <el-form-item label="优先级">
              <el-select
                v-model="searchInfo.priority"
                clearable
                placeholder="请选择"
              >
                <el-option
                  v-for="item in priorityTypeOption"
                  :key="item.value"
                  :label="item.label"
                  :value="Number(item.value)"
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
          :permissions="{
            create: perms.add,
            remove: perms.del
          }"
          :selected-count="crud.multipleSelection.value.length"
          @create="crud.toAdd()"
          @remove="crud.doBatchDelete()"
        />
      </template>
      <template #column-isSend="scope">
        <el-switch v-model="scope.row.isSend" :disabled="true" inline-prompt />
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
    <formPanel
      :email-account-option="emailAccountOption"
      :status-type-option="statusTypeOption"
      :priority-type-option="priorityTypeOption"
    />
  </div>
</template>

<script setup lang="ts">
  import type { EmailQueued } from '@/api/message/email/emailQueued'

  import { computed, ref } from 'vue'
  import { getAll, type EmailAccount } from '@/api/message/email/emailAccount'
  import { del, edit, get, add } from '@/api/message/email/emailQueued'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useCrud } from '@/components/crud/UseCrud'
  import { getDict, type DictOption } from '@/composables/useDictionary'
  import { strToBool } from '@/utils/converter'
  import formPanel from './module/formPanel.vue'

  defineOptions({
    name: 'EmailQueued'
  })

  const perms = {
    add: ['sys:queuedEmail:add'],
    edit: ['sys:queuedEmail:edit'],
    del: ['sys:queuedEmail:del']
  }

  const searchInfo = ref<Record<string, string | number | null>>({
    email: null,
    displayName: null,
    enabled: null,
    priority: null
  })

  // 状态
  const statusTypeOption = ref<DictOption[]>([])
  // 优先级
  const priorityTypeOption = ref<DictOption[]>([])
  const emailAccountOption = ref<EmailAccount[]>([])

  const crud = useCrud({
    crudMethod: {
      list: get,
      del: del,
      add: add,
      edit: edit
    },
    defaultForm: () => ({
      id: '0',
      subject: undefined,
      from: undefined,
      fromName: undefined,
      to: undefined,
      toName: undefined,
      bcc: undefined,
      isSend: false,
      sentTries: 0,
      sendTime: undefined,
      priority: undefined,
      emailAccountId: undefined
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

  const tableColumns = computed<TableColumnSpec<EmailQueued>[]>(
    () => [
      { key: 'selection', label: '', type: 'selection', width: 55 },
      {
        key: 'subject',
        label: '邮件主题',
        prop: 'subject',
        sortable: 'custom',
        minWidth: 200
      },
      {
        key: 'from',
        label: '发件邮箱',
        prop: 'from',
        sortable: 'custom',
        minWidth: 180
      },
      {
        key: 'fromName',
        label: '发件人名称',
        prop: 'fromName',
        sortable: 'custom',
        minWidth: 150
      },
      {
        key: 'to',
        label: '收件邮箱',
        prop: 'to',
        sortable: 'custom',
        minWidth: 180
      },
      {
        key: 'toName',
        label: '收件人名称',
        prop: 'toName',
        sortable: 'custom',
        minWidth: 150
      },
      {
        key: 'bcc',
        label: '抄送邮箱',
        prop: 'bcc',
        sortable: 'custom',
        minWidth: 200,
        showOverflowTooltip: true
      },
      {
        key: 'isSend',
        label: '是否已发送',
        prop: 'isSend',
        sortable: 'custom',
        width: 120
      },
      {
        key: 'sentTries',
        label: '发送次数',
        prop: 'sentTries',
        sortable: 'custom',
        width: 110
      },
      {
        key: 'sendTime',
        label: '发送时间',
        prop: 'sendTime',
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
      { key: 'actions', label: '操作', width: 180 }
    ]
  )

  const init = async () => {
    statusTypeOption.value = await getDict('status_type')
    priorityTypeOption.value = await getDict('priority_type')

    const res = await getAll()
    emailAccountOption.value = res.data
  }

  init()
</script>
