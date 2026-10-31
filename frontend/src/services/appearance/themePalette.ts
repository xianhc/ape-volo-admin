import { DEFAULT_PRIMARY_COLOR } from '@/services/appearance/themeRuntime'

/** 登录页与系统偏好共用的预设颜色；预设之外的合法颜色仍作为自定义主色使用。 */
export interface ThemeColorPreset {
  /** 六位十六进制主色值。 */
  readonly value: string
  /** 当前颜色名称。 */
  readonly label: string
}

/** 八种共用预设色，第一项为默认蓝色；系统设置加上自定义入口后组成三行三列。 */
export const THEME_COLOR_PRESETS: readonly ThemeColorPreset[] = [
  { value: DEFAULT_PRIMARY_COLOR, label: '蓝色' },
  { value: '#4f46e5', label: '靛蓝' },
  { value: '#7c3aed', label: '紫色' },
  { value: '#0e7490', label: '青色' },
  { value: '#047857', label: '绿色' },
  { value: '#c2410c', label: '橙色' },
  { value: '#e11d48', label: '玫红' },
  { value: '#475569', label: '石墨' }
]
