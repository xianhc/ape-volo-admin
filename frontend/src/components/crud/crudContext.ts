import { inject, provide } from 'vue'
import type { InjectionKey, Ref } from 'vue'
import type { FormInstance } from 'element-plus'

/** 表单只依赖编辑状态和保存操作，不接触表格、分页及删除状态。 */
export interface CrudFormContext<TForm> {
  form: Ref<TForm>
  dialogVisible: Ref<boolean>
  loading: Ref<boolean>
  closeDialog: () => void
  getFormTitle: () => string
  validateAndSave: (form?: FormInstance, value?: TForm) => Promise<void>
}

const formKey: InjectionKey<CrudFormContext<unknown>> = Symbol('crud-form')

export function provideCrudForm<TForm>(context: CrudFormContext<TForm>): void {
  provide(formKey, context as CrudFormContext<unknown>)
}

/** 泛型断言只放在注入入口；业务表单必须声明自己的模型。 */
export function useCrudForm<TForm>(): CrudFormContext<TForm> {
  const context = inject(formKey)
  if (!context) throw new Error('CRUD form must be rendered inside its CRUD provider')
  return context as CrudFormContext<TForm>
}
