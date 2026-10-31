/**
 * 菜单搜索组合式函数。
 */
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useRouterStore } from '@/pinia/modules/router'
import type { RouteMenuItem } from '@/pinia/modules/router'
import { MenuType } from '@/enums/MenuType'
import { navigateToMenu } from '@/services/navigation/menuNavigation'
import type { NavigableMenuItem } from '@/services/navigation/menuNavigation'
import { readStoredJson, writeStoredJson, removeStoredValue } from '@/services/storage/appStorage'

/**
 * 菜单搜索的候选节点与匹配信息。
 */
interface SearchItem {
  /** 当前对象的名称。 */
  name: string
  /** 当前内容或界面的展示标题。 */
  title: string
  /** 当前菜单或控件的图标标识。 */
  icon: string
  /** 搜索结果关联的可导航菜单项。 */
  menu: NavigableMenuItem
}

/**
 * 需要解析的路由查询对象或路由来源。
 */
type SearchRoute = RouteMenuItem
const HISTORY_KEY = 'menu_search_history'
/**
 * 将当前输入转换为统一的比较或存储表示。
 * @param value 本次转换、校验或更新所接收的值。
 * @returns 用于菜单匹配的规范化、小写且移除空白的文本。
 */
const normalize = (value: string): string => value.normalize('NFKC').toLocaleLowerCase().replace(/\s+/g, '')

/**
 * 计算菜单条目与搜索关键字的匹配优先级。
 * @param title 菜单显示标题。
 * @param keyword 用于筛选候选记录的关键字。
 * @returns 匹配等级：完全匹配为 0、前缀为 1、包含为 2、顺序字符匹配为 3；不匹配为 -1。
 */
function matchRank(title: string, keyword: string): number {
  const text = normalize(title)
  if (text === keyword) return 0
  if (text.startsWith(keyword)) return 1
  if (text.includes(keyword)) return 2
  let position = 0
  for (const character of keyword) {
    const index = text.indexOf(character, position)
    if (index < 0) return -1
    position = index + character.length
  }
  return 3
}

/**
 * 按当前可见菜单建立搜索候选，维护搜索历史、键盘选中项及导航中的状态。
 * @returns 搜索关键字、匹配结果、活动项及选择、移动、关闭和历史管理操作。
 */
export function useMenuSearch() {
  const router = useRouter()
  const routerStore = useRouterStore()
  const keyword = ref('')
  const activeIndex = ref(-1)
  const navigating = ref(false)
  const dismissed = ref(new Set<string>())
  const history = ref(readStoredJson<string[]>(localStorage, HISTORY_KEY, [],
    (value): value is string[] => Array.isArray(value) && value.every(item => typeof item === 'string')))

  const menuItems = computed(() => {
    const items: SearchItem[] = []
    const seen = new Set<string>()
    /**
     * 遍历当前树或文件结构并收集符合条件的条目。
     * @param routes 待构建或遍历的路由集合。
     */
    function collect(routes: readonly SearchRoute[]): void {
      for (const raw of routes) {
        const route = raw as SearchRoute
        if (route.hidden || route.meta?.hidden) continue
        const name = typeof route.name === 'string' ? route.name : ''
        const displayName = route.meta?.displayName
        const isPage = route.menuType !== MenuType.Catalog &&
          (!route.children?.length || route.menuType === MenuType.Menu || route.menuType === MenuType.InternalLink)
        if (name && typeof displayName === 'string' && isPage && !seen.has(name)) {
          const source = routerStore.routeMap[name]
          const component = source?.component
          items.push({
            name, title: displayName, icon: String(route.meta?.icon || 'lucide:menu'),
            menu: source ?? { name, menuType: route.menuType, parameters: route.parameters, component }
          })
          seen.add(name)
        }
        if (route.children?.length) collect(route.children)
      }
    }
    collect(routerStore.asyncRouters[0]?.children ?? [])
    return items
  })

  const searching = computed(() => normalize(keyword.value).length > 0)
  const results = computed(() => {
    const query = normalize(keyword.value)
    if (!query) return history.value.flatMap(name => {
      const item = menuItems.value.find(menu => menu.name === name)
      return item ? [item] : []
    })
    return menuItems.value.map(item => ({ item, rank: matchRank(item.title, query) }))
      .filter(({ item, rank }) => rank >= 0 && !dismissed.value.has(item.name))
      .sort((left, right) => left.rank - right.rank)
      .map(({ item }) => item)
  })

  watch(keyword, () => {
    dismissed.value = new Set()
  }, { flush: 'sync' })
  watch(results, () => {
    activeIndex.value = results.value.length ? 0 : -1
  }, { flush: 'sync' })

  /**
   * 恢复菜单搜索当前输入或状态的初始值。
   */
  function reset(): void {
    keyword.value = ''
    dismissed.value = new Set()
    activeIndex.value = results.value.length ? 0 : -1
  }

  /**
   * 按当前方向移动选择项、节点或标签。
   * @param direction 本次移动或选择的方向。
   */
  function move(direction: number): void {
    const count = results.value.length
    if (count) activeIndex.value = (activeIndex.value + direction + count) % count
  }

  /**
   * 关闭当前菜单并恢复触发器焦点。
   * @param index 当前项目在集合中的位置。
   */
  function dismiss(index: number): void {
    const item = results.value[index]
    if (!item) return
    if (searching.value) dismissed.value = new Set([...dismissed.value, item.name])
    else {
      history.value = history.value.filter(name => name !== item.name)
      writeStoredJson(localStorage, HISTORY_KEY, history.value)
    }
    activeIndex.value = results.value.length ? Math.min(index, results.value.length - 1) : -1
  }

  /**
   * 清除当前用户的本地导航历史。
   */
  function clearHistory(): void {
    history.value = []
    removeStoredValue(localStorage, HISTORY_KEY)
  }

  /**
   * 将选中的菜单搜索项目应用到当前状态。
   * @param index 当前项目在集合中的位置。
   * @returns 菜单导航完成并已记录搜索历史时为 true；目标不可用或导航失败时为 false。
   */
  async function select(index: number): Promise<boolean> {
    const item = results.value[index]
    if (!item || navigating.value) return false
    navigating.value = true
    try {
      if (!await navigateToMenu(router, item.menu)) return false
      const available = new Set(menuItems.value.map(menu => menu.name))
      history.value = [item.name, ...history.value.filter(name => name !== item.name && available.has(name))].slice(0, 5)
      writeStoredJson(localStorage, HISTORY_KEY, history.value)
      return true
    } finally {
      navigating.value = false
    }
  }

  return { keyword, activeIndex, results, searching, navigating, reset, move, dismiss, clearHistory, select }
}
