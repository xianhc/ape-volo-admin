<template>
  <el-drawer
    v-model="dialogVisible"
    :size="1200"
    :before-close="closeDialog"
    :show-close="false"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <span class="text-lg">{{ getFormTitle() }}</span>
        <div>
          <el-button @click="closeDialog">取消</el-button>
          <el-button type="primary" :loading="loading" @click="handleSave">
            保存
          </el-button>
        </div>
      </div>
    </template>

    <el-form
      ref="emailQueuedForm"
      :model="form"
      :rules="rules"
      :inline="true"
      label-width="120px"
      label-position="right"
      class="grid grid-cols-2"
    >
      <el-form-item label="邮件主题" class="col-span-2" prop="subject">
        <el-input v-model="form.subject" placeholder="请输入" />
      </el-form-item>
      <el-form-item label="收件邮箱" prop="to">
        <el-input v-model="form.to" placeholder="请输入" />
      </el-form-item>
      <el-form-item label="收件人名称" prop="toName">
        <el-input v-model="form.toName" placeholder="请输入" />
      </el-form-item>
      <el-form-item label="发送邮箱" prop="emailAccountId">
        <el-select
          v-model="form.emailAccountId"
          style="width: 100%"
          clearable
          filterable
          placeholder="请选择"
        >
          <el-option
            v-for="item in props.emailAccountOption"
            :key="item.id"
            :label="item.displayName"
            :value="item.id"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="优先级" prop="priority">
        <el-select
          v-model="form.priority"
          style="width: 100%"
          clearable
          filterable
          placeholder="请选择"
        >
          <el-option
            v-for="item in props.priorityTypeOption"
            :key="item.value"
            :label="item.label"
            :value="Number(item.value)"
          />
        </el-select>
      </el-form-item>
      <el-form-item class="col-span-2" label="抄送邮箱" prop="bcc">
        <el-input v-model="form.bcc" placeholder="多个邮箱都英文隔开" />
      </el-form-item>
      <el-form-item class="col-span-2" label="邮件内容" prop="body">
        <el-input
          v-model="form.body"
          placeholder="请输入"
          :rows="20"
          type="textarea"
        />
      </el-form-item>
    </el-form>
  </el-drawer>
</template>
<script setup lang="ts">
  import type { FormInstance } from 'element-plus'
  import { useCrudForm } from '@/components/crud/crudContext'
  import type { EntityDraft } from '@/api/types/base'
  import type { EmailQueued } from '@/api/message/email/emailQueued'

  import { ref } from 'vue'
  import type { EmailAccount } from '@/api/message/email/emailAccount'
  import type { DictOption } from '@/composables/useDictionary'

  const props = defineProps<{
    statusTypeOption: DictOption[]
    emailAccountOption: EmailAccount[]
    priorityTypeOption: DictOption[]
  }>()

  // 注入crud
  const crud = useCrudForm<EntityDraft<EmailQueued>>()
  const {
    form,
    dialogVisible,
    loading,
    closeDialog,
    validateAndSave,
    getFormTitle
  } = crud

  const emailQueuedForm = ref<FormInstance>()
  const rules = ref({
    subject: [{ required: true, message: '请输入邮件主题', trigger: 'blur' }],
    to: [{ required: true, message: '请输入收件邮箱', trigger: 'blur' }],
    body: [{ required: true, message: '请输入邮件内容', trigger: 'blur' }],
    emailAccountId: [{ required: true, message: '请选择', trigger: 'blur' }],
    priority: [{ required: true, message: '请选择', trigger: 'blur' }]
  })

  // 验证表单并保存
  const handleSave = () => validateAndSave(emailQueuedForm.value)
</script>
