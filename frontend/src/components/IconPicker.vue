<template>
  <el-popover v-model:visible="visible" trigger="click" placement="bottom-start" :width="panelWidth" :popper-style="{ maxWidth: 'calc(100vw - 24px)' }" :disabled="disabled">
    <template #reference>
      <slot :icon="model" :open="open" :close="close" :disabled="disabled">
        <el-button v-if="iconOnly" circle :disabled="disabled" :title="'选择图标'" :aria-label="'选择图标'" :aria-expanded="visible" aria-haspopup="dialog">
          <AppIcon :icon="model || 'lucide:smile'" :size="iconSize" />
        </el-button>
        <el-input v-else v-model="model" class="icon-picker-input" :style="{ width }" :disabled="disabled" :placeholder="placeholder || '选择图标'" :aria-label="'选择图标'" :aria-expanded="visible" aria-haspopup="dialog" @change="emit('change', model)" @keydown.esc="close">
          <template #suffix><span class="icon-picker-preview"><AppIcon :icon="model || 'lucide:smile'" :size="iconSize" /></span></template>
        </el-input>
      </slot>
    </template>
    <div class="icon-picker-panel" role="dialog" :aria-label="'选择图标'" :aria-busy="loading" @keydown.esc.stop="close">
      <div class="icon-picker-filters">
        <el-select v-model="collection" :teleported="false" :aria-label="'图标集合'" @change="loadNames">
          <el-option v-for="item in availableCollections" :key="item.prefix" :label="item.label" :value="item.prefix" />
        </el-select>
        <el-input v-model="search" clearable :placeholder="'搜索图标'" :aria-label="'搜索图标'" />
      </div>
      <div class="icon-picker-results">
        <p v-if="loading" class="icon-picker-message" role="status">{{ '图标加载中' }}</p>
        <div v-else-if="error" class="icon-picker-message" role="alert">
          <p>{{ '图标加载失败' }}</p>
          <el-button @click="loadNames">{{ '刷新' }}</el-button>
        </div>
        <p v-else-if="!filtered.length" class="icon-picker-message" role="status">{{ '暂无图标' }}</p>
        <div v-else class="icon-picker-grid">
          <el-tooltip v-for="name in pageIcons" :key="name" :content="name" placement="top" :show-after="300">
            <button type="button" class="icon-picker-item" :class="{ 'is-selected': model === name }" :title="name" :aria-label="name" :aria-pressed="model === name" @click="select(name)">
              <AppIcon :icon="name" :size="iconSize" />
            </button>
          </el-tooltip>
        </div>
      </div>
      <div class="icon-picker-footer">
        <el-button v-if="clearable" text :disabled="!model" @click="select('')">{{ '清除图标' }}</el-button>
        <el-pagination v-model:current-page="page" small layout="prev, pager, next" :pager-count="5" :page-size="safePageSize" :total="filtered.length" />
      </div>
      <div v-if="model" class="icon-picker-selected"><AppIcon :icon="model" /><span>{{ model }}</span></div>
    </div>
  </el-popover>
</template>
<script setup lang="ts">
  /**
   * 图标选择器组件。
   */
  import { computed, onBeforeUnmount, ref, watch } from 'vue'
  import AppIcon from './AppIcon.vue'
  import { getIconNames, iconCollections } from '@/services/icons/iconRegistry'
  const model = defineModel<string>({ default: '' })
  const props = withDefaults(defineProps<{
    /** Redis 或日志队列当前运行模式。 */
    mode?: 'all' | 'iconify' | 'svg'
    /** 当前组件对应的展示尺寸。 */
    width?: string
    /** 当前组件对应的展示尺寸。 */
    panelWidth?: number
    /** 每页显示的条目数。 */
    pageSize?: number
    /** 当前组件对应的展示尺寸。 */
    iconSize?: number
    /** 是否仅显示图标。 */
    iconOnly?: boolean
    /** 当前控件或选项是否禁止操作。 */
    disabled?: boolean
    /** 是否允许清空当前值。 */
    clearable?: boolean
    /** 输入为空时显示的提示。 */
    placeholder?: string
  }>(), { mode: 'all', width: '100%', panelWidth: 480, pageSize: 96, iconSize: 20, iconOnly: false, disabled: false, clearable: true, placeholder: '' })
  const emit = defineEmits<{
    /** 当前值发生变化时的更新函数。 */
    change: [icon: string]
    /** 将选中的图标标识通知给父组件。 */
    select: [icon: string]
  }>()
  const visible = ref(false)
  const collection = ref('lucide')
  const search = ref('')
  const names = ref<string[]>([])
  const loading = ref(false)
  const error = ref(false)
  const page = ref(1)
  const availableCollections = computed(() => [
    ...(props.mode === 'svg' ? [] : iconCollections),
    ...(props.mode === 'iconify' ? [] : [{ prefix: 'svg', label: '本地 SVG' }])
  ])
  const safePageSize = computed(() => Number.isFinite(props.pageSize) ? Math.max(1, Math.floor(props.pageSize)) : 96)
  const filtered = computed(() => {
    const query = search.value.trim().toLowerCase()
    return query ? names.value.filter(name => name.includes(query)) : names.value
  })
  const pageIcons = computed(() => filtered.value.slice((page.value - 1) * safePageSize.value, page.value * safePageSize.value))
  let revision = 0
  watch([search, collection, safePageSize], () => {
    page.value = 1
  })
  watch(collection, () => {
    search.value = ''
  })
  watch(() => [props.mode, model.value], () => {
    const prefix = model.value.split(':')[0]
    const next = availableCollections.value.find(item => item.prefix === prefix)?.prefix
      ?? availableCollections.value.find(item => item.prefix === collection.value)?.prefix
      ?? availableCollections.value[0].prefix
    if (next !== collection.value) {
      collection.value = next
      if (visible.value) void loadNames()
    }
  }, { immediate: true })
  watch(() => props.disabled, disabled => {
    if (disabled) close()
  })
  watch(visible, value => {
    if (value) void loadNames()
  })
  /**
   * 读取可选择的资源或业务对象名称。
   */
  async function loadNames() {
    const ticket = ++revision
    loading.value = true
    error.value = false
    names.value = []
    page.value = 1
    try {
      const result = await getIconNames(collection.value)
      if (ticket === revision) names.value = result
    } catch {
      if (ticket === revision) error.value = true
    }
    finally {
      if (ticket === revision) loading.value = false
    }
  }
  /**
   * 将选中的图标选择器项目应用到当前状态。
   * @param icon 当前菜单或控件的图标标识。
   */
  function select(icon: string) {
    if (props.disabled) return
    model.value = icon
    emit('change', icon)
    emit('select', icon)
    close()
  }
  /**
   * 打开图标选择器的目标视图或编辑表单。
   */
  function open() {
    if (!props.disabled) visible.value = true
  }
  /**
   * 关闭图标选择器当前显示的视图。
   */
  function close() {
    visible.value = false
  }
  defineExpose({ open, close })
  onBeforeUnmount(() => {
    revision++
  })
</script>
<style scoped>
  .icon-picker-input :deep(.el-input__wrapper) { min-width: 0; }
  .icon-picker-input :deep(.el-input__inner) { min-width: 0; }
  .icon-picker-preview { display: inline-flex; align-items: center; justify-content: center; align-self: stretch; width: 51px; margin-right: -11px; box-sizing: border-box; border-left: 1px solid var(--el-border-color); }
  .icon-picker-panel { color: var(--el-text-color-primary); }
  .icon-picker-filters { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); gap: 8px; margin-bottom: 12px; }
  .icon-picker-results { height: 280px; overflow: auto; }
  .icon-picker-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(36px, 1fr)); gap: 6px; padding: 2px; }
  .icon-picker-item { display: flex; align-items: center; justify-content: center; min-height: 36px; padding: 6px; border: 1px solid var(--el-border-color-lighter); border-radius: var(--el-border-radius-base); color: inherit; background: var(--el-bg-color); cursor: pointer; }
  .icon-picker-item:hover, .icon-picker-item.is-selected { color: var(--el-color-primary); border-color: var(--el-color-primary); background: var(--el-color-primary-light-9); }
  .icon-picker-item:focus-visible { outline: 2px solid var(--el-color-primary); outline-offset: 1px; }
  .icon-picker-message { display: grid; place-content: center; height: 100%; margin: 0; text-align: center; color: var(--el-text-color-secondary); }
  .icon-picker-footer { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 6px; margin-top: 12px; }
  .icon-picker-selected { display: flex; align-items: center; gap: 8px; margin-top: 8px; font-size: 12px; }
  .icon-picker-selected span { overflow: hidden; white-space: nowrap; text-overflow: ellipsis; }
</style>
