/**
 * 表格列定义与列偏好工具。
 */
import type { TableColumnCtx } from 'element-plus'

/**
 * 表格列的业务定义或用户保存的展示偏好。
 * @typeParam T 表格行数据类型，用于约束格式化及选择回调。
 */
export interface TableColumnSpec<T extends object> {
  /** 当前映射、缓存或节点的唯一键。 */
  key: string
  /** 面向用户显示的标签。 */
  label: string
  /** 表格列绑定的属性名或排序字段。 */
  prop?: string
  /** 当前对象的分类。 */
  type?: 'selection' | 'index' | 'expand'
  /** 当前组件对应的展示尺寸。 */
  width?: number | string
  /** 当前组件对应的展示尺寸。 */
  minWidth?: number | string
  /** 表格列内容对齐方式。 */
  align?: 'left' | 'center' | 'right'
  /** 当前列是否允许排序。 */
  sortable?: boolean | 'custom'
  /** 内容溢出时是否显示完整内容提示。 */
  showOverflowTooltip?: boolean
  /** 将字段值转换为展示文本的函数。 */
  formatter?: TableColumnCtx<T>['formatter']
  /** 当前记录是否允许选择。 */
  selectable?: TableColumnCtx<T>['selectable']
  /** 表格列的固定位置。 */
  fixed?: boolean | 'left' | 'right'
  /** 当前表格列默认是否显示。 */
  defaultVisible?: boolean
  /** 工具项或浮层的展示位置。 */
  placement?: 'start' | 'normal' | 'end'
  /** 当前字段是否必须填写。 */
  required?: boolean
}

/**
 * 表格列的业务定义或用户保存的展示偏好。
 */
export interface ColumnPreference {
  /** 当前映射、缓存或节点的唯一键。 */
  key: string
  /** 当前界面是否显示。 */
  visible: boolean
}

/**
 * 判断当前列是否为普通业务数据列，排除选择、索引、展开及尾部操作列。
 * @param column 当前表格的列定义。
 * @typeParam T 当前表格的行数据类型。
 * @returns 当前列没有特殊列类型且不位于固定尾部时返回 true。
 */
function isBusinessColumn<T extends object>(column: TableColumnSpec<T>) {
  return !column.type && column.placement !== 'end'
}

/**
 * 根据列定义创建默认列偏好。
 * @typeParam T 表格行数据类型。
 * @param columns 页面声明的完整表格列集合。
 * @returns 默认顺序及显示状态；确保至少保留一个可用业务列。
 */
export function createColumnPreferences<T extends object>(columns: readonly TableColumnSpec<T>[]): ColumnPreference[] {
  const keys = new Set<string>()
  for (const column of columns) {
    if (keys.has(column.key)) throw new Error(`重复表格列 key: ${column.key}`)
    keys.add(column.key)
  }
  const grouped = [
    ...columns.filter(c => c.placement === 'start'),
    ...columns.filter(c => !c.placement || c.placement === 'normal'),
    ...columns.filter(c => c.placement === 'end')
  ]
  const state = grouped.map(c => ({ key: c.key, visible: !!c.required || c.defaultVisible !== false }))
  if (!columns.some(c => isBusinessColumn(c) && state.find(s => s.key === c.key)?.visible)) {
    const first = columns.find(isBusinessColumn)
    const item = state.find(s => s.key === first?.key)
    if (item) item.visible = true
  }
  return state
}

/**
 * 更新指定表格列的显示状态。
 * @typeParam T 表格行数据类型。
 * @param columns 页面声明的完整表格列集合。
 * @param state 调用前的列显示和顺序偏好。
 * @param key 本次需要调整显示状态的列标识。
 * @param visible 当前列准备切换到的显示状态。
 * @returns 调整后的独立偏好列表；必选列及最后一个业务列不可隐藏。
 */
export function setColumnVisibility<T extends object>(
  columns: readonly TableColumnSpec<T>[], state: readonly ColumnPreference[], key: string, visible: boolean
): ColumnPreference[] {
  const column = columns.find(c => c.key === key)
  const result = state.map(c => ({ ...c }))
  if (!column || (!visible && column.required)) return result
  if (!visible
    && isBusinessColumn(column)
    && !columns.some(c =>
      c.key !== key
      && isBusinessColumn(c)
      && state.find(s => s.key === c.key)?.visible)) return result
  const item = result.find(c => c.key === key)
  if (item) item.visible = visible
  return result
}

/**
 * 调整指定表格列的显示顺序。
 * @typeParam T 表格行数据类型。
 * @param columns 页面声明的完整表格列集合。
 * @param state 调用前的列显示和顺序偏好。
 * @param source 当前被拖动的列标识。
 * @param target 拖动操作准备插入到的目标列标识。
 * @returns 调整顺序后的独立偏好列表；固定位置列或无效目标保持原顺序。
 */
export function moveColumn<T extends object>(
  columns: readonly TableColumnSpec<T>[], state: readonly ColumnPreference[], source: string, target: string
): ColumnPreference[] {
  const result = state.map(c => ({ ...c }))
  /**
   * 判断当前项目是否允许参与排序移动。
   * @param key 当前查询、映射或缓存使用的键。
   * @returns 当前项目是否允许参与排序移动时返回 true，否则返回 false。
   */
  const movable = (key: string) => {
    const column = columns.find(c => c.key === key)
    return column && (!column.placement || column.placement === 'normal')
  }
  if (!movable(source) || !movable(target)) return result
  const from = result.findIndex(c => c.key === source)
  const to = result.findIndex(c => c.key === target)
  if (from < 0 || to < 0 || from === to) return result
  const [item] = result.splice(from, 1)
  result.splice(to, 0, item)
  return result
}
