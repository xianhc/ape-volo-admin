<template>
  <div class="toolbar-order-list">
    <div
      v-for="item in modelValue"
      :key="item"
      class="toolbar-order-item"
      :class="{ 'is-dragging': dragging === item }"
      @dragover.prevent="allowDrop($event)"
      @drop.prevent="drop(item)"
      @dragend="dragging = undefined"
    >
      <button
        type="button" class="toolbar-order-handle" draggable="true"
        :aria-label="'工具位置' + ': ' + TOOLBAR_ITEMS[item].label"
        @dragstart="startDrag(item, $event)" @keydown.up.prevent="move(item, -1)" @keydown.down.prevent="move(item, 1)">
        <AppIcon icon="lucide:grip-vertical" />
      </button>
      <span class="toolbar-order-label">{{ TOOLBAR_ITEMS[item].label }}</span>
      <el-select :model-value="placements[item]" :aria-label="TOOLBAR_ITEMS[item].label" @update:model-value="setPlacement(item, $event)">
        <el-option :label="'顶部栏'" value="header" />
        <el-option :label="'用户下拉菜单'" value="userDropdown" />
        <el-option :label="'不显示'" value="hidden" :disabled="item === 'preferences'" />
      </el-select>
    </div>
  </div>
</template>

<script setup lang="ts">
  /**
   * 工具栏项排序编辑组件。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { TOOLBAR_ITEMS } from '@/services/appearance/toolbar'
  import type { ToolbarItemKey, ToolbarPosition } from '@/services/appearance/preferences.types'
  import { ref } from 'vue'
  defineOptions({ name: 'ToolbarOrderEditor' })
  const props = defineProps<{
    /** 由父组件持有的受控绑定值。 */
    modelValue: ToolbarItemKey[]
    /** 工具项或浮层的展示位置。 */
    placements: Record<ToolbarItemKey, ToolbarPosition>
  }>()
  const emit = defineEmits<{
    /** 将新的绑定值通知给父组件。 */
    'update:modelValue': [value: ToolbarItemKey[]]
    /** 将工具栏位置变更通知给父组件。 */
    'update:placements': [value: Record<ToolbarItemKey, ToolbarPosition>]
  }>()
  const dragging = ref<ToolbarItemKey>()
  /**
   * 更新工具栏项目所在的位置。
   * @param item 当前待处理的候选项或记录。
   * @param position 项目准备放置的位置。
   */
  function setPlacement(item: ToolbarItemKey, position: ToolbarPosition): void {
    if (item === 'preferences' && position === 'hidden') return
    emit('update:placements', { ...props.placements, [item]: position })
  }

  /**
   * 按当前方向移动选择项、节点或标签。
   * @param item 当前待处理的候选项或记录。
   * @param direction 本次移动或选择的方向。
   */
  function move(item: ToolbarItemKey, direction: number): void {
    const next = [...props.modelValue]
    const index = next.indexOf(item)
    const target = index + direction
    if (target < 0 || target >= next.length) return
    next.splice(index, 1)
    next.splice(target, 0, item)
    emit('update:modelValue', next)
  }

  /**
   * 记录拖动起点与当前被拖动的目标。
   * @param item 当前待处理的候选项或记录。
   * @param event 触发当前交互的事件。
   */
  function startDrag(item: ToolbarItemKey, event: DragEvent): void {
    dragging.value = item
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move'
      event.dataTransfer.setData('text/plain', item)
    }
  }

  /**
   * 判断当前树节点是否允许拖放到指定位置。
   * @param event 触发当前交互的事件。
   */
  function allowDrop(event: DragEvent): void {
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move'
  }

  /**
   * 提交当前拖动得到的顺序变化。
   * @param target 本次处理的目标对象。
   */
  function drop(target: ToolbarItemKey): void {
    if (!dragging.value || dragging.value === target) return
    const next = [...props.modelValue]
    const sourceIndex = next.indexOf(dragging.value)
    const targetIndex = next.indexOf(target)
    if (sourceIndex < 0 || targetIndex < 0) return
    const [source] = next.splice(sourceIndex, 1)
    next.splice(next.indexOf(target), 0, source)
    emit('update:modelValue', next)
    dragging.value = undefined
  }
</script>

<style scoped>
  .toolbar-order-list { display: grid; gap: 8px; min-width: 0; }
  .toolbar-order-item { display: flex; align-items: center; gap: 10px; min-width: 0; min-height: 64px; padding: 10px 12px; border: 1px solid var(--app-border); border-radius: 10px; background: var(--app-surface-subtle); }
  .toolbar-order-item.is-dragging { opacity: 0.45; }
  .toolbar-order-handle { display: grid; place-items: center; flex: none; width: 20px; height: 32px; padding: 0; border: 0; border-radius: 4px; color: var(--app-text-muted); background: transparent; cursor: grab; }
  .toolbar-order-handle:active { cursor: grabbing; }
  .toolbar-order-handle:focus-visible { outline: 2px solid var(--el-color-primary); outline-offset: 2px; }
  .toolbar-order-label { flex: 1; min-width: 0; color: var(--app-text-primary); font-size: 14px; overflow-wrap: anywhere; }
  .toolbar-order-item :deep(.el-select) { flex: 0 0 46%; width: 46%; min-width: 0; }
  .toolbar-order-item :deep(.el-select__wrapper) { min-height: 38px; }
  @media (max-width: 400px) { .toolbar-order-item { gap: 6px; padding-inline: 8px; } .toolbar-order-label { font-size: 13px; } }
</style>
