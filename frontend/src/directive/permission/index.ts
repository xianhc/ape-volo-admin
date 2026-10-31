/**
 * 权限控制指令模块
 * 提供基于角色和权限的DOM元素显示控制功能
 *
 * 功能特性：
 * - 角色权限验证指令 (v-has-role)
 * - 功能权限验证指令 (v-has-perm)
 * - 支持单个或多个权限验证
 * - 自动移除无权限的DOM元素
 * - 与Pinia用户状态管理集成
 *
 * 使用场景：
 * - 按钮权限控制
 * - 菜单项显示控制
 * - 功能模块访问限制
 * - 界面元素的细粒度权限管理
 */

import type { Directive, DirectiveBinding } from 'vue'
import { useUserStore } from '@/pinia/modules/user'

/**
 * 角色或权限值类型
 */
type PermissionValue = string | string[]

/**
 * 角色权限验证指令
 * @directive hasRole
 * @description Vue 3 自定义指令，用于根据用户角色控制DOM元素的显示
 *
 * 验证逻辑：
 * - 检查用户是否拥有指定角色
 * - 支持单个角色字符串或角色数组
 * - 数组模式下，满足任一角色即可显示
 * - 无权限时自动移除DOM元素
 *
 * 使用方式：
 * - 单个角色: v-has-role="'admin'"
 * - 多个角色: v-has-role="['admin', 'manager']"
 * - 变量绑定: v-has-role="roleVariable"
 *
 * @example
 * <!-- 只有admin角色可见 -->
 * <button v-has-role="'admin'">管理员功能</button>
 *
 * <!-- admin或manager角色可见 -->
 * <div v-has-role="['admin', 'manager']">高级功能</div>
 *
 * <!-- 动态角色检查 -->
 * <span v-has-role="dynamicRole">动态内容</span>
 */
export const hasRole: Directive<HTMLElement, PermissionValue> = {
  /**
   * 元素挂载时的权限检查
   * @param {HTMLElement} el - 要控制的DOM元素
   * @param {DirectiveBinding<PermissionValue>} binding - Vue指令绑定对象
   * @throws {Error} 当角色参数格式不正确时抛出错误
   */
  mounted(el: HTMLElement, binding: DirectiveBinding<PermissionValue>) {
    const requiredRoles = binding.value

    // 参数验证：确保提供了有效的角色标识
    if (
      !requiredRoles ||
      (typeof requiredRoles !== 'string' && !Array.isArray(requiredRoles))
    ) {
      throw new Error(
        'v-has-role 指令需要提供有效的角色标识参数。\n' +
          '支持格式：\n' +
          '• 单个角色：v-has-role="\'admin\'"\n' +
          "• 多个角色：v-has-role=\"['admin', 'manager', 'guest']\"\n" +
          '• 变量绑定：v-has-role="roleVariable"'
      )
    }

    const userStore = useUserStore()

    // 权限验证逻辑
    const hasAuth = Array.isArray(requiredRoles)
      ? // 数组模式：检查用户是否拥有任一指定角色
        requiredRoles.some((role) =>
          userStore.userInfo.roleCodes.includes(role)
        )
      : // 字符串模式：检查用户是否拥有指定角色
        userStore.userInfo.roleCodes.includes(requiredRoles)

    // 无权限时移除DOM元素
    if (!hasAuth && el.parentNode) {
      el.parentNode.removeChild(el)
    }
  }
}

/**
 * 功能权限验证指令
 * @directive hasPerm
 * @description Vue 3 自定义指令，用于根据用户功能权限控制DOM元素的显示
 *
 * 验证逻辑：
 * - 检查用户是否拥有指定功能权限
 * - 支持单个权限字符串或权限数组
 * - 数组模式下，满足任一权限即可显示
 * - 无权限时自动移除DOM元素
 * - 权限标识通常采用层级格式：模块:功能:操作
 *
 * 权限标识格式：
 * - 用户管理: sys:user:add, sys:user:edit, sys:user:delete
 * - 角色管理: sys:role:view, sys:role:assign
 * - 菜单管理: sys:menu:create, sys:menu:update
 *
 * 使用方式：
 * - 单个权限: v-has-perm="'sys:user:add'"
 * - 多个权限: v-has-perm="['sys:user:add', 'sys:user:edit']"
 * - 变量绑定: v-has-perm="permissionVariable"
 *
 * @example
 * <!-- 只有添加用户权限可见 -->
 * <button v-has-perm="'sys:user:add'">添加用户</button>
 *
 * <!-- 有编辑或删除权限可见 -->
 * <div v-has-perm="['sys:user:edit', 'sys:user:delete']">操作按钮</div>
 *
 * <!-- 动态权限检查 -->
 * <span v-has-perm="computedPermission">动态功能</span>
 */
export const hasPerm: Directive<HTMLElement, PermissionValue> = {
  /**
   * 元素挂载时的权限检查
   * @param {HTMLElement} el - 要控制的DOM元素
   * @param {DirectiveBinding<PermissionValue>} binding - Vue指令绑定对象
   * @throws {Error} 当权限参数格式不正确时抛出错误
   */
  mounted(el: HTMLElement, binding: DirectiveBinding<PermissionValue>) {
    const requiredPerms = binding.value

    // 参数验证：确保提供了有效的权限标识
    if (
      !requiredPerms ||
      (typeof requiredPerms !== 'string' && !Array.isArray(requiredPerms))
    ) {
      // 不抛异常：指令里抛出会中断整个组件树的渲染，把页面打成白屏。
      // 这里只记错误并保留元素，让问题可定位但不影响其它按钮。
      console.error(
        '[v-has-perm] 权限参数无效，已跳过该元素的权限控制。期望字符串或字符串数组，实际收到：',
        requiredPerms,
        '\n元素：',
        el.outerHTML.slice(0, 200)
      )
      return
    }

    const userStore = useUserStore()

    // 权限验证逻辑
    const hasAuth = Array.isArray(requiredPerms)
      ? // 数组模式：检查用户是否拥有任一指定权限
        requiredPerms.some((perm) =>
          userStore.userInfo.authCodes.includes(perm)
        )
      : // 字符串模式：检查用户是否拥有指定权限
        userStore.userInfo.authCodes.includes(requiredPerms)

    // 无权限时移除DOM元素
    if (!hasAuth && el.parentNode) {
      el.parentNode.removeChild(el)
    }
  }
}
