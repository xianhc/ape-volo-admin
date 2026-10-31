const STORAGE_VERSION = 1

/**
 * 浏览器存储中包含格式版本的封装值。
 * @typeParam T 版本封装中保存的业务值类型。
 */
interface StoredValue<T> {
  /** 当前记录或资源的版本标记。 */
  version: number
  /** 当前对象承载的值。 */
  value: T
}

/**
 * 检查未知配置值是否满足目标结构的类型守卫。
 * @typeParam T 结构校验通过后可收窄到的业务类型。
 */
type ValueGuard<T> = (value: unknown) => value is T

/**
 * 检查存储值是否包含数值格式版本及业务值封装；业务值结构由调用方守卫校验。
 * @param value 本次转换、校验或更新所接收的值。
 * @returns 输入满足目标类型要求的必要字段与值约束时返回 true。
 */
const isStoredValue = (value: unknown): value is StoredValue<unknown> =>
  typeof value === 'object' &&
  value !== null &&
  'version' in value &&
  typeof value.version === 'number' &&
  'value' in value

/**
 * 移除指定存储项；浏览器禁止访问存储时忽略删除失败。
 * @param storage 待操作的浏览器存储。
 * @param key 待删除的存储键。
 */
export const removeStoredValue = (storage: Storage, key: string): void => {
  try {
    storage.removeItem(key)
  } catch {
    // Storage may be unavailable in hardened browser contexts.
  }
}

/**
 * 读取浏览器存储文本；键不存在或存储不可访问时使用备用值。
 * @param storage localStorage、sessionStorage 或同协议存储。
 * @param key 待读取的存储键。
 * @param fallback 键缺失或读取失败时返回的文本。
 * @returns 已存储的文本或调用方提供的备用值。
 */
export function readStoredText(
  storage: Storage,
  key: string,
  fallback: string
): string {
  try {
    return storage.getItem(key) ?? fallback
  } catch {
    return fallback
  }
}

/**
 * 写入浏览器存储文本；配额不足或存储被禁用时忽略写入失败。
 * @param storage 待操作的浏览器存储。
 * @param key 待写入的存储键。
 * @param value 待保存的原始文本。
 */
export function writeStoredText(
  storage: Storage,
  key: string,
  value: string
): void {
  try {
    storage.setItem(key, value)
  } catch {
    // Persistence is optional when storage is blocked or its quota is full.
  }
}

/**
 * 解析并校验存储值，补写版本封装；无效数据会被移除并回退到默认值。
 * @param storage 待读取的浏览器存储。
 * @param key 业务值对应的存储键。
 * @param fallback 读取或校验失败时使用的业务默认值。
 * @param guard 校验解析结果是否符合业务数据结构。
 * @returns 通过类型守卫的业务值；缺失、读取失败或校验失败时使用默认值。
 * @typeParam T 默认值及校验成功后业务值的类型。
 */
export function readStoredJson<T>(
  storage: Storage,
  key: string,
  fallback: T,
  guard: ValueGuard<T>
): T {
  try {
    const serialized = storage.getItem(key)
    if (!serialized) return fallback
    const parsed: unknown = JSON.parse(serialized)
    const candidate = isStoredValue(parsed) ? parsed.value : parsed
    if (!guard(candidate)) throw new Error('Invalid stored value')
    if (!isStoredValue(parsed) || parsed.version !== STORAGE_VERSION) {
      writeStoredJson(storage, key, candidate)
    }
    return candidate
  } catch {
    removeStoredValue(storage, key)
    return fallback
  }
}

/**
 * 将业务值连同当前存储格式版本序列化保存；存储不可用时忽略失败。
 * @param storage 待写入的浏览器存储。
 * @param key 业务值对应的存储键。
 * @param value 待保存的业务值。
 * @typeParam T 序列化保存的业务值类型。
 */
export function writeStoredJson<T>(
  storage: Storage,
  key: string,
  value: T
): void {
  try {
    storage.setItem(key, JSON.stringify({ version: STORAGE_VERSION, value }))
  } catch {
    // Persistence is optional when storage is blocked or its quota is full.
  }
}

