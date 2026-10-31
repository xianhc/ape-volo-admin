<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="角色管理"
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
          <el-form-item label="角色名称">
            <el-input
              v-model="searchInfo.name"
              clearable
              style="width: 200px"
              placeholder="请输入"
            />
          </el-form-item>
          <el-form-item label="数据权限">
            <el-select
              v-model="searchInfo.dataScopeType"
              clearable
              placeholder="请选择"
            >
              <el-option
                v-for="item in dataScopeTypeOption"
                :key="Number(item.value)"
                :label="item.label"
                :value="Number(item.value)"
              />
            </el-select>
          </el-form-item>
          <template #advanced>
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
            v-has-role="'admin'"
            :icon="createIconifyIcon('lucide:settings')"
            type="primary"
            link
            @click="handleOpenAuthorizeDialog(scope.row.id)"
          >
            设置权限
          </el-button>
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
    <formPanel :data-scope-type-option="dataScopeTypeOption" />

    <!--权限配置-->
    <authorizePanel ref="authorizeRef" />
  </div>
</template>

<script setup lang="ts">
  import type { Role } from '@/api/permission/types/role.types'

  import { computed, ref } from 'vue'
  import { add, edit, del, get, download } from '@/api/permission/role'
  import type { RoleQueryParams } from '@/api/permission/types/role.types'
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
  import authorizePanel from './module/authorizePanel.vue'

  defineOptions({
    name: 'Role'
  })

  const perms = {
    add: ['sys:role:add'],
    edit: ['sys:role:edit'],
    del: ['sys:role:del'],
    download: ['sys:role:download']
  }

  // 数据权限
  const dataScopeTypeOption = ref<DictOption[]>([])

  // 格式化数据权限
  const formatDataScope = (_row: any, _column: any, cellValue: any) => {
    return showDictLabel(dataScopeTypeOption.value, cellValue)
  }

  const searchInfo = ref<RoleQueryParams>({})
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
      name: undefined,
      authCode: undefined,
      deptIdArray: [],
      description: undefined,
      dataScopeType: undefined
    }),
    searchInfo
  })
  const {
    data,
    loading,
    refresh,
    resetQuery,
    onSelectionChange,
    onSortChange,
    pagination,
    searchToggle
  } = crud

  const tableColumns = computed<TableColumnSpec<Role>[]>(
    () => [
      {
        key: 'selection',
        label: '',
        type: 'selection',
        width: 55,
        align: 'left'
      },
      {
        key: 'name',
        label: '角色名称',
        prop: 'name',
        sortable: 'custom',
        minWidth: 160,
        align: 'left'
      },
      {
        key: 'authCode',
        label: '角色代码',
        prop: 'authCode',
        sortable: 'custom',
        minWidth: 160,
        align: 'left'
      },
      {
        key: 'dataScopeType',
        label: '数据权限',
        prop: 'dataScopeType',
        sortable: 'custom',
        width: 130,
        align: 'left',
        formatter: formatDataScope
      },
      {
        key: 'description',
        label: '描述',
        prop: 'description',
        minWidth: 180,
        align: 'left',
        showOverflowTooltip: true
      },
      {
        key: 'creationTime',
        label: '创建日期',
        prop: 'creationTime',
        sortable: 'custom',
        minWidth: 170,
        align: 'left'
      },
      { key: 'actions', label: '操作', width: 260, align: 'left' }
    ]
  )

  getDict('data_scope_type')
    .then((res) => {
      dataScopeTypeOption.value = res
    })
    .catch((err) => {
      console.error('获取 data_scope_type 失败:', err)
    })

  const authorizeRef = ref<any>(null)
  const handleOpenAuthorizeDialog = (id: string) => {
    authorizeRef.value?.openAuthorizeDialog(id) // 调用子组件方法并传递行对象
  }
</script>
