// 全局图标组件类型声明。
export { }
declare module 'vue' {
  /**
   * 项目全局注册组件的类型扩展。
   */
  export interface GlobalComponents {
    /** 全局统一图标组件的类型。 */
    AppIcon: typeof import('./components/AppIcon.vue')['default']
    /** 全局图标选择器组件的类型。 */
    IconPicker: typeof import('./components/IconPicker.vue')['default']
  }
}
