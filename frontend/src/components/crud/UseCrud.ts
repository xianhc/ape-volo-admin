import type { Ref, ComputedRef } from 'vue'
import { ref, computed } from 'vue'
import { cloneDeep } from 'lodash-es'
import { ElMessage } from 'element-plus'
import type { FormInstance } from 'element-plus'
import type { AxiosResponse } from 'axios'
import type { EntityId, OperationResult, PagedResultDto, PagedAndSortedResultRequestDto, RequestOptions } from '@/api/types/base'
import { useTableController } from '@/composables/useTableController'
import { useEntityEditor } from '@/composables/useEntityEditor'
import { useMutationAction } from '@/composables/useMutationAction'
import { useExportAction } from '@/composables/useExportAction'
import { confirmAction } from '@/services/feedback/confirmation'
import { provideCrudForm } from './crudContext'
import type { PaginationBindings, SortOrder } from './crud.types'

interface IdentifiedRow { id: EntityId }

export interface CrudMethod<TRow extends IdentifiedRow, TForm extends object, TQuery extends PagedAndSortedResultRequestDto> {
  list: (params: TQuery, options?: RequestOptions) => Promise<AxiosResponse<PagedResultDto<TRow> | TRow[]>>
  add: (data: TForm) => Promise<AxiosResponse<OperationResult>>
  edit: (data: TForm) => Promise<AxiosResponse<OperationResult>>
  del: (ids: EntityId[]) => Promise<AxiosResponse<OperationResult>>
  download?: (params: TQuery) => Promise<AxiosResponse<Blob>>
}

export interface CrudOptions<TRow extends IdentifiedRow, TForm extends object, TQuery extends PagedAndSortedResultRequestDto> {
  crudMethod: CrudMethod<TRow, TForm, TQuery>
  defaultForm: () => TForm
  searchInfo?: Ref<TQuery>
  pageSizeOptions?: number[]
  sortProp?: string
  sortOrder?: SortOrder
  refreshOnInit?: boolean
}

export interface CrudInstance<TRow extends IdentifiedRow, TForm extends object, TQuery extends PagedAndSortedResultRequestDto> {
  data: Ref<TRow[]>
  form: Ref<TForm>
  formTitle: Ref<string>
  pageIndex: Ref<number>
  pageSize: Ref<number>
  total: Ref<number>
  loading: Ref<boolean>
  error: Ref<unknown>
  submitting: Ref<boolean>
  exporting: Ref<boolean>
  dialogVisible: Ref<boolean>
  isEdit: Ref<boolean>
  multipleSelection: Ref<TRow[]>
  sortProp: Ref<string>
  sortOrder: Ref<SortOrder>
  searchToggle: Ref<boolean>
  query: Ref<TQuery>
  pagination: ComputedRef<PaginationBindings>
  refresh: () => Promise<void>
  toAdd: (extra?: Partial<TForm>) => void
  toEdit: (row: TRow) => void
  save: (formData?: TForm) => Promise<void>
  doDelete: (row: TRow) => Promise<void>
  doBatchDelete: () => Promise<void>
  doExport: () => Promise<void>
  onPageChange: (page: number) => Promise<void>
  onSizeChange: (size: number) => Promise<void>
  onSelectionChange: (rows: TRow[]) => void
  resetQuery: () => Promise<void>
  onSortChange: (sort: { prop: string; order: 'ascending' | 'descending' | null }) => Promise<void>
  resetForm: (data?: TForm) => void
  closeDialog: () => void
  getFormTitle: () => string
  validateAndSave: (form?: FormInstance, value?: TForm) => Promise<void>
}

/** 行模型、编辑草稿、查询条件分别推导，长整型主键不转换为 JS number。 */
export function useCrud<
  TRow extends IdentifiedRow,
  TForm extends object,
  TQuery extends PagedAndSortedResultRequestDto = PagedAndSortedResultRequestDto
>(options: CrudOptions<TRow, TForm, TQuery>): CrudInstance<TRow, TForm, TQuery> {
  const { crudMethod } = options
  const query = options.searchInfo ?? ref({}) as Ref<TQuery>
  const defaults = cloneDeep(query.value)
  const table = useTableController<TQuery, TRow>({
    query,
    defaults: () => cloneDeep(defaults),
    list: async (params, requestOptions) => {
      const response = await crudMethod.list(params, requestOptions)
      const payload = response.data
      return { ...response, data: Array.isArray(payload) ? { items: payload, totalCount: payload.length } : payload }
    },
    pageSizes: options.pageSizeOptions ?? [1, 10, 20, 30, 50, 100],
    defaultPageSize: (options.pageSizeOptions ?? [1, 10])[1],
    defaultSort: { prop: options.sortProp ?? 'id', order: options.sortOrder ?? 'descending' },
    refreshOnInit: options.refreshOnInit
  })
  const editor = useEntityEditor<TForm>({
    defaults: options.defaultForm,
    create: async value => { const response = await crudMethod.add(value); ElMessage.success(response.data.message || '新增成功') },
    update: async value => { const response = await crudMethod.edit(value); ElMessage.success(response.data.message || '编辑成功') },
    saved: table.refresh
  })
  const removal = useMutationAction(async (rows: TRow[]) => {
    if (!rows.length) { ElMessage.warning('请先选择要删除的数据'); return false }
    if (!await confirmAction('确认删除选中的' + rows.length + '条数据?', '警告')) return false
    const response = await crudMethod.del(rows.map(row => row.id))
    ElMessage.success(response.data.message || '删除成功')
    await table.refresh()
  })
  const exporting = useExportAction({
    export: params => {
      if (!crudMethod.download) throw new Error('Export is not available for this list')
      return crudMethod.download(params)
    },
    currentQuery: () => table.currentQuery(false)
  })
  const instance: CrudInstance<TRow, TForm, TQuery> = {
    data: table.data, form: editor.form, formTitle: computed(() => editor.title.value),
    pageIndex: table.pageIndex, pageSize: table.pageSize, total: table.total,
    loading: computed(() => table.loading.value || editor.saving.value || removal.pending.value),
    error: table.error, submitting: computed(() => editor.saving.value || removal.pending.value), exporting: exporting.exporting,
    dialogVisible: editor.visible, isEdit: editor.editing, multipleSelection: table.selection,
    sortProp: table.sortProp, sortOrder: table.sortOrder, searchToggle: table.searchVisible, query,
    pagination: table.pagination,
    refresh: table.refresh, toAdd: editor.openCreate,
    toEdit: row => editor.openEdit({ ...options.defaultForm(), ...row }),
    save: async value => { await editor.save(value) },
    doDelete: async row => { await removal.run([row]) },
    doBatchDelete: async () => { await removal.run([...table.selection.value]) },
    doExport: async () => { try { await exporting.run() } catch { /* 请求层反馈错误，导出模块释放状态。 */ } },
    onPageChange: table.pagination.value.onCurrentChange,
    onSizeChange: table.pagination.value.onSizeChange,
    onSelectionChange: table.onSelectionChange, resetQuery: table.resetQuery, onSortChange: table.onSortChange,
    resetForm: editor.reset, closeDialog: editor.close, getFormTitle: () => editor.title.value,
    validateAndSave: async (form, value) => { await editor.validateAndSave(form, value) }
  }
  provideCrudForm({
    form: instance.form, dialogVisible: instance.dialogVisible, loading: instance.loading,
    closeDialog: instance.closeDialog, getFormTitle: instance.getFormTitle, validateAndSave: instance.validateAndSave
  })
  return instance
}