<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="系统配置"
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
        <SearchPanel :model="searchInfo" :loading="loading" @query="refresh" @reset="resetQuery">
          <el-form-item label="配置名称">
            <el-input v-model="searchInfo.name" placeholder="请输入" />
          </el-form-item>
          <el-form-item label="数据值">
            <el-input v-model="searchInfo.value" placeholder="请输入" />
          </el-form-item>
          <template #advanced>
            <el-form-item label="描述">
              <el-input v-model="searchInfo.description" placeholder="请输入" />
            </el-form-item>
            <el-form-item label="状态">
              <el-select v-model="searchInfo.enabled" clearable placeholder="请选择">
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
  import type { Setting } from '@/api/system/types/setting.types'

  import { computed, reactive, ref } from 'vue'
  import { ElMessage, ElMessageBox } from 'element-plus'
  import { del, edit, get, add, download } from '@/api/system/setting'
  import type { SettingQueryParams } from '@/api/system/types/setting.types'
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
    name: 'Setting'
  })

  const perms = {
    add: ['sys:setting:add'],
    edit: ['sys:setting:edit'],
    del: ['sys:setting:del'],
    download: ['sys:setting:download']
  }

  const searchInfo = ref<SettingQueryParams>({})

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
      name: '',
      value: '',
      description: '',
      enabled: true
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

  const tableColumns = computed<TableColumnSpec<Setting>[]>(() => [
    { key: 'selection', label: '', type: 'selection', width: 55 },
    { key: 'name', label: '配置名称', prop: 'name', sortable: 'custom', minWidth: 160 },
    { key: 'value', label: '数据值', prop: 'value', sortable: 'custom', minWidth: 200, showOverflowTooltip: true },
    { key: 'enabled', label: '状态', prop: 'enabled', sortable: 'custom', width: 110 },
    { key: 'description', label: '描述', prop: 'description', minWidth: 200, showOverflowTooltip: true },
    { key: 'creationTime', label: '创建时间', prop: 'creationTime', sortable: 'custom', minWidth: 170 },
    { key: 'actions', label: '操作', width: 180 }
  ])

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
