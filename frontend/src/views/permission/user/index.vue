<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="用户管理"
      :data="data"
      :columns="tableColumns"
      :loading="loading"
      :error="tableError"
      row-key="id"
      @selection-change="onSelectionChange"
      @sort-change="onSortChange"
      @refresh="refresh"
    >
      <template #search>
        <SearchPanel
          :model="searchInfo"
          :loading="loading"
          @query="search"
          @reset="resetQuery"
        >
          <el-form-item label="部门">
            <el-cascader
              v-model="searchInfo.departmentIdArray"
              style="width: 250px"
              :options="departmentOption"
              :props="{
                checkStrictly: true,
                multiple: true,
                label: 'name',
                value: 'id',
                emitPath: false
              }"
              :show-all-levels="false"
              filterable
              clearable
              collapse-tags
              collapse-tags-tooltip
            />
          </el-form-item>
          <el-form-item label="用户名">
            <el-input
              v-model="searchInfo.username"
              placeholder="请输入"
              clearable
            />
          </el-form-item>
          <template #advanced>
            <el-form-item label="昵称">
              <el-input v-model="searchInfo.nickname" placeholder="请输入" />
            </el-form-item>
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
          :show-create="canAssignRoles"
          :permissions="{
            create: perms.add,
            remove: perms.del,
            export: perms.download
          }"
          :selected-count="selection.length"
          :submitting="removing"
          :exporting="exporting"
          @create="openCreate"
          @remove="removeUsers(selection)"
          @export="runExport"
        />
      </template>
      <template #column-dept="scope">
        {{ scope.row.dept?.name }}
      </template>
      <template #column-roles="scope">
        <el-select
          v-model="scope.row.roles"
          :disabled="!canAssignRoles || assignmentPending[scope.row.id + ':roles']"
          value-key="id"
          multiple
          collapse-tags
          collapse-tags-tooltip
          placeholder="请选择"
          @visible-change="handleRoleVisibleChange(scope.row, $event)"
          @remove-tag="handleRoleRemove(scope.row, $event)"
        >
          <el-option
            v-for="item in allRoleData"
            :key="item.id"
            :label="item.name"
            :value="item"
          />
        </el-select>
      </template>
      <template #column-positions="scope">
        <el-select
          v-model="scope.row.positions"
          :disabled="!canEditUsers || assignmentPending[scope.row.id + ':positions']"
          value-key="id"
          multiple
          collapse-tags
          collapse-tags-tooltip
          placeholder="请选择"
          @visible-change="handlePositionVisibleChange(scope.row, $event)"
          @remove-tag="handlePositionRemove(scope.row, $event)"
        >
          <el-option
            v-for="item in allPositionData"
            :key="item.id"
            :label="item.name"
            :value="item"
          />
        </el-select>
      </template>
      <template #column-enabled="scope">
        <el-switch
          v-model="scope.row.enabled"
          inline-prompt
          :loading="loadingMap[scope.row.id]"
          :disabled="!canEditUsers"
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
            @click="openEdit(scope.row)"
          >
            编辑
          </el-button>
          <el-button
            v-has-perm="perms.del"
            :icon="createIconifyIcon('lucide:trash-2')"
            type="danger"
            link
            @click="removeUsers([scope.row])"
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
      v-model="formVisible"
      :user="editingUser"
      :gender-option="genderOption"
      :status-type-option="statusTypeOption"
      :department-option="departmentOption"
      :position-option="allPositionData"
      :role-option="allRoleData"
      @saved="refresh"
    />
  </div>
</template>

<script setup lang="ts">
  import { computed, reactive, ref } from 'vue'
  import { ElMessage } from 'element-plus'
  import { edit, del, get, download } from '@/api/permission/user'
  import type { User, UserQueryParams } from '@/api/permission/types/user.types'
  import { getAll as getAllRole, type Role } from '@/api/permission/role'
  import {
    getAll as getAllPosition,
    type Position
  } from '@/api/organization/positions'
  import { getDeptTree, type Department } from '@/api/permission/department'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useTableController } from '@/composables/useTableController'
  import { useExportAction } from '@/composables/useExportAction'
  import { useMutationAction } from '@/composables/useMutationAction'
  import { confirmAction } from '@/services/feedback/confirmation'
  import { useUserAssignments } from './module/useUserAssignments'
  import { getDict, showDictLabel, type DictOption } from '@/composables/useDictionary'
  import { useUserStore } from '@/pinia/modules/user'
  import formPanel from './module/formPanel.vue'

  defineOptions({
    name: 'User'
  })

  const perms = {
    add: ['sys:user:add'],
    edit: ['sys:user:edit'],
    del: ['sys:user:del'],
    download: ['sys:user:download']
  }

  //字典
  const statusTypeOption = ref<DictOption[]>([])
  const genderOption = ref<DictOption[]>([])
  const departmentOption = ref<Department[]>([])

  const formatGender = (_row: User, _column: unknown, cellValue: unknown) => {
    return showDictLabel(genderOption.value, String(cellValue))
  }

  //角色
  const allRoleData = ref<Role[]>([])
  //岗位
  const allPositionData = ref<Position[]>([])

  const userStore = useUserStore()
  const canAssignRoles = computed(() => userStore.userInfo.roleCodes.includes('admin'))
  const canEditUsers = computed(() => userStore.userInfo.authCodes.includes('sys:user:edit'))
  const checkboxT = (row: User) => {
    return row.id !== userStore.userInfo.id
  }

  const queryDefaults = (): UserQueryParams => ({})
  const searchInfo = ref<UserQueryParams>(queryDefaults())
  const formVisible = ref(false)
  const editingUser = ref<User | null>(null)
  const openCreate = () => { editingUser.value = null; formVisible.value = true }
  const openEdit = (row: User) => { editingUser.value = row; formVisible.value = true }
  const optionsReady = ref(false)
  const {
    data, loading, error: tableError, refresh, search, resetQuery, selection,
    onSelectionChange, pagination, searchVisible: searchToggle, onSortChange, currentQuery
  } = useTableController<UserQueryParams, User>({
    list: get, query: searchInfo, defaults: queryDefaults,
    readiness: { ready: optionsReady, initialize: async () => { await init(); optionsReady.value = true; return true } }
  })
  const { exporting, run: runExport } = useExportAction({ export: download, currentQuery: () => currentQuery(false), fallbackFileName: '用户列表.xlsx' })
  const removal = useMutationAction(async (rows: User[]) => {
    if (!rows.length || !await confirmAction('确认删除选中的' + rows.length + '条用户?', '删除用户')) return false
    await del(rows.map(row => row.id))
    ElMessage.success('删除成功')
    await refresh()
  })
  const removing = removal.pending
  const removeUsers = removal.run
  const { assignmentPending, handleRoleVisibleChange, handleRoleRemove, handlePositionVisibleChange, handlePositionRemove } = useUserAssignments()

  const tableColumns = computed<TableColumnSpec<User>[]>(
    () => [
      {
        key: 'selection',
        label: '',
        type: 'selection',
        width: 55,
        align: 'left',
        selectable: checkboxT
      },
      {
        key: 'userName',
        label: '用户名',
        prop: 'userName',
        sortable: 'custom',
        minWidth: 130,
        align: 'left'
      },
      {
        key: 'nickName',
        label: '用户昵称',
        prop: 'nickName',
        sortable: 'custom',
        minWidth: 130,
        align: 'left'
      },
      {
        key: 'email',
        label: '邮箱',
        prop: 'email',
        sortable: 'custom',
        minWidth: 190,
        align: 'left'
      },
      {
        key: 'phone',
        label: '电话',
        prop: 'phone',
        sortable: 'custom',
        minWidth: 140,
        align: 'left'
      },
      {
        key: 'genderCode',
        label: '性别',
        prop: 'genderCode',
        minWidth: 90,
        align: 'left',
        formatter: formatGender
      },
      { key: 'dept', label: '部门', minWidth: 150, align: 'left' },
      { key: 'roles', label: '角色', minWidth: 200, align: 'left' },
      { key: 'positions', label: '岗位', minWidth: 180, align: 'left' },
      {
        key: 'enabled',
        label: '状态',
        prop: 'enabled',
        minWidth: 110,
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

  async function init(): Promise<void> {
    const [status, genders, departments, roles, positions] = await Promise.all([
      getDict('status_type'), getDict('gender_code'), getDeptTree(), getAllRole(), getAllPosition()
    ])
    statusTypeOption.value = status
    genderOption.value = genders
    departmentOption.value = departments.data
    allRoleData.value = roles.data
    allPositionData.value = positions.data
  }

  const loadingMap = reactive<Record<string, boolean>>({})
  const changeEnabled = async (row: User, value: boolean): Promise<void> => {
    if (loadingMap[row.id]) return
    loadingMap[row.id] = true
    try {
      if (!await confirmAction('你要将' + row.userName + '的状态切换为【' + (value ? '启用' : '禁用') + '】吗？', '切换状态')) {
        row.enabled = !value
        return
      }
      await edit(row)
      ElMessage.success('修改成功')
    } catch { row.enabled = !value }
    finally { loadingMap[row.id] = false }
  }
</script>
