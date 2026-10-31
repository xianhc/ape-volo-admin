<template>
  <el-dialog
    v-model="dialogVisible"
    :before-close="closeDialog"
    :show-close="false"
    :title="getFormTitle()"
    width="600px"
  >
    <el-form ref="dictForm" :model="form" :rules="rules" label-width="80px">
      <el-form-item label="字典类型" prop="dictType">
        <el-radio
          v-for="item in props.dictTypeOption"
          :key="item.id"
          v-model="form.dictType"
          border
          :label="Number(item.value)"
        >
          {{ item.label }}
        </el-radio>
      </el-form-item>
      <el-form-item label="字典名称" prop="name">
        <el-input v-model="form.name" />
      </el-form-item>
      <el-form-item label="描述信息" prop="description">
        <el-input
          v-model="form.description"
          placeholder="请输入"
          :rows="5"
          type="textarea"
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
  import type { Dictionary } from '@/api/system/types/dictionary.types'

  import { ref } from 'vue'
  import type { DictOption } from '@/composables/useDictionary'

  const props = defineProps<{
    dictTypeOption: DictOption[]
  }>()

  // 注入crud
  const crud = useCrudForm<EntityDraft<Dictionary>>()
  const {
    form,
    dialogVisible,
    loading,
    closeDialog,
    validateAndSave,
    getFormTitle
  } = crud

  const dictForm = ref<FormInstance>()
  const rules = ref({
    dictType: [{ required: true, message: '请选择字典类型', trigger: 'blur' }],
    name: [{ required: true, message: '请输入字典名称', trigger: 'blur' }]
  })
  // 验证表单并保存
  const handleSave = () => validateAndSave(dictForm.value)
</script>
