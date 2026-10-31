<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="菜单管理"
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
          <el-form-item label="菜单标题">
            <el-input
              v-model="searchInfo.title"
              clearable
              style="width: 200px"
              placeholder="请输入"
            />
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
      <template #column-title="scope">
        <span>{{ scope.row.title }}</span>
        <AppIcon
          v-if="scope.row.icon"
          :icon="String(scope.row.icon)"
          class="menu-title-icon"
        />
      </template>
      <template #column-enabled="scope">
        <el-switch
          v-model="scope.row.enabled"
          inline-prompt
          :disabled="true"
          :active-text="showDictLabel(statusTypeOption, 'true')"
          :inactive-text="showDictLabel(statusTypeOption, 'false')"
        />
      </template>
      <template #column-keepAlive="scope">
        <el-switch v-model="scope.row.keepAlive" :disabled="true" />
      </template>
      <template #column-hidden="scope">
        <el-switch v-model="scope.row.hidden" :disabled="true" />
      </template>
      <template #column-actions="scope">
        <RowActions>
          <el-button
            v-show="scope.row.menuType !== 3"
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
    <formPanel
      :status-type-option="statusTypeOption"
      :menu-type-option="menuTypeOption"
    />
  </div>
</template>

<script setup lang="ts">
  import type { Menu } from '@/api/permission/types/menu.types'

  import { computed, ref } from 'vue'
  import { add, edit, del, get, download } from '@/api/permission/menu'
  import type { MenuQueryParams } from '@/api/permission/types/menu.types'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import AppIcon from '@/components/AppIcon.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useCrud } from '@/components/crud/UseCrud'
  import { getDict, showDictLabel, type DictOption } from '@/composables/useDictionary'
  import formPanel from './module/formPanel.vue'

  defineOptions({
    name: 'Menus'
  })

  const perms = {
    add: ['sys:menu:add'],
    edit: ['sys:menu:edit'],
    del: ['sys:menu:del'],
    download: ['sys:menu:download']
  }

  // 懒加载树表的根列表只取顶级菜单（后端 parent_id IS NULL）；
  // 不传 isRoot 也不传 parentId 时后端按「未参与过滤」处理，会把所有层级平铺返回。
  const searchInfo = ref<MenuQueryParams>({ isRoot: true })
  // 菜单类型
  const menuTypeOption = ref<DictOption[]>([])
  // 状态
  const statusTypeOption = ref<DictOption[]>([])

  // 格式化菜单类型
  const formatMenuType = (_row: any, _column: any, cellValue: any) => {
    return showDictLabel(menuTypeOption.value, cellValue)
  }

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
      title: undefined,
      sort: 999,
      path: undefined,
      component: undefined,
      componentName: undefined,
      // null 表示顶级菜单
      parentId: null,
      icon: '',
      keepAlive: false,
      hidden: false,
      menuType: 2,
      enabled: true,
      authCode: undefined
    }),
    searchInfo
  })
  const {
    data,
    loading,
    searchToggle,
    refresh,
    resetQuery,
    onSelectionChange,
    onSortChange,
    pagination
  } = crud

  const tableColumns = computed<TableColumnSpec<Menu>[]>(
    () => [
      {
        key: 'title',
        label: '菜单标题',
        prop: 'title',
        sortable: 'custom',
        minWidth: 200,
        align: 'left'
      },
      {
        key: 'menuType',
        label: '类型',
        prop: 'menuType',
        sortable: 'custom',
        width: 110,
        align: 'left',
        formatter: formatMenuType
      },
      {
        key: 'path',
        label: '路由地址',
        prop: 'path',
        sortable: 'custom',
        minWidth: 160,
        align: 'left'
      },
      {
        key: 'component',
        label: '页面组件',
        prop: 'component',
        sortable: 'custom',
        minWidth: 180,
        align: 'left'
      },
      {
        key: 'componentName',
        label: '组件名称',
        prop: 'componentName',
        sortable: 'custom',
        minWidth: 150,
        align: 'left'
      },
      {
        key: 'authCode',
        label: '权限标识',
        prop: 'authCode',
        sortable: 'custom',
        minWidth: 160,
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
        key: 'keepAlive',
        label: '是否缓存',
        prop: 'keepAlive',
        sortable: 'custom',
        width: 110,
        align: 'left'
      },
      {
        key: 'hidden',
        label: '是否隐藏',
        prop: 'hidden',
        sortable: 'custom',
        width: 110,
        align: 'left'
      },
      {
        key: 'sort',
        label: '排序',
        prop: 'sort',
        sortable: 'custom',
        width: 100,
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
      { key: 'actions', label: '操作', width: 280, align: 'left' }
    ]
  )

  const init = async () => {
    menuTypeOption.value = await getDict('menu_type')
    statusTypeOption.value = await getDict('status_type')
  }

  init()

  function GetChildren(tree: any, _treeNode: any, resolve: any) {
    const params = { parentId: tree.id }
    setTimeout(() => {
      get(params).then((res) => {
        resolve(res.data?.items ?? res.data)
      })
    }, 200)
  }
</script>

<style lang="scss">
  .menu-title-icon {
    margin-left: 8px;
    vertical-align: middle;
  }
</style>
