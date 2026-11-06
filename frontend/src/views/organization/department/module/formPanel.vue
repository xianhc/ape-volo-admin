<template>
  <el-drawer
    v-model="dialogVisible"
    :size="appStore.drawerSize"
    :before-close="closeDialog"
    :show-close="false"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <span class="text-lg">{{ getFormTitle() }}</span>
        <div>
          <el-button @click="closeDialog">取 消</el-button>
          <el-button type="primary" :loading="loading" @click="handleSave">
            确 定
          </el-button>
        </div>
      </div>
    </template>

    <el-form
      ref="departmentForm"
      :model="form"
      :rules="rules"
      label-width="80px"
    >
      <el-form-item label="部门名称" prop="name">
        <el-input v-model="form.name" />
      </el-form-item>
      <el-form-item label="排序" prop="sort">
        <el-input-number
          v-model.number="form.sort"
          :min="0"
          :max="999"
          controls-position="right"
          style="width: 200px"
        />
      </el-form-item>
      <el-form-item label="状态" prop="enabled">
        <el-radio
          v-for="item in props.statusTypeOption"
          :key="item.id"
          v-model="form.enabled"
          border
          :label="strToBool(String(item.value))"
        >
          {{ item.label }}
        </el-radio>
      </el-form-item>
    </el-form>
  </el-drawer>
</template>
<script setup lang="ts">
  import type { FormInstance } from 'element-plus'
  import { useCrudForm } from '@/components/crud/crudContext'
  import type { EntityDraft } from '@/api/types/base'
  import type { Department } from '@/api/organization/department/types'

  import { useAppStore } from '@/pinia'
  import { ref } from 'vue'
  import { strToBool } from '@/utils/converter'
  import type { DictOption } from '@/composables/useDictionary'

  const appStore = useAppStore()

  const props = defineProps<{
    statusTypeOption: DictOption[]
  }>()

  // 注入crud
  const crud = useCrudForm<EntityDraft<Department>>()
  const {
    form,
    dialogVisible,
    loading,
    closeDialog,
    validateAndSave,
    getFormTitle
  } = crud

  const departmentForm = ref<FormInstance>()
  const rules = ref({
    name: [{ required: true, message: '请输入部门名称', trigger: 'blur' }],
    sort: [{ required: true, message: '请输入序号', trigger: 'blur' }]
  })
  // 验证表单并保存
  const handleSave = () => validateAndSave(departmentForm.value)
</script>
