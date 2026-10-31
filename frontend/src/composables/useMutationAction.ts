import { shallowRef } from 'vue'

/** 在校验和确认阶段就锁定本次写入；失败保留错误，允许显式重试。 */
export function useMutationAction<T>(execute: (input: T) => Promise<void | boolean>) {
  const pending = shallowRef(false)
  const error = shallowRef<unknown>(null)
  const run = async (input: T): Promise<boolean> => {
    if (pending.value) return false
    pending.value = true
    error.value = null
    try { return (await execute(input)) !== false }
    catch (cause) { error.value = cause; return false }
    finally { pending.value = false }
  }
  return { pending, error, run }
}
