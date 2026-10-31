<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="部门管理"
      :data="data"
      :columns="tableColumns"
      :loading="loading"
      :error="crud.error.value"
      row-key="id"
      lazy
      :load="GetChildren"
      :tree-props="{ children: 'children', hasChildren: 'hasChildren' }"
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
          <el-form-item label="部门名称">
            <el-input
              v-model="searchInfo.name"
              clearable
              style="width: 200px"
              placeholder="请输入"
            />
          </el-form-item>
          <el-form-item label="部门状态">
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
            v-has-perm="perms.add"
            :icon="createIconifyIcon('lucide:plus')"
            type="primary"
            link
            @click="crud.toAdd({ parentId: scope.row.id })"
          >
            新增下级
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
    <formPanel :status-type-option="statusTypeOption" />
  </div>
</template>

<script setup lang="ts">
  import type { Department } from '@/api/permission/types/department.types'

  import { computed, reactive, ref } from 'vue'
  import { ElMessage, ElMessageBox } from 'element-plus'
  import { add, del, edit, get, download } from '@/api/permission/department'
  import type { DepartmentQueryParams } from '@/api/permission/types/department.types'
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
    name: 'Department'
  })

  const perms = {
    add: ['sys:dept:add'],
    edit: ['sys:dept:edit'],
    del: ['sys:dept:del'],
    download: ['sys:dept:download']
  }

  // 懒加载树表的根列表只取顶级部门（后端 parent_id IS NULL）；
  // 不传 isRoot 也不传 parentId 时后端按「未参与过滤」处理，会把所有层级平铺返回。
  const searchInfo = ref<DepartmentQueryParams>({ isRoot: true })
  // 状态
  const statusTypeOption = ref<DictOption[]>([])
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
      name: '',
      // null 表示顶级部门
      parentId: null,
      sort: 999,
      enabled: true
    }),
    searchInfo
  })
  const {
    data,
    loading,
    refresh,
    resetQuery,
    pagination,
    searchToggle,
    onSelectionChange,
    onSortChange
  } = crud

  const tableColumns = computed<TableColumnSpec<Department>[]>(
    () => [
      {
        key: 'name',
        label: '名称',
        prop: 'name',
        sortable: 'custom',
        minWidth: 220,
        align: 'left'
      },
      {
        key: 'sort',
        label: '排序',
        prop: 'sort',
        sortable: 'custom',
        width: 110,
        align: 'left'
      },
      {
        key: 'enabled',
        label: '状态',
        prop: 'enabled',
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
      { key: 'actions', label: '操作', width: 260, align: 'left' }
    ]
  )

  function GetChildren(tree: any, _treeNode: any, resolve: any) {
    const params = { parentId: tree.id }
    setTimeout(() => {
      get(params).then((res) => {
        resolve(res.data?.items ?? res.data)
      })
    }, 200)
  }

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

  getDict('status_type')
    .then((res) => {
      statusTypeOption.value = res
    })
    .catch((err) => {
      console.error('获取 status_type 失败:', err)
    })
</script>
