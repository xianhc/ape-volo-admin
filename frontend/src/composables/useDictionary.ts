/**
 * 字典数据加载组合式函数。
 */
import { computed, ref, watch, toValue, onScopeDispose } from 'vue'
import type { MaybeRefOrGetter } from 'vue'
import { useDictionaryStore } from '@/pinia/modules/dictionary'
/**
 * 响应字典名称和缓存版本变化加载选项，以请求代次保护当前加载状态，并在作用域销毁时失效。
 * @param name 字典名称，可传入响应式值或读取函数。
 * @returns 字典选项、加载错误和缺失状态，以及普通加载和强制刷新入口。
 */
export function useDictionary(name: MaybeRefOrGetter<string>) {
  const store = useDictionaryStore()
  const loading = ref(false)
  let generation = 0
  const currentName = computed(() => toValue(name))
  const options = computed(() => store.dictionaryMap[currentName.value] ?? [])
  const error = computed(() => store.errors[currentName.value] === true)
  const missing = computed(() => store.missing[currentName.value] === true)
  const available = computed(() => Object.prototype.hasOwnProperty.call(store.dictionaryMap, currentName.value))
  const status = computed(() => {
    if (loading.value) {
      return 'loading'
    }
    if (error.value) {
      return 'error'
    }
    if (missing.value) {
      return 'missing'
    }
    if (available.value) {
      return 'ready'
    }
    return 'idle'
  })
  const ready = computed(() => available.value && !missing.value && !error.value)
  /**
   * 读取字典当前条件对应的数据。
   * @param force 是否跳过已有字典缓存并请求刷新。
   * @returns 当前加载仍有效且字典存在时返回 true；名称为空、过期、缺失或加载失败时返回 false。
   */
  async function load(force = false): Promise<boolean> {
    const ticket = ++generation, current = currentName.value
    if (!current) return false
    loading.value = true
    try {
      await (force ? store.refresh(current) : store.getDictionary(current))
      if (ticket !== generation) return false
      return !missing.value
    } catch {
      if (ticket !== generation) return false
      return false
    } finally {
      if (ticket === generation) loading.value = false
    }
  }
  watch([currentName, () => store.revision], () => {
    void load()
  }, { immediate: true })
  onScopeDispose(() => {
    generation++
  })
  return {
    options, loading, error, missing, status, ready, load,
    /**
     * 重新加载字典当前上下文的数据。
     * @returns 强制刷新完成后，当前字典存在且加载仍有效时为 true。
     */
    reload: () => load(true)
  }
}

/**
 * 字典选项接口（用于下拉框等组件）
 * @interface DictOption
 * @example
 * const option: DictOption = { value: '1', label: '启用' }
 */
export interface DictOption {
  /** 选项值，用于表单绑定和数据提交 */
  value: string | number
  /** 选项标签，用于界面显示 */
  label: string
  /** 其他扩展属性 */
  [key: string]: any
}

/**
 * 一次性获取字典数据，适用于打开表单时加载下拉选项等无需响应式的场景。
 * 需要跟随字典名称变化自动刷新时改用 {@link useDictionary}。
 * @param name 字典名称。
 * @returns 字典选项数组，未命中时为空数组。
 */
export const getDict = async (name: string): Promise<DictOption[]> => {
  const dictionaryStore = useDictionaryStore()
  await dictionaryStore.getDictionary(name)
  return (dictionaryStore.dictionaryMap[name] || []) as DictOption[]
}

/**
 * 显示字典的值
 * @function showDictLabel
 * @description 根据字典代码获取对应的显示标签
 *
 * @param {DictOption[] | null | undefined} dict - 字典数组
 * @param {string | number} code - 字典代码
 * @param {string} [keyCode='value'] - 字典项中作为键的字段名
 * @param {string} [valueCode='label'] - 字典项中作为值的字段名
 * @returns {string} 对应的标签值，如果找不到则返回空字符串
 *
 * @example
 * const dict = [
 *   { value: '1', label: '启用' },
 *   { value: '0', label: '禁用' }
 * ]
 * showDictLabel(dict, '1') // '启用'
 * showDictLabel(dict, '2') // ''
 * showDictLabel(dict, '1', 'code', 'name') // 使用自定义字段名
 */
export const showDictLabel = (
  dict: DictOption[] | null | undefined,
  code: string | number,
  keyCode: string = 'value',
  valueCode: string = 'label'
): string => {
  if (!dict) {
    return ''
  }
  const dictMap: Record<string | number, any> = {}
  dict.forEach((item) => {
    if (Reflect.has(item, keyCode) && Reflect.has(item, valueCode)) {
      dictMap[item[keyCode]] = item[valueCode]
    }
  })
  return Reflect.has(dictMap, code) ? dictMap[code] : ''
}
