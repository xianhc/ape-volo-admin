<template>
  <span class="app-icon" :class="{ 'app-icon--spin': spin, 'app-icon--loading': loading }" :style="style" :role="title ? 'img' : undefined" :aria-label="title || undefined" :aria-hidden="title ? undefined : true" :aria-busy="loading || undefined" :data-icon="icon || undefined">
    <Icon v-if="data" :icon="data" width="100%" height="100%" />
    <svg v-else-if="failed" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="12" cy="12" r="9" /><path d="M9 9a3 3 0 0 1 6 0c0 2-3 2-3 4m0 3v1" /></svg>
  </span>
</template>
<script setup lang="ts">
  /**
   * 图标组件：按名称渲染 Iconify 图标。
   */
  import { computed, onBeforeUnmount, shallowRef, ref, watch } from 'vue'
  import { Icon } from '@iconify/vue'
  import type { IconifyIcon } from '@iconify/vue'
  import { resolveIcon } from '@/services/icons/iconRegistry'
  const props = withDefaults(defineProps<{
    /** 当前菜单或控件的图标标识。 */
    icon?: string | null
    /** 图标尺寸；数值或纯数字文本按像素处理，其他文本保留 CSS 单位。 */
    size?: number | string
    /** 当前控件或选项使用的颜色。 */
    color?: string
    /** 图标是否持续旋转。 */
    spin?: boolean
    /** 当前内容或界面的展示标题。 */
    title?: string
  }>(), { icon: '', size: undefined, color: undefined, spin: false, title: '' })
  const data = shallowRef<IconifyIcon>()
  const loading = ref(false)
  const failed = ref(false)
  const emit = defineEmits<{
    /** 图标加载成功后通知父组件，载荷为图标名称。 */
    load: [name: string]
    /** 图标加载失败后通知父组件，载荷为图标名称。 */
    error: [name: string]
  }>()
  const style = computed(() => {
    let fontSize: string | undefined
    if (props.size === undefined) {
      fontSize = undefined
    } else {
      if (typeof props.size === 'number' || /^\d+$/.test(props.size)) {
        fontSize = `${props.size}px`
      } else {
        fontSize = props.size
      }
    }
    return { fontSize: fontSize, color: props.color }
  })
  let revision = 0
  watch(() => props.icon, async name => {
    const ticket = ++revision
    data.value = undefined
    failed.value = false
    loading.value = Boolean(name)
    if (!name) return
    try {
      const result = await resolveIcon(name)
      if (ticket !== revision) return
      data.value = result
      emit('load', name)
    } catch {
      if (ticket !== revision) return
      failed.value = true
      emit('error', name)
    } finally {
      if (ticket === revision) loading.value = false
    }
  }, { immediate: true })
  onBeforeUnmount(() => {
    revision++
  })
</script>
<style scoped>
  .app-icon { display: inline-flex; flex: none; align-items: center; justify-content: center; width: 1em; height: 1em; vertical-align: -0.125em; line-height: 1; }
  .app-icon > svg { display: block; width: 100%; height: 100%; }
  .app-icon--spin { animation: app-icon-spin 1s linear infinite; }
  .app-icon--loading::before { content: ''; width: 45%; height: 45%; border-radius: 50%; background: currentColor; opacity: 0.2; }
  @keyframes app-icon-spin { to { transform: rotate(360deg); } }
  @media (prefers-reduced-motion: reduce) { .app-icon--spin { animation: none; } }
</style>
