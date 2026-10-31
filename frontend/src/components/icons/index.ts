import { defineComponent, h } from 'vue'
import type { Component } from 'vue'
import AppIcon from '../AppIcon.vue'
export { default as AppIcon } from '../AppIcon.vue'
export { default as IconPicker } from '../IconPicker.vue'
const components = new Map<string, Component>()
/**
 * Element Plus 的 icon 属性需要组件；相同名称复用组件身份，避免反复挂载。
 * @param icon 需要包装为组件的图标标识。
 * @returns 按图标名称缓存并复用的 Vue 图标组件。
 */
export function createIconifyIcon(icon: string): Component {
  let component = components.get(icon)
  if (!component) {
    component = defineComponent({
      name: 'ConfiguredIcon', inheritAttrs: false,
      /**
       * 创建统一图标组件的渲染函数并转发属性。
       * @param _ 框架传入但此回调不使用的参数。
       * @param context 组件上下文，其中 attrs 用于向图标转发属性。
       * @returns 转发属性并使用指定图标的渲染函数。
       * @param sort 排序事件，其中 prop 为字段名、order 为排序方向。
       */
      setup: (_, { attrs }) => () => h(AppIcon, { ...attrs, icon })
    })
    components.set(icon, component)
  }
  return component
}
