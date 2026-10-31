import { onMounted, onBeforeUnmount } from 'vue'
import { useDebounceFn } from '@vueuse/core'
import { useAppStore } from '@/pinia'

/**
 * 响应式断点阈值 - 992px 以下视为移动端
 * @constant {number}
 */
const WIDTH = 992

/**
 * 查询当前设备类型（桌面端或移动端）
 * @function queryDevice
 * @description 通过检测 document.body 的宽度来判断设备类型
 * @returns {boolean} 如果宽度小于 992px 返回 true（移动端），否则返回 false（桌面端）
 */
function queryDevice(): boolean {
  const rect = document.body.getBoundingClientRect()
  return rect.width < WIDTH
}

/**
 * 响应式设备监听 Hook
 * @function useResponsive
 * @description Vue 3 Composition API Hook，用于监听窗口大小变化并自动切换设备类型状态
 *
 * 功能特性：
 * - 自动监听浏览器窗口 resize 事件
 * - 使用防抖优化性能，避免频繁触发
 * - 在页面隐藏时暂停检测，节省资源
 * - 支持立即执行检测
 * - 自动管理事件监听器的添加和移除
 *
 * 使用场景：
 * - 响应式布局适配
 * - 移动端和桌面端界面切换
 * - 设备类型相关的功能启用/禁用
 *
 * @param {boolean} [immediate=false] - 是否在组件挂载时立即执行一次设备检测
 *
 * @example
 * // 在 Vue 组件中使用
 * import useResponsive from '@/composables/useResponsive'
 *
 * export default {
 *   setup() {
 *     // 启用响应式监听，不立即执行
 *     useResponsive()
 *
 *     // 启用响应式监听，并立即执行一次检测
 *     useResponsive(true)
 *   }
 * }
 * @param immediate 是否在创建响应式布局监听时立即计算当前布局。
 */
export default function useResponsive(immediate: boolean = false): void {
  const appStore = useAppStore()

  /**
   * 窗口大小变化处理函数
   * @function resizeHandler
   * @description 处理窗口 resize 事件，检测设备类型并更新应用状态
   * 只在页面可见时执行，避免在后台标签页中进行不必要的计算
   */
  const resizeHandler = (): void => {
    if (!document.hidden) {
      const isMobile = queryDevice()
      appStore.toggleDevice(isMobile ? 'mobile' : 'desktop')
    }
  }

  // 使用防抖函数优化性能，100ms 内的多次调用只执行最后一次
  const debounceFn = useDebounceFn(resizeHandler, 100)

  /**
   * 组件挂载时的初始化逻辑
   * 添加窗口 resize 事件监听器，并根据 immediate 参数决定是否立即执行检测
   */
  onMounted(() => {
    window.addEventListener('resize', debounceFn)
    if (immediate) debounceFn()
  })

  /**
   * 组件卸载前的清理逻辑
   * 移除窗口 resize 事件监听器，避免内存泄漏
   */
  onBeforeUnmount(() => {
    window.removeEventListener('resize', debounceFn)
  })
}
