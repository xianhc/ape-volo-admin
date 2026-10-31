/**
 * 过渡类型枚举
 */
export const TransitionType = {
  /** 滑动 */
  Slide: 'slide',
  /** 淡入淡出 */
  Fade: 'fade',
  /** 缩放 */
  Zoom: 'zoom',
  /** 无动画 */
  None: 'none'
} as const

export type TransitionTypeValue =
  (typeof TransitionType)[keyof typeof TransitionType]
