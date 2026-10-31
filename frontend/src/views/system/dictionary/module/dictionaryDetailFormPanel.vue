<template>
  <el-dialog
    v-model="dialogVisible"
    :before-close="closeDialog"
    :show-close="false"
    :title="getFormTitle()"
    width="600px"
  >
    <el-form
      ref="dictDetailForm"
      :model="form"
      :rules="rules"
      label-width="80px"
    >
      <el-form-item label="字典名称">
        <el-input :value="props.dictName" :disabled="true" />
      </el-form-item>
      <el-form-item label="标签名" prop="label">
        <el-input v-model="form.label" />
      </el-form-item>
      <el-form-item label="数据值" prop="value">
        <el-input v-model="form.value" />
      </el-form-item>
      <el-form-item label="排序" prop="dictSort">
        <el-input-number
          v-model.number="form.dictSort"
          :min="0"
          :max="999"
          controls-position="right"
          style="width: 200px"
        />
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
</template>
<script setup lang="ts">
  import type { FormInstance } from 'element-plus'
  import { useCrudForm } from '@/components/crud/crudContext'
  import type { EntityDraft } from '@/api/types/base'
  import type { DictionaryDetail } from '@/api/system/types/dictionaryDetail.types'

  import { ref } from 'vue'

  const props = defineProps({
    dictName: {
      type: String,
      default: ''
    }
  })

  // 注入crud
  const crud = useCrudForm<EntityDraft<DictionaryDetail>>()
  const {
    form,
    dialogVisible,
    loading,
    closeDialog,
    validateAndSave,
    getFormTitle
  } = crud

  const dictDetailForm = ref<FormInstance>()
  const rules = ref({
    label: [{ required: true, message: '请输入标签名', trigger: 'blur' }],
    value: [{ required: true, message: '请输入数据值', trigger: 'blur' }],
    dictSort: [{ required: true, message: '请输入排序', trigger: 'blur' }]
  })
  // 验证表单并保存
  const handleSave = () => validateAndSave(dictDetailForm.value)
</script>
