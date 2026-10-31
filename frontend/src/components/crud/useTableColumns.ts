/**
 * 表格列显示偏好组合式函数。
 */
import { computed, ref, toValue, watch, type MaybeRefOrGetter } from 'vue'
import { createColumnPreferences, type TableColumnSpec, type ColumnPreference } from './tableColumns'

/**
 * 创建表格列显示与排序控制器。
 * @typeParam T 表格行数据类型。
 * @param source 响应式表格列定义或读取列定义的函数。
 * @returns 列偏好、当前可见列以及恢复默认配置的入口。
 */
export function useTableColumns<T extends object>(source: MaybeRefOrGetter<readonly TableColumnSpec<T>[]>) {
  const preferences = ref<ColumnPreference[]>(createColumnPreferences(toValue(source)))
  const visibleColumns = computed(() => preferences.value.filter(p => p.visible)
    .flatMap(p => toValue(source).filter(c => c.key === p.key)))
  /**
   * 恢复表格当前输入或状态的初始值。
   */
  const reset = () => {
    preferences.value = createColumnPreferences(toValue(source))
  }

  watch(() => toValue(source).map(c => `${c.key}:${c.placement}:${c.required}`).join('|'), () => {
    const defaults = createColumnPreferences(toValue(source))
    const normal = defaults.filter(p => {
      const column = toValue(source).find(c => c.key === p.key)
      return !column?.placement || column.placement === 'normal'
    })
    const ordered = preferences.value.filter(p => normal.some(c => c.key === p.key))
    const missing = normal.filter(p => !ordered.some(c => c.key === p.key))
    /**
     * 判断目标标签或侧栏是否处于固定状态。
     * @param placement 工具项或浮层的展示位置。
     * @returns 目标标签或侧栏是否处于固定状态时返回 true，否则返回 false。
     */
    const pinned = (placement: 'start' | 'end') => defaults.filter(p =>
      toValue(source).find(c => c.key === p.key)?.placement === placement)
    preferences.value = [...pinned('start'), ...ordered, ...missing, ...pinned('end')].map(p => {
      const column = toValue(source).find(c => c.key === p.key)
      const old = preferences.value.find(c => c.key === p.key)
      return { key: p.key, visible: column?.required ? true : old?.visible ?? p.visible }
    })
    const business = toValue(source).find(c => !c.type && c.placement !== 'end')
    if (!visibleColumns.value.some(c => !c.type && c.placement !== 'end') && business) {
      preferences.value = preferences.value.map(p => p.key === business.key ? { ...p, visible: true } : p)
    }
  })
  return { preferences, visibleColumns, reset }
}
