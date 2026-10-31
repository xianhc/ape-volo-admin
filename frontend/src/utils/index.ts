import { RouteLocationNormalizedLoaded } from 'vue-router'

/**
 * 接口基础前缀。构建期由 VITE_BASE_API 注入，运行期不会变化，
 * 因此直接用常量而不是 ref，避免每次读取都走一次响应式代理。
 */
const baseUrl = import.meta.env.VITE_BASE_API

/** 后端基础地址，用于拼接接口地址（如 `/api/user/info`）。 */
export const getBaseUrl = (): string => {
  return baseUrl === '/' ? '' : baseUrl
}

/**
 * 静态资源基础地址，用于拼接头像等由后端 wwwroot 直接提供的文件。
 * 后端 `UseStaticFiles()` 未配置 RequestPath，静态文件挂在根路径，
 * 因此这里不能复用带 `/api` 的接口前缀，否则会拼出 404 的 `/api/uploads/...`。
 */
export const getStaticBaseUrl = (): string => ''

/** 把菜单标题里的 `${param}` 占位符替换成当前路由参数或查询参数。 */
export const fmtTitle = (
  title: string,
  now: Pick<RouteLocationNormalizedLoaded, 'params' | 'query'>
): string => {
  const reg = /\$\{(.+?)\}/
  const reg_g = /\$\{(.+?)\}/g
  const result = title.match(reg_g)
  if (result) {
    result.forEach((item) => {
      const match = item.match(reg)
      if (match) {
        const key = match[1]
        const value = now.params[key] || now.query[key]
        title = title.replace(item, String(value))
      }
    })
  }
  return title
}
