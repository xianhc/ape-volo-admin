<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="邮件模板"
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
          :permissions="{
            create: perms.add,
            remove: perms.del
          }"
          :selected-count="crud.multipleSelection.value.length"
          @create="crud.toAdd()"
          @remove="crud.doBatchDelete()"
        />
      </template>
      <template #column-emailAccountId="scope">
        <el-select
          v-model="scope.row.emailAccountId"
          style="width: 200px"
          value-key="id"
        >
          <el-option
            v-for="item in emailAccountOption"
            :key="item.id"
            :label="item.displayName"
            :value="item.id"
          />
        </el-select>
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
    />
  </div>
</template>

<script setup lang="ts">
  import type { EmailTemplate } from '@/api/message/email/emailTemplate'

  import { computed, reactive, ref } from 'vue'
  import { ElMessage, ElMessageBox } from 'element-plus'
  import { getAll, type EmailAccount } from '@/api/message/email/emailAccount'
  import { del, edit, get, add } from '@/api/message/email/emailTemplate'
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
    name: 'EmailTemplate'
  })

  const perms = {
    add: ['sys:emailTemplate:add'],
    edit: ['sys:emailTemplate:edit'],
    del: ['sys:emailTemplate:del']
  }

  const searchInfo = ref<Record<string, string | number | null>>({
    email: null,
    displayName: null,
    enabled: null
  })

  // 状态
  const statusTypeOption = ref<DictOption[]>([])
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
      name: undefined,
      bccEmailAddresses: undefined,
      subject: undefined,
      body: '',
      enabled: true,
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

  const tableColumns = computed<TableColumnSpec<EmailTemplate>[]>(
    () => [
      { key: 'selection', label: '', type: 'selection', width: 55 },
      {
        key: 'id',
        label: '模板Id',
        prop: 'id',
        sortable: 'custom',
        width: 160,
        showOverflowTooltip: true
      },
      {
        key: 'name',
        label: '模板名称',
        prop: 'name',
        sortable: 'custom',
        minWidth: 160
      },
      {
        key: 'subject',
        label: '邮件主题',
        prop: 'subject',
        sortable: 'custom',
        minWidth: 200
      },
      {
        key: 'emailAccountId',
        label: '发送邮箱',
        prop: 'emailAccountId',
        sortable: 'custom',
        minWidth: 220
      },
      {
        key: 'bccEmailAddresses',
        label: '抄送邮箱',
        prop: 'bccEmailAddresses',
        sortable: 'custom',
        minWidth: 200,
        showOverflowTooltip: true
      },
      {
        key: 'enabled',
        label: '状态',
        prop: 'enabled',
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

  const init = async () => {
    statusTypeOption.value = await getDict('status_type')

    const res = await getAll()
    emailAccountOption.value = res.data
  }

  init()

  const loadingMap = reactive<Record<string | number, boolean>>({})
  const changeEnabled = async (row: any, val: boolean) => {
    loadingMap[row.id] = true
    ElMessageBox.confirm(
      `你要将${row.name}的状态切换为【${val ? '启用' : '禁用'}】吗？`,
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
</script>
