/**
 * 顶部工具栏项配置（本项目不做多语言，label 直接为中文）。
 */
import type { ToolbarItemKey } from './preferences.types'

export const TOOLBAR_ITEMS: Record<ToolbarItemKey, {
  /** 展示文案。 */
  label: string
  /** 当前菜单或控件的图标标识。 */
  icon: string
}> = {
  menuSearch: { label: '菜单搜索', icon: 'lucide:search' },
  preferences: { label: '系统设置', icon: 'lucide:settings' },
  theme: { label: '主题', icon: 'lucide:sun-moon' },
  fullscreen: { label: '全屏', icon: 'lucide:maximize' },
  refresh: { label: '刷新', icon: 'lucide:rotate-cw' },
  lockScreen: { label: '锁屏', icon: 'lucide:lock-keyhole' },
  logout: { label: '退出登录', icon: 'lucide:log-out' }
}
