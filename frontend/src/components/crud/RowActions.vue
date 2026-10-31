<template>
  <div ref="root" class="table-row-actions">
    <slot name="before" />
    <el-button
      v-if="row && edit && permissions.edit"
      v-has-perm="permissions.edit"
      type="primary"
      link
      :icon="createIconifyIcon('lucide:square-pen')"
      :disabled="busy || disableEdit"
      @click="edit(row)"
    >
      {{ '编辑' }}
    </el-button>
    <el-popconfirm
      v-if="row && remove && permissions.remove"
      :title="removeTitle"
      :confirm-button-text="'确定'"
      :cancel-button-text="'取消'"
      @confirm="remove(row)"
    >
      <template #reference>
        <el-button
          v-has-perm="permissions.remove"
          type="danger"
          link
          :icon="createIconifyIcon('lucide:trash-2')"
          :disabled="busy || disableRemove"
        >
          {{ '删除' }}
        </el-button>
      </template>
    </el-popconfirm>
    <slot />
    <slot name="after" />
  </div>
</template>
<script setup lang="ts" generic="T extends { id: EntityId }">
  /**
   * 表格行操作按钮组件。
   */
  import { createIconifyIcon } from '@/components/icons'
  import {
    inject,
    ref,
    onMounted,
    onBeforeUnmount,
    onActivated,
    onDeactivated,
    nextTick
  } from 'vue'
  import { computed } from 'vue'
  import type { EntityId } from '@/api/types/base'
  import { rowActionMeasurementsKey } from './tableContext'
  /**
   * 按钮是否可用所需的权限编码或判断条件。
   */
  type Permission = string | string[] | undefined
  const props = withDefaults(
    defineProps<{
      /** 当前记录或响应内容。 */
      row?: T
      /** 面向用户显示的标签。 */
      label?: string
      /** 当前操作按钮所需的权限配置。 */
      permissions?: {
        /** 当前编辑入口或编辑配置。 */
        edit?: Permission
        /** 删除业务记录的操作。 */
        remove?: Permission
      }
      /**
       *  当前编辑入口或编辑配置。
       * @param row 本次查看、编辑或写入所对应的业务记录。
       */
      edit?: (row: T) => void
      /**
       *  删除业务记录的操作。
       * @param row 本次查看、编辑或写入所对应的业务记录。
       */
      remove?: (row: T) => void | Promise<void>
      /** 当前操作是否仍未结束。 */
      busy?: boolean
      /** 是否禁止当前记录的编辑或删除。 */
      disableEdit?: boolean
      /** 是否禁止当前记录的编辑或删除。 */
      disableRemove?: boolean
    }>(),
    {
      row: undefined,
      label: '',
      /**
       * 读取当前操作按钮所需的权限配置。
       * @returns 当前组件默认使用的空权限配置。
       */
      permissions: () => ({}),
      edit: undefined,
      remove: undefined,
      busy: false,
      disableEdit: false,
      disableRemove: false
    }
  )
  const removeTitle = computed(() =>
    props.label
      ? '确认删除？'
      : '确认删除该记录？'
  )
  const root = ref<HTMLElement>()
  const measurements = inject(rowActionMeasurementsKey, undefined)
  const id = Symbol('row')
  let observer: ResizeObserver | undefined
  /**
   * 测量当前元素的实际展示宽度。
   */
  const measure = () => {
    if (root.value?.isConnected && root.value.getClientRects().length) {
      measurements?.report(
        id,
        Math.ceil(root.value.getBoundingClientRect().width) + 24
      )
    }
  }
  /**
   * 注册表格所需的状态变化监听。
   */
  function observe() {
    observer?.disconnect()
    if (root.value && measurements) {
      observer = new ResizeObserver(measure)
      observer.observe(root.value)
      measure()
    }
  }
  onMounted(observe)
  onActivated(async () => {
    await nextTick()
    observe()
  })
  onDeactivated(() => {
    observer?.disconnect()
    measurements?.remove(id)
  })
  onBeforeUnmount(() => {
    observer?.disconnect()
    measurements?.remove(id)
  })
</script>
<style scoped>
  .table-row-actions {
    display: inline-flex;
    align-items: center;
    flex-wrap: nowrap;
    gap: 8px;
    width: max-content;
    white-space: nowrap;
  }
  .table-row-actions > :deep(*) {
    flex: none;
  }
  .table-row-actions :deep(.el-button + .el-button) {
    margin-left: 0;
  }
</style>
