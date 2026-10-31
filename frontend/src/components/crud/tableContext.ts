/**
 * 表格上下文注入键与类型。
 */
import type { ComputedRef, InjectionKey } from 'vue'
/**
 * 表格行操作宽度的测量与释放协调。
 */
export interface RowActionMeasurements {
  /**
   *  上报行操作区域的测量宽度。
   * @param id 目标记录或附件的标识，按现有字符串协议传递。
   * @param width 当前表格内容区可用宽度，单位为像素。
   */
  report: (id: symbol, width: number) => void
  /**
   *  删除业务记录的操作。
   * @param id 目标记录或附件的标识，按现有字符串协议传递。
   */
  remove: (id: symbol) => void
}
export const rowActionMeasurementsKey: InjectionKey<RowActionMeasurements> = Symbol('table-row-actions')

/**
 * 表格加载态的单一归属。
 *
 * 首次加载由表格遮罩负责，之后的刷新、翻页只体现在触发控件上，
 * 这样同一次请求只会有一个加载指示，不会出现遮罩与按钮同时转圈。
 */
export interface TableLoadingState {
  /** 表格尚未展示过任何数据，需要用遮罩占住内容区。 */
  blocking: ComputedRef<boolean>
  /** 已有数据时的重新加载，只需触发控件自身反馈。 */
  refreshing: ComputedRef<boolean>
}
export const tableLoadingKey: InjectionKey<TableLoadingState> = Symbol('table-loading')
