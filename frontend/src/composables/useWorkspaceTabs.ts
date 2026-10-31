import { computed, ref, watch } from 'vue'
import { useRoute, useRouter, isNavigationFailure } from 'vue-router'
import { useTimeoutFn } from '@vueuse/core'
import { useAppStore } from '@/pinia'
import { useRouterStore } from '@/pinia/modules/router'
import { emitter } from '@/utils/bus'
import setting from '@/setting'
import { menuTitle } from '@/utils/menuTitle'
import { fmtTitle } from '@/utils/index'
import {
  readStoredJson,
  readStoredText,
  removeStoredValue,
  writeStoredJson,
  writeStoredText
} from '@/services/storage/appStorage'
import type {
  TabAction,
  TabHistoryItem
} from '@/services/navigation/workspaceTabs.types'

/**
 * 管理工作区标签历史、固定顺序、路由激活及关闭动作，并将历史同步到会话存储。
 * @returns 标签列表、活动项、忙碌状态及激活、关闭、排序和刷新入口。
 */
export function useWorkspaceTabs() {
  const route = useRoute()
  const router = useRouter()
  const app = useAppStore()
  const navigation = useRouterStore()
  const historys = ref<TabHistoryItem[]>([])
  const activeValue = ref('')
  const busy = ref(false)
  const reloading = ref(false)
  const { start: startReloadIndicator } = useTimeoutFn(
    () => {
      reloading.value = false
    },
    1000,
    { immediate: false }
  )
  const historyMap = computed(
    () => new Map(historys.value.map((item) => [item.name, item]))
  )

  /**
   * 将固定标签排列在普通标签之前，持久化历史并通知 KeepAlive 更新缓存。
   */
  function commit() {
    historys.value = [
      ...historys.value.filter((item) => item.pinned),
      ...historys.value.filter((item) => !item.pinned)
    ]
    writeStoredJson(sessionStorage, 'historys', historys.value)
    navigation.setKeepAliveRouters(historys.value)
  }

  /**
   * 按当前上下文同步已展示的数据。
   */
  function syncCurrent() {
    if (
      !route.name ||
      route.name === 'Login' ||
      route.name === 'Reload' ||
      route.meta.closeTab
    )
      return
    historys.value = historys.value.filter(
      (item) => router.hasRoute(item.name) && !item.meta.closeTab
    )
    const name = String(route.name)
    const meta = { ...route.meta } as TabHistoryItem['meta']
    delete meta.matched
    const existing = historyMap.value.get(name)
    const next: TabHistoryItem = {
      name,
      meta,
      query: { ...route.query },
      params: { ...route.params },
      fullPath: route.fullPath,
      pinned: existing?.pinned ?? name === setting.defaultRouter
    }
    if (existing) Object.assign(existing, next)
    else historys.value.push(next)
    activeValue.value = name
    writeStoredText(sessionStorage, 'activeValue', name)
    commit()
  }

  /**
   * 读取当前操作的目标及其标识。
   * @param item 当前待处理的候选项或记录。
   * @returns 标签保存的完整路径或带参数的路由定位对象。
   */
  function target(item: TabHistoryItem) {
    return (
      item.fullPath || {
        name: item.name,
        query: item.query as never,
        params: item.params
      }
    )
  }

  /**
   * 按目标配置进入页面或外部地址。
   * @param item 当前待处理的候选项或记录。
   * @returns 路由成功到达目标路径时为 true；导航失败或未到达时为 false。
   */
  async function navigate(item: TabHistoryItem): Promise<boolean> {
    if (!router.hasRoute(item.name)) return false
    try {
      const resolved = router.resolve(target(item))
      if (route.fullPath === resolved.fullPath) return true
      const result = await router.push(resolved.fullPath)
      return (
        !isNavigationFailure(result) && route.fullPath === resolved.fullPath
      )
    } catch {
      return false
    }
  }

  /**
   * 激活工作区标签并恢复其需要的监听或数据。
   * @param name 待解析或匹配的名称。
   */
  async function activate(name: string) {
    const item = historyMap.value.get(name)
    if (!item || busy.value || !(await navigate(item)))
      activeValue.value = String(route.name ?? '')
  }

  /**
   * 判断当前忙碌状态是否允许关闭弹层。
   * @param item 当前待处理的候选项或记录。
   * @returns 当前忙碌状态是否允许关闭弹层时返回 true，否则返回 false。
   */
  function canClose(item: TabHistoryItem) {
    return !busy.value && !item.pinned && historys.value.length > 1
  }

  /**
   * 按拖放结果更新标签顺序。
   * @param sourceName 原节点或原文件的名称。
   * @param targetName 目标节点或目标文件的名称。
   */
  function reorderTabs(sourceName: string, targetName: string): void {
    if (busy.value || sourceName === targetName) return
    const sourceIndex = historys.value.findIndex(
      (item) => item.name === sourceName
    )
    const targetIndex = historys.value.findIndex(
      (item) => item.name === targetName
    )
    if (sourceIndex < 0 || targetIndex < 0) return
    const source = historys.value[sourceIndex]
    const target = historys.value[targetIndex]
    if (source.pinned || target.pinned) return

    historys.value.splice(sourceIndex, 1)
    const nextTargetIndex = historys.value.findIndex(
      (item) => item.name === targetName
    )
    historys.value.splice(nextTargetIndex, 0, source)
    commit()
  }

  /**
   * 按标签动作和固定状态计算允许关闭的标签名称。
   * @param action 当前执行或展示的业务动作。
   * @param name 待解析或匹配的名称。
   * @returns 符合当前关闭动作且未固定的标签名称。
   */
  function candidates(action: TabAction, name: string): string[] {
    const index = historys.value.findIndex((item) => item.name === name)
    if (index < 0) return []
    return historys.value
      .filter(
        (item, position) =>
          !item.pinned &&
          (action === 'closeAll' ||
            (action === 'close' && item.name === name) ||
            (action === 'closeLeft' && position < index) ||
            (action === 'closeRight' && position > index) ||
            (action === 'closeOthers' && item.name !== name))
      )
      .map((item) => item.name)
  }

  /**
   * 判断是否启用。
   * @param action 当前执行或展示的业务动作。
   * @param name 待解析或匹配的名称。
   * @returns 是否启用。
   */
  function enabled(action: TabAction, name: string) {
    const item = historyMap.value.get(name)
    if (!item || busy.value || route.name === 'Reload') return false
    if (action === 'close') return canClose(item)
    if (action === 'reload') return name === route.name && !reloading.value
    if (
      action === 'closeLeft' ||
      action === 'closeRight' ||
      action === 'closeOthers'
    ) {
      return name === route.name && candidates(action, name).length > 0
    }
    if (action === 'closeAll')
      return historys.value.length > 1 && candidates(action, name).length > 0
    return router.hasRoute(name)
  }

  /**
   * 关闭指定的未固定标签，并为被关闭的活动页选择后继路由。
   * @param names 待匹配或操作的名称集合。
   */
  async function closeNames(names: string[]) {
    const removed = new Set(names)
    if (removed.has(String(route.name))) {
      const index = historys.value.findIndex((item) => item.name === route.name)
      const remaining = historys.value.filter((item) => !removed.has(item.name))
      const next =
        historys.value
          .slice(index + 1)
          .find((item) => !removed.has(item.name)) ||
        remaining[remaining.length - 1]
      const fallback: TabHistoryItem = {
        name: router.hasRoute(setting.defaultRouter)
          ? setting.defaultRouter
          : 'Forbidden',
        meta: { displayName: 'Ui.Shell.Home', icon: 'lucide:layout-dashboard' }
      }
      // 导航被未保存确认或权限守卫阻止时，不先删除历史和当前页面。
      if (!(await navigate(next || fallback))) return
    }
    // 回退首页可能也在待关闭集合中，保留导航后重新打开的当前页面。
    historys.value = historys.value.filter(
      (item) => !removed.has(item.name) || item.name === route.name
    )
    commit()
  }

  /**
   * 按当前标签执行所选上下文菜单动作。
   * @param action 当前执行或展示的业务动作。
   * @param name 待解析或匹配的名称。
   */
  async function runAction(action: TabAction, name: string) {
    if (!enabled(action, name)) return
    const item = historyMap.value.get(name)
    if (!item) return
    if (action === 'openWindow') {
      window.open(
        router.resolve(target(item)).href,
        '_blank',
        'noopener,noreferrer'
      )
      return
    }
    if (action === 'reload') {
      reloading.value = true
      startReloadIndicator()
      emitter.emit('reload')
      return
    }
    if (action === 'pin') {
      item.pinned = !item.pinned
      commit()
      return
    }
    busy.value = true
    try {
      if (action === 'maximize') {
        if (app.workspaceMaximized || (await navigate(item)))
          app.toggleWorkspaceMaximized()
      } else {
        await closeNames(candidates(action, name))
      }
    } finally {
      busy.value = false
    }
  }

  const restored = readStoredJson<TabHistoryItem[]>(
    sessionStorage,
    'historys',
    [],
    (value): value is TabHistoryItem[] =>
      Array.isArray(value) &&
      value.every(
        (item) =>
          typeof item === 'object' &&
          item !== null &&
          typeof item.name === 'string' &&
          typeof item.meta === 'object' &&
          item.meta !== null
      )
  )
  historys.value = restored
    .filter((item) => {
      if (!router.hasRoute(item.name) || item.meta.closeTab) return false
      try {
        return (
          !item.fullPath || router.resolve(item.fullPath).name === item.name
        )
      } catch {
        return false
      }
    })
    .map((item) => ({
      ...item,
      pinned:
        typeof item.pinned === 'boolean'
          ? item.pinned
          : item.name === setting.defaultRouter
    }))
  syncCurrent()
  watch(() => route.fullPath, syncCurrent)
  if (readStoredText(sessionStorage, 'needCloseAll', 'false') === 'true') {
    removeStoredValue(sessionStorage, 'needCloseAll')
    void closeNames(historys.value.map((item) => item.name))
  }

  return {
    historys,
    activeValue,
    busy,
    reloading,
    historyMap,
    activate,
    enabled,
    canClose,
    runAction,
    reorderTabs,
    /**
     * 取得或应用当前公告或通知的标题。
     * @param item 当前待处理的候选项或记录。
     * @returns 公告或通知的标题。
     */
    title: (item: TabHistoryItem) =>
      fmtTitle(menuTitle(item.meta), {
        query: (item.query ?? {}) as never,
        params: (item.params ?? {}) as never
      })
  }
}
