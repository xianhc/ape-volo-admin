<template>
  <div class="table-managed-view">
    <TablePage
      v-model:search-visible="searchToggle"
      title="文件管理"
      :data="data"
      :columns="tableColumns"
      :loading="loading"
      :error="crud.error.value"
      row-key="id"
      @selection-change="onSelectionChange"
      @sort-change="onSortChange"
      @refresh="refresh"
    >
      <template #search>
        <SearchPanel :model="searchInfo" :loading="loading" @query="refresh" @reset="resetQuery">
          <el-form-item label="文件描述">
            <el-input v-model="searchInfo.description" placeholder="请输入" />
          </el-form-item>
          <template #advanced>
            <DateRangePicker v-model="searchInfo" />
          </template>
        </SearchPanel>
      </template>
      <template #toolbar-left>
        <CrudToolbarActions
          :submitting="crud.submitting.value"
          :exporting="crud.exporting.value"
          :permissions="{
            create: perms.add,
            remove: perms.del,
            export: perms.download
          }"
          :selected-count="crud.multipleSelection.value.length"
          @create="crud.toAdd()"
          @remove="crud.doBatchDelete()"
          @export="crud.doExport()"
        >
          <template #before>
            <el-button
              type="primary"
              :icon="createIconifyIcon('lucide:upload')"
              @click="uploadDialogVisible = true"
            >
              上传
            </el-button>
          </template>
        </CrudToolbarActions>
      </template>
      <template #column-preview="scope">
        <el-image
          lazy
          :src="getBaseUrl() + scope.row.filePath"
          class="file"
          :preview-teleported="true"
          :preview-src-list="[getBaseUrl() + scope.row.filePath]"
        />
      </template>
      <template #column-actions="scope">
        <RowActions>
          <el-button
            v-has-perm="perms.edit"
            :icon="createIconifyIcon('lucide:pencil')"
            type="primary"
            link
            @click="crud.toEdit(scope.row)"
          >
            编辑
          </el-button>
          <el-button
            v-has-perm="perms.del"
            :icon="createIconifyIcon('lucide:trash-2')"
            type="danger"
            link
            @click="crud.doDelete(scope.row)"
          >
            删除
          </el-button>
        </RowActions>
      </template>
      <template #pagination>
        <el-pagination v-bind="pagination" />
      </template>
    </TablePage>

    <!-- 弹窗，内部有 el-upload 和确认按钮 -->
    <el-dialog v-model="uploadDialogVisible" title="文件上传" width="600px">
      <el-upload
        ref="uploadRef"
        v-model:file-list="fileList"
        :action="`${getBaseUrl()}/file-record/create`"
        :headers="headers"
        :show-file-list="true"
        :auto-upload="false"
        multiple
        class="upload-area"
        :before-upload="beforeUpload"
        :on-error="uploadError"
        :on-success="uploadSuccess"
      >
        <el-button type="primary" :icon="createIconifyIcon('lucide:upload')">
          选择文件
        </el-button>
      </el-upload>
      <template #footer>
        <el-button @click="handleCancel">取消</el-button>
        <el-button
          type="primary"
          :loading="uploadLoading"
          :disabled="fileList.length === 0"
          @click="handleConfirmUpload"
        >
          确认上传
        </el-button>
      </template>
    </el-dialog>

    <!--表单渲染-->
    <el-dialog
      v-model="dialogVisible"
      :before-close="closeDialog"
      :show-close="false"
      title="编辑"
      width="600px"
    >
      <el-form
        ref="storageForm"
        :model="form"
        :rules="rules"
        label-width="80px"
      >
        <el-form-item label="文件描述" prop="description">
          <el-input v-model="form.description" placeholder="请输入" />
        </el-form-item>
      </el-form>
      <template #footer>
        <div>
          <el-button @click="closeDialog">取消</el-button>
          <el-button type="primary" :loading="loading" @click="handleSave">
            保存
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
  import { computed, ref } from 'vue'
  import type { UploadInstance, FormInstance } from 'element-plus'
  import { ElMessage } from 'element-plus'
  import { del, edit, get, add, download } from '@/api/system/storage'
  import type {
    Storage,
    StorageQueryParams
  } from '@/api/system/types/storage.types'
  import TablePage from '@/components/crud/TablePage.vue'
  import CrudToolbarActions from '@/components/crud/CrudToolbarActions.vue'
  import RowActions from '@/components/crud/RowActions.vue'
  import DateRangePicker from '@/components/crud/DateRangePicker.vue'
  import SearchPanel from '@/components/crud/SearchPanel.vue'
  import { createIconifyIcon } from '@/components/icons'
  import type { TableColumnSpec } from '@/components/crud/tableColumns'
  import { useCrud } from '@/components/crud/UseCrud'
  import { getBaseUrl } from '@/utils/index'
  import { getApeToken } from '@/services/session/tokenStorage'

  defineOptions({
    name: 'Storage'
  })

  const perms = {
    add: ['sys:storage:add'],
    edit: ['sys:storage:edit'],
    del: ['sys:storage:del'],
    download: ['sys:storage:download']
  }

  const searchInfo = ref<StorageQueryParams>({})

  const crud = useCrud({
    crudMethod: {
      list: get,
      del: del,
      add: (data: any) => add(data),
      edit: (data: any) => edit(data as Partial<Storage>),
      download: download
    },
    defaultForm: () => ({ description: '' }),
    searchInfo
  })
  const {
    data,
    refresh,
    resetQuery,
    searchToggle,
    loading,
    onSelectionChange,
    pagination,
    onSortChange,
    form,
    dialogVisible,
    closeDialog,
    validateAndSave
  } = crud

  const tableColumns = computed<TableColumnSpec<Storage>[]>(() => [
    { key: 'selection', label: '', type: 'selection', width: 55 },
    { key: 'preview', label: '预览图', width: 100, align: 'center' },
    { key: 'description', label: '文件描述', prop: 'description', minWidth: 160 },
    { key: 'contentType', label: '文件类型', prop: 'contentType', sortable: 'custom', minWidth: 160 },
    { key: 'contentTypeName', label: '文件类别', prop: 'contentTypeName', sortable: 'custom', minWidth: 140 },
    { key: 'size', label: '文件大小', prop: 'size', sortable: 'custom', width: 120 },
    { key: 'creatorId', label: '创建人ID', prop: 'creatorId', minWidth: 130 },
    { key: 'creationTime', label: '创建时间', prop: 'creationTime', sortable: 'custom', minWidth: 170 },
    { key: 'actions', label: '操作', width: 180, align: 'center' }
  ])

  const apeToken = getApeToken()
  const headers = ref({
    Authorization: apeToken.tokenType + ' ' + apeToken.accessToken
  })
  const uploadDialogVisible = ref(false)
  const uploadLoading = ref(false)
  const uploadRef = ref<UploadInstance | null>(null)
  const fileList = ref([])

  const handleConfirmUpload = () => {
    uploadLoading.value = true
    uploadRef.value?.submit()
  }

  const handleCancel = () => {
    uploadLoading.value = false
    uploadDialogVisible.value = false
    fileList.value = []
  }

  const beforeUpload = (file: File) => {
    const isTrue = file.size / 1024 / 1024 < 5
    if (!isTrue) {
      uploadLoading.value = false
      ElMessage.error('上传文件大小不能超过5MB!')
    }
    return isTrue
  }

  const uploadSuccess = () => {
    ElMessage({
      type: 'success',
      message: '上传成功'
    })
    uploadLoading.value = false
    uploadDialogVisible.value = false
    fileList.value = []
    refresh()
  }

  const uploadError = (res: any, file: any) => {
    ElMessage.error(`…${res.message}: ${file.name}`)
    uploadLoading.value = false
  }

  const storageForm = ref<FormInstance | null>(null)
  const rules = ref({
    description: [
      { required: true, message: '请输入文件描述', trigger: 'blur' }
    ]
  })
  // 验证表单并保存
  const handleSave = () => validateAndSave(storageForm.value || undefined)
</script>
<style scoped>
  .file {
    width: 60px;
    height: 60px;
    position: relative;
  }

  .upload-area {
    width: 100%;
    margin-bottom: 16px;
  }
</style>
