/**
 * 页签菜单类型声明。
 */
import type { TabAction } from '@/services/navigation/workspaceTabs.types'

/**
 * 标签上下文菜单中的动作、可用状态与展示信息。
 */
export interface TabMenuEntry {
  /** 主键。 */
  id: TabAction
  /** 标签。 */
  label: string
  /** 图标。 */
  icon: string
  /** 是否禁用。 */
  disabled: boolean
  /** 分隔符。 */
  separator?: boolean
}
