<template>
  <TablePage
    v-model:search-visible="searchVisible"
    :title="`${departmentName} · 部门用户`"
    :data="data"
    :columns="columns"
    :loading="loading"
    :error="error"
    row-key="id"
    @refresh="refresh"
    @sort-change="onSortChange"
  >
    <template #search>
      <SearchPanel :model="query" :loading="loading" @query="search" @reset="resetQuery">
        <el-form-item label="用户名">
          <el-input v-model="query.username" clearable placeholder="请输入" />
        </el-form-item>
        <el-form-item label="昵称">
          <el-input v-model="query.nickname" clearable placeholder="请输入" />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="query.enabled" clearable placeholder="请选择">
            <el-option
              v-for="option in statusOptions"
              :key="option.value"
              :label="option.label"
              :value="String(option.value) === 'true'"
            />
          </el-select>
        </el-form-item>
        <template #advanced>
          <DateRangePicker v-model="query" />
        </template>
      </SearchPanel>
    </template>
    <template #column-genderCode="{ row }">
      {{ showDictLabel(genderOptions, row.genderCode) || '—' }}
    </template>
    <template #column-enabled="{ row }">
      <el-tag :type="row.enabled ? 'success' : 'info'">
        {{ showDictLabel(statusOptions, String(row.enabled)) || (row.enabled ? '启用' : '禁用') }}
      </el-tag>
    </template>
    <template #pagination>
      <el-pagination v-bind="pagination" />
    </template>
  </TablePage>
</template>

<script setup lang="ts">
  import { ref } from 'vue'
  import { get } from '@/api/identity/user'
  import type { User, UserQueryParams } from '@/api/identity/types/user.types'
  import TablePage from '@/components/crud/TablePage.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useTableController } from '@/composables/useTableController'
  import { showDictLabel, useDictionary } from '@/composables/useDictionary'

  const props = defineProps<{
    departmentId: string
    departmentName: string
  }>()
  const { options: genderOptions } = useDictionary('gender_code')
  const { options: statusOptions } = useDictionary('status_type')
  const defaults = (): UserQueryParams => ({})
  const query = ref<UserQueryParams>(defaults())
  const { data, loading, error, pagination, refresh, search, resetQuery, onSortChange, searchVisible } = useTableController<UserQueryParams, User>({
    list: (params, options) => get({ ...params, departmentIdArray: [props.departmentId] }, options),
    query,
    defaults
  })

  const columns: TableColumnSpec<User>[] = [
    { key: 'userName', label: '用户名', prop: 'userName', sortable: 'custom', minWidth: 130, required: true },
    { key: 'nickName', label: '昵称', prop: 'nickName', sortable: 'custom', minWidth: 120 },
    { key: 'email', label: '邮箱', prop: 'email', sortable: 'custom', minWidth: 190 },
    { key: 'phone', label: '电话', prop: 'phone', sortable: 'custom', minWidth: 140 },
    { key: 'genderCode', label: '性别', prop: 'genderCode', width: 80 },
    { key: 'enabled', label: '状态', prop: 'enabled', width: 90 },
    { key: 'creationTime', label: '创建时间', prop: 'creationTime', sortable: 'custom', minWidth: 170 }
  ]
</script>
