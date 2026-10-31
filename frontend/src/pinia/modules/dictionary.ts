import { single } from '@/api/system/dictionary'
import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { DictionaryDetail } from '@/api/system/types/dictionaryDetail.types'

/**
 * 字典项接口
 */
export interface DictionaryItem {
  label: string
  value: string
}

/**
 * 字典映射类型
 */
export type DictionaryMap = Record<string, DictionaryItem[]>

/**
 * 字典数据状态管理 Store
 * @description 管理系统字典数据的缓存和获取
 * @returns {Object} 字典状态和相关方法
 */
export const useDictionaryStore = defineStore('dictionary', () => {
  // 字典数据映射表
  const dictionaryMap = ref<DictionaryMap>({})
  // 加载失败的字典名称
  const errors = ref<Record<string, boolean>>({})
  // 后端不存在（返回空）的字典名称
  const missing = ref<Record<string, boolean>>({})
  // 缓存版本号：强制刷新后自增，驱动所有 useDictionary 重新取值
  const revision = ref(0)
  let generation = 0

  /**
   * 设置字典数据到映射表
   * @param {DictionaryMap} dictionaryRes - 字典数据对象
   * @returns {void}
   */
  const setDictionaryMap = (dictionaryRes: DictionaryMap): void => {
    dictionaryMap.value = { ...dictionaryMap.value, ...dictionaryRes }
  }

  /**
   * 获取指定名称的字典数据
   * @param {string} name - 字典名称
   * @returns {Promise<DictionaryItem[]>} 返回字典项数组的Promise
   * @description 优先从缓存获取，缓存不存在则从API获取并缓存
   */
  const getDictionary = async (name: string): Promise<DictionaryItem[]> => {
    const ticket = generation
    // 检查缓存中是否已存在该字典数据
    if (dictionaryMap.value[name] && dictionaryMap.value[name].length) {
      return dictionaryMap.value[name]
    }
    try {
      // 从API获取字典数据
      const res = await single({ name })
      if (ticket !== generation) return []
      const dictionaryRes: DictionaryMap = {}
      const dict: DictionaryItem[] = []
      // 后端可能对不存在的字典返回空 data，这里统一兜底为空列表
      const detail = res?.data?.dictDetails
      if (detail?.length) {
        detail.forEach((item: DictionaryDetail) => {
          dict.push({
            label: item.label,
            value: item.value
          })
        })
      }
      // 将数据加入缓存
      dictionaryRes[res?.data?.name ?? name] = dict
      setDictionaryMap(dictionaryRes)
      delete errors.value[name]
      missing.value[name] = dict.length === 0
      return dictionaryMap.value[name]
    } catch (error) {
      if (ticket === generation) errors.value[name] = true
      throw error
    }
  }

  /**
   * 丢弃指定字典的缓存并重新拉取。
   * @param {string} name - 字典名称
   * @returns {Promise<DictionaryItem[]>} 重新加载后的字典项
   */
  const refresh = async (name: string): Promise<DictionaryItem[]> => {
    delete dictionaryMap.value[name]
    delete missing.value[name]
    delete errors.value[name]
    revision.value++
    return getDictionary(name)
  }

  const reset = (): void => {
    generation++
    dictionaryMap.value = {}
    errors.value = {}
    missing.value = {}
    // 退出只清理缓存，不触发旧页面的字典重载订阅。
  }

  return {
    dictionaryMap,
    errors,
    missing,
    revision,
    setDictionaryMap,
    getDictionary,
    refresh,
    reset
  }
})
