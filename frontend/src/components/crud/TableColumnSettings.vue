<template>
  <el-popover ref="popover" trigger="click" :width="260" placement="bottom-end" popper-class="table-column-settings">
    <template #reference>
      <el-button ref="trigger" :icon="createIconifyIcon('lucide:settings')" circle :title="'列设置'" :aria-label="'列设置'" />
    </template>
    <div @keydown.esc.stop="close">
    <div class="table-column-settings__list">
      <div
        v-for="item in ordered" :key="item.preference.key" class="table-column-settings__item"
        @dragover.prevent="onDragOver" @drop.prevent="drop(item.column.key)"
      >
        <span
          class="table-column-settings__handle" :draggable="movable(item.column)"
          :aria-label="`拖动 ${item.column.label}`" :class="{ 'is-locked': !movable(item.column) }"
          @dragstart="startDrag($event, item.column.key)" @dragend="dragged = ''"
        >⠿</span>
        <el-checkbox
          :model-value="item.preference.visible" :disabled="cannotHide(item.column.key)"
          :title="cannotHide(item.column.key) ? '必需列或最后一列业务数据不可隐藏' : ''"
          @change="setVisible(item.column.key, $event)"
        >
          {{ item.column.label }}
        </el-checkbox>
        <div v-if="movable(item.column)" class="table-column-settings__move">
          <el-button link :icon="createIconifyIcon('lucide:chevron-up')" :aria-label="`上移 ${item.column.label}`" :disabled="!neighbor(item.column.key, -1)" @click="shift(item.column.key, -1)" />
          <el-button link :icon="createIconifyIcon('lucide:chevron-down')" :aria-label="`下移 ${item.column.label}`" :disabled="!neighbor(item.column.key, 1)" @click="shift(item.column.key, 1)" />
        </div>
      </div>
    </div>
    <el-button class="table-column-settings__reset" link type="primary" @click="emit('reset')">{{ '恢复默认' }}</el-button>
    </div>
  </el-popover>
</template>
<script setup lang="ts" generic="T extends object">
  /**
   * 表格列显示设置组件。
   */
  import { createIconifyIcon } from '@/components/icons'
  import { computed, ref, nextTick, onDeactivated } from 'vue'
  import type { ButtonInstance, PopoverInstance } from 'element-plus'
  import { moveColumn, setColumnVisibility, type TableColumnSpec, type ColumnPreference } from './tableColumns'
  const props = defineProps<{
    /** 表格展示使用的业务列定义集合。 */
    columns: readonly TableColumnSpec<T>[]
    /** 由父组件持有的受控绑定值。 */
    modelValue: ColumnPreference[]
  }>()
  const emit = defineEmits<{
    /** 将新的绑定值通知给父组件。 */
    'update:modelValue': [value: ColumnPreference[]]
    /** 通知父组件恢复默认列或筛选配置。 */
    reset: []
  }>()
  const dragged = ref('')
  const popover = ref<PopoverInstance>()
  const trigger = ref<ButtonInstance>()
  /**
   * 关闭表格当前显示的视图。
   */
  function close() {
    popover.value?.hide()
    void nextTick(() => trigger.value?.ref?.focus())
  }
  onDeactivated(() => {
    popover.value?.hide()
    dragged.value = ''
  })
  const ordered = computed(() => props.modelValue.flatMap(preference =>
    props.columns.filter(c => c.key === preference.key).map(column => ({ column, preference }))))
  /**
   * 判断当前项目是否允许参与排序移动。
   * @param c 当前需要格式化的颜色或条目。
   * @returns 当前项目是否允许参与排序移动时返回 true，否则返回 false。
   */
  const movable = (c: TableColumnSpec<T>) => !c.placement || c.placement === 'normal'
  /**
   * 判断隐藏当前列是否会违反至少保留一列的约束。
   * @param key 当前查询、映射或缓存使用的键。
   * @returns 隐藏当前列是否会违反至少保留一列的约束时返回 true，否则返回 false。
   */
  const cannotHide = (key: string) => props.modelValue.find(c => c.key === key)?.visible === true &&
    setColumnVisibility(props.columns, props.modelValue, key, false).find(c => c.key === key)?.visible === true
  /**
   * 更新弹层受控可见状态。
   * @param key 当前查询、映射或缓存使用的键。
   * @param value 本次转换、校验或更新所接收的值。
   */
  function setVisible(key: string, value: string | number | boolean) {
    emit('update:modelValue', setColumnVisibility(props.columns, props.modelValue, key, value === true))
  }
  /**
   * 取得当前位置在指定方向上的相邻项。
   * @param key 当前查询、映射或缓存使用的键。
   * @param delta 相对当前位置的偏移量。
   * @returns 指定偏移位置的相邻列键；超出集合时为 undefined。
   */
  function neighbor(key: string, delta: number) {
    const list = ordered.value.filter(i => movable(i.column))
    const index = list.findIndex(i => i.column.key === key)
    return list[index + delta]?.column.key
  }
  /**
   * 按偏移量调整当前选择或可见区域。
   * @param key 当前查询、映射或缓存使用的键。
   * @param delta 相对当前位置的偏移量。
   */
  function shift(key: string, delta: number) {
    const target = neighbor(key, delta)
    if (target) emit('update:modelValue', moveColumn(props.columns, props.modelValue, key, target))
  }
  /**
   * 记录拖动起点与当前被拖动的目标。
   * @param event 触发当前交互的事件。
   * @param key 当前查询、映射或缓存使用的键。
   */
  function startDrag(event: DragEvent, key: string) {
    dragged.value = key
    event.dataTransfer?.setData('text/plain', key)
    if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move'
  }
  /**
   * 提交当前拖动得到的顺序变化。
   * @param target 本次处理的目标对象。
   */
  function drop(target: string) {
    if (dragged.value) emit('update:modelValue', moveColumn(props.columns, props.modelValue, dragged.value, target))
    dragged.value = ''
  }
  /**
   * 在拖动经过目标时更新候选位置。
   * @param event 触发当前交互的事件。
   */
  function onDragOver(event: DragEvent) {
    const list = event.currentTarget instanceof Element ? event.currentTarget.parentElement : null
    if (!list) return
    const bounds = list.getBoundingClientRect()
    if (event.clientY < bounds.top + 36) list.scrollTop -= 16
    else if (event.clientY > bounds.bottom - 36) list.scrollTop += 16
  }
</script>
<style scoped>
  .table-column-settings__list { max-height: min(320px, 50dvh); overflow-y: auto; }
  .table-column-settings__item { display: flex; align-items: center; gap: 8px; min-height: 38px; }
  .table-column-settings__handle { cursor: grab; color: var(--el-text-color-secondary); font-size: 20px; }
  .table-column-settings__handle.is-locked { cursor: default; opacity: .4; }
  .table-column-settings__item :deep(.el-checkbox) { flex: 1; min-width: 0; margin-right: 0; }
  .table-column-settings__move { display: flex; flex: none; gap: 2px; }
  .table-column-settings__move :deep(.el-button + .el-button) { margin-left: 0; }
  .table-column-settings__reset { margin-top: 8px; }
</style>
