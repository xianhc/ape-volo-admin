<template>
  <div ref="workspaceRef" class="table-managed-view department-workspace">
    <el-radio-group
      v-if="compact"
      v-model="compactPanel"
      class="department-navigation"
      aria-label="部门管理面板"
    >
      <el-radio-button value="departments">部门列表</el-radio-button>
      <el-radio-button value="members" :disabled="!selectedDepartment">
        部门用户
      </el-radio-button>
    </el-radio-group>
    <div class="department-layout" :class="{ 'is-compact': compact }">
      <section
        v-show="!compact || compactPanel === 'departments'"
        class="department-panel department-master"
        aria-label="部门列表"
      >
        <TablePage
          v-model:search-visible="searchToggle"
          title="部门管理"
          :data="data"
          :columns="tableColumns"
          :loading="loading"
          :error="crud.error.value"
          row-key="id"
          highlight-current-row
          default-expand-all
          :current-row-key="selectedDepartmentId"
          :tree-props="{ children: 'children' }"
          @selection-change="onSelectionChange"
          @sort-change="onSortChange"
          @row-click="handleDepartmentRowClick"
          @refresh="refresh"
        >
          <template #search>
            <SearchPanel :model="searchInfo" :loading="loading" @query="refresh" @reset="resetQuery">
              <el-form-item label="部门名称">
                <el-input v-model="searchInfo.name" clearable placeholder="请输入" />
              </el-form-item>
              <el-form-item label="部门状态">
                <el-select v-model="searchInfo.enabled" clearable placeholder="请选择">
                  <el-option
                    v-for="item in statusTypeOption"
                    :key="item.value"
                    :label="item.label"
                    :value="String(item.value) === 'true'"
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
              :permissions="{ create: perms.add, remove: perms.del, export: perms.download }"
              :selected-count="crud.multipleSelection.value.length"
              @create="crud.toAdd()"
              @remove="crud.doBatchDelete()"
              @export="crud.doExport()"
            />
          </template>
          <template #column-name="{ row }">
            <button
              class="department-choice"
              type="button"
              :aria-pressed="row.id === selectedDepartmentId"
              @click.stop="selectDepartment(row)"
            >
              {{ row.name }}
            </button>
          </template>
          <template #column-enabled="{ row }">
            <div @click.stop>
              <el-switch
                v-model="row.enabled"
                size="small"
                inline-prompt
                :disabled="!canEditDepartments"
                :loading="loadingMap[row.id]"
                :active-text="showDictLabel(statusTypeOption, 'true') || '启用'"
                :inactive-text="showDictLabel(statusTypeOption, 'false') || '禁用'"
                @change="changeEnabled(row, row.enabled)"
              />
            </div>
          </template>
          <template #column-actions="{ row }">
            <div @click.stop>
              <RowActions>
                <el-button
                  v-has-perm="perms.add"
                  :icon="createIconifyIcon('lucide:plus')"
                  type="primary"
                  link
                  @click="crud.toAdd({ parentId: row.id })"
                >
                  新增下级
                </el-button>
                <el-button
                  v-has-perm="perms.edit"
                  :icon="createIconifyIcon('lucide:pencil')"
                  type="primary"
                  link
                  @click="crud.toEdit(row)"
                >
                  编辑
                </el-button>
                <el-button
                  v-has-perm="perms.del"
                  :icon="createIconifyIcon('lucide:trash-2')"
                  type="danger"
                  link
                  @click="crud.doDelete(row)"
                >
                  删除
                </el-button>
              </RowActions>
            </div>
          </template>
        </TablePage>
      </section>
      <section
        v-show="!compact || compactPanel === 'members'"
        class="department-panel department-members"
        aria-label="部门用户"
      >
        <el-empty v-if="!selectedDepartment" description="请在左侧选择部门查看用户" />
        <el-empty v-else-if="!canReadUsers" description="暂无用户查看权限" />
        <DepartmentUsers
          v-else
          :key="selectedDepartment.id"
          :department-id="selectedDepartment.id"
          :department-name="selectedDepartment.name"
        />
      </section>
    </div>
    <formPanel :status-type-option="statusTypeOption" />
  </div>
</template>

<script setup lang="ts">
  import { computed, reactive, ref, watch } from 'vue'
  import { useElementSize } from '@vueuse/core'
  import { ElMessage } from 'element-plus'
  import type { Department, DepartmentQueryParams } from '@/api/organization/department/types'
  import { add, del, edit, getDeptTree, download } from '@/api/organization/department'
  import { filterDepartmentTree } from '@/api/organization/department/adapter'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useCrud } from '@/components/crud/UseCrud'
  import { showDictLabel, useDictionary } from '@/composables/useDictionary'
  import { confirmAction } from '@/services/feedback/confirmation'
  import { useUserStore } from '@/pinia/modules/user'
  import DepartmentUsers from './module/DepartmentUsers.vue'
  import formPanel from './module/formPanel.vue'

  defineOptions({ name: 'Department' })

  const perms = {
    add: ['sys:dept:add'],
    edit: ['sys:dept:edit'],
    del: ['sys:dept:del'],
    download: ['sys:dept:download']
  }
  const userStore = useUserStore()
  const canReadUsers = computed(() => userStore.userInfo.authCodes.includes('sys:user:list'))
  const canEditDepartments = computed(() => userStore.userInfo.authCodes.includes('sys:dept:edit'))
  const { options: statusTypeOption } = useDictionary('status_type')
  const workspaceRef = ref<HTMLElement>()
  const { width } = useElementSize(workspaceRef)
  const compact = computed(() => width.value > 0 && width.value < 1100)
  const compactPanel = ref<'departments' | 'members'>('departments')
  const selectedDepartmentId = ref<string | null>(null)
  const searchInfo = ref<DepartmentQueryParams>({})

  const crud = useCrud({
    crudMethod: {
      list: async (params, options) => {
        const response = await getDeptTree(options)
        const items = filterDepartmentTree(response.data, params)
        return { ...response, data: { items, totalCount: items.length } }
      },
      del, add, edit, download
    },
    defaultForm: () => ({ id: '0', name: '', parentId: null as string | null, sort: 999, enabled: true }),
    searchInfo,
    sortProp: 'sort',
    sortOrder: 'ascending'
  })
  const { data, loading, refresh, resetQuery, searchToggle, onSelectionChange, onSortChange } = crud
  searchToggle.value = false

  function findDepartment(departments: Department[], id: string | null): Department | undefined {
    for (const department of departments) {
      if (department.id === id) return department
      const child = findDepartment(department.children ?? [], id)
      if (child) return child
    }
    return undefined
  }

  const selectedDepartment = computed(() => findDepartment(data.value, selectedDepartmentId.value))
  watch(selectedDepartment, (department) => {
    if (!department) {
      selectedDepartmentId.value = null
      compactPanel.value = 'departments'
    }
  })

  function selectDepartment(department: Department) {
    selectedDepartmentId.value = department.id
    compactPanel.value = 'members'
  }

  function handleDepartmentRowClick(department: Department, _column: unknown, event: MouseEvent) {
    if (event.target instanceof Element && event.target.closest('button, .el-checkbox, .el-switch, .el-table__expand-icon')) return
    selectDepartment(department)
  }

  const tableColumns: TableColumnSpec<Department>[] = [
    { key: 'selection', label: '', type: 'selection', width: 40, placement: 'start' },
    { key: 'name', label: '部门名称', prop: 'name', minWidth: 150, align: 'left', required: true },
    { key: 'enabled', label: '状态', prop: 'enabled', width: 75, align: 'left' },
    { key: 'sort', label: '排序', prop: 'sort', sortable: 'custom', width: 90, defaultVisible: false },
    { key: 'creationTime', label: '创建时间', prop: 'creationTime', sortable: 'custom', minWidth: 170, defaultVisible: false },
    { key: 'actions', label: '操作', minWidth: 95, align: 'left', placement: 'end', required: true }
  ]

  const loadingMap = reactive<Record<string, boolean>>({})
  async function changeEnabled(row: Department, value: boolean): Promise<void> {
    if (loadingMap[row.id]) return
    loadingMap[row.id] = true
    try {
      if (!await confirmAction('你要将' + row.name + '的状态切换为【' + (value ? '启用' : '禁用') + '】吗？', '切换状态')) {
        row.enabled = !value
        return
      }
      await edit({ id: row.id, name: row.name, parentId: row.parentId, sort: row.sort, enabled: value })
      ElMessage.success('修改成功')
    } catch { row.enabled = !value }
    finally { loadingMap[row.id] = false }
  }
</script>

<style scoped>
  .department-workspace {
    min-width: 0;
    min-height: 0;
  }
  .department-layout {
    display: flex;
    flex: 1;
    min-height: 0;
    gap: 12px;
  }
  .department-navigation {
    flex: none;
    margin: 8px 0;
  }
  .department-panel {
    display: flex;
    flex: 1;
    min-width: 0;
    min-height: 0;
  }
  .department-master {
    flex: 0 0 30%;
    min-width: 520px;
  }
  .department-panel > :deep(.table-page),
  .department-members > .el-empty {
    flex: 1;
    width: 100%;
  }
  .department-master :deep(.search-panel__grid) {
    grid-template-columns: minmax(0, 1fr);
  }
  .department-master :deep(.search-panel__actions) {
    grid-column: 1;
  }
  .department-master :deep(.el-table__cell) {
    padding: 6px 0;
  }
  .department-master :deep(.el-table .el-table__row td .cell) {
    line-height: 24px;
  }
  .department-choice {
    padding: 0;
    border: 0;
    background: transparent;
    color: var(--app-text-primary);
    font: inherit;
    font-weight: 500;
    text-align: left;
    cursor: pointer;
  }
  .department-choice:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 2px;
  }
  .department-choice[aria-pressed='true'] {
    color: var(--el-color-primary);
  }
  .department-layout.is-compact .department-panel {
    flex: 1;
    width: 100%;
    min-width: 0;
  }
</style>
