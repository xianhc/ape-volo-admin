/**
 * 菜单标题解析（本项目不做多语言，直接使用后端返回的 meta.title）。
 */

/**
 * 读取路由元信息中的菜单标题。
 * @param meta 路由元信息。
 * @returns 菜单标题；缺失时返回空文本。
 */
export function menuTitle(meta: Record<string, unknown> | undefined): string {
  return String(meta?.title ?? '')
}
