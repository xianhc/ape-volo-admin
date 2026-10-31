<template>
  <div class="crud-toolbar-actions">
    <slot name="before" />
    <el-button
      v-if="showCreate && permissions.create"
      v-has-perm="permissions.create"
      type="primary"
      :icon="createIconifyIcon('lucide:plus')"
      :disabled="submitting"
      @click="emit('create')"
    >
      {{ '新增' }}
    </el-button>
    <el-button
      v-if="showRemove && permissions.remove"
      v-has-perm="permissions.remove"
      type="danger"
      :icon="createIconifyIcon('lucide:trash-2')"
      :loading="submitting"
      :disabled="submitting || selectedCount === 0"
      @click="emit('remove')"
    >
      {{ '删除' }}
    </el-button>
    <el-button
      v-if="showExport && permissions.export"
      v-has-perm="permissions.export"
      type="warning"
      :icon="createIconifyIcon('lucide:download')"
      :loading="exporting"
      :disabled="exporting"
      @click="emit('export')"
    >
      {{ '导出' }}
    </el-button>
    <slot />
  </div>
</template>

<script setup lang="ts">
  /**
   * 表格工具栏按钮组件。
   */
  import { createIconifyIcon } from '@/components/icons'
  /**
   * 按钮是否可用所需的权限编码或判断条件。
   */
  type Permission = string | string[] | undefined

  defineOptions({ name: 'CrudToolbarActions' })
  withDefaults(
    defineProps<{
      /** 当前操作按钮所需的权限配置。 */
      permissions?: {
        /** 创建业务记录的操作。 */
        create?: Permission
        /** 删除业务记录的操作。 */
        remove?: Permission
        /** 当前功能的文件导出操作。 */
        export?: Permission
      }
      /** 是否显示对应的新增、删除或导出入口。 */
      showCreate?: boolean
      /** 是否显示对应的新增、删除或导出入口。 */
      showRemove?: boolean
      /** 是否显示对应的新增、删除或导出入口。 */
      showExport?: boolean
      /** 当前选中记录数。 */
      selectedCount?: number
      /** 是否正在提交写入请求。 */
      submitting?: boolean
      /** 是否正在导出文件。 */
      exporting?: boolean
    }>(),
    {
      /**
       * 读取当前操作按钮所需的权限配置。
       * @returns 当前组件默认使用的空权限配置。
       */
      permissions: () => ({}),
      showCreate: true,
      showRemove: true,
      showExport: true,
      selectedCount: 0,
      submitting: false,
      exporting: false
    }
  )
  const emit = defineEmits<{
    /** 创建业务记录的操作。 */
    create: []
    /** 删除业务记录的操作。 */
    remove: []
    /** 当前功能的文件导出操作。 */
    export: []
  }>()
</script>

<style scoped>
  .crud-toolbar-actions {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: var(--app-space-2);
  }

  .crud-toolbar-actions :deep(.el-button + .el-button) {
    margin-left: 0;
  }
</style>
