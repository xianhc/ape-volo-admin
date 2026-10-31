import { computed, ref } from 'vue'
import type { Ref } from 'vue'
import { cloneDeep } from 'lodash-es'
import { useMutationAction } from './useMutationAction'

interface ValidationForm { validate: () => Promise<unknown> }

/** 编辑草稿属于表单，不与表格行共享嵌套对象。 */
export function useEntityEditor<T>(options: {
  defaults: () => T
  create: (value: T) => Promise<unknown>
  update: (value: T) => Promise<unknown>
  saved: () => Promise<void>
}) {
  const form = ref(options.defaults()) as Ref<T>
  const visible = ref(false)
  const editing = ref(false)
  const title = computed(() => editing.value ? '编辑' : '新增')
  const mutation = useMutationAction(async (input: { validator?: ValidationForm; value?: T }) => {
    if (input.validator) {
      const valid = await input.validator.validate().catch(() => false)
      if (!valid) return false
    }
    const value = cloneDeep(input.value ?? form.value)
    await (editing.value ? options.update(value) : options.create(value))
    visible.value = false
    editing.value = false
    await options.saved()
  })
  const reset = (value?: T): void => { if (!mutation.pending.value) form.value = value ? cloneDeep(value) : options.defaults() }
  const openCreate = (extra: Partial<T> = {}): void => {
    if (mutation.pending.value) return
    form.value = cloneDeep({ ...options.defaults(), ...extra })
    editing.value = false
    mutation.error.value = null
    visible.value = true
  }
  const openEdit = (value: T): void => {
    if (mutation.pending.value) return
    form.value = cloneDeep(value)
    editing.value = true
    mutation.error.value = null
    visible.value = true
  }
  const close = (): void => { if (!mutation.pending.value) { visible.value = false; editing.value = false; reset() } }
  return {
    form, visible, editing, title, saving: mutation.pending, error: mutation.error,
    openCreate, openEdit, close, reset,
    save: (value?: T) => mutation.run({ value }),
    validateAndSave: (validator?: ValidationForm | null, value?: T) => validator ? mutation.run({ validator, value }) : Promise.resolve(false)
  }
}
