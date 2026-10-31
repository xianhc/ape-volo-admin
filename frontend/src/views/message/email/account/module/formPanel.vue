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
          <el-button @click="closeDialog">取消</el-button>
          <el-button type="primary" :loading="loading" @click="handleSave">
            保存
          </el-button>
        </div>
      </div>
    </template>

    <el-form
      ref="emailAccountForm"
      :model="form"
      :rules="rules"
      :inline="true"
      label-width="120px"
      label-position="right"
      class="grid grid-cols-2"
    >
      <el-form-item label="邮箱账户" prop="email">
        <el-input v-model="form.email" placeholder="请输入" />
      </el-form-item>
      <el-form-item label="显示名称" prop="displayName">
        <el-input v-model="form.displayName" placeholder="请输入" />
      </el-form-item>
      <el-form-item label="主机" prop="host">
        <el-input v-model="form.host" placeholder="请输入" />
      </el-form-item>
      <el-form-item label="端口" prop="port">
        <el-input
          v-model="form.port"
          oninput="value=value.replace(/[^0-9]/g,'')"
          placeholder="请输入"
        />
      </el-form-item>
      <el-form-item label="用户名称" prop="userName">
        <el-input v-model="form.userName" placeholder="请输入" />
      </el-form-item>
      <el-form-item label="密码" prop="password">
        <el-input
          v-model="form.password"
          type="password"
          placeholder="请输入"
        />
      </el-form-item>
      <el-form-item label="Ssl" prop="enableSsl">
        <el-radio-group v-model="form.enableSsl">
          <el-radio border :value="true">是</el-radio>
          <el-radio border :value="false">否</el-radio>
        </el-radio-group>
      </el-form-item>
      <el-form-item label="发送凭证" prop="useDefaultCredentials">
        <el-radio-group v-model="form.useDefaultCredentials">
          <el-radio border :value="true">是</el-radio>
          <el-radio border :value="false">否</el-radio>
        </el-radio-group>
      </el-form-item>
    </el-form>
  </el-drawer>
</template>
<script setup lang="ts">
  import type { FormInstance } from 'element-plus'
  import { useCrudForm } from '@/components/crud/crudContext'
  import type { EntityDraft } from '@/api/types/base'
  import type { EmailAccount } from '@/api/message/email/emailAccount'

  import { ref } from 'vue'
  import { useAppStore } from '@/pinia'

  const appStore = useAppStore()

  // 注入crud
  const crud = useCrudForm<EntityDraft<EmailAccount>>()
  const {
    form,
    dialogVisible,
    loading,
    closeDialog,
    validateAndSave,
    getFormTitle
  } = crud

  const emailAccountForm = ref<FormInstance>()
  const rules = ref({
    email: [
      { required: true, message: '请输入邮箱账户', trigger: 'blur' },
      { type: 'email', message: '请输入正确的邮箱账户', trigger: 'blur' }
    ],
    displayName: [
      { required: true, message: '请输入邮箱显示名称', trigger: 'blur' }
    ],
    host: [{ required: true, message: '请输入主机', trigger: 'blur' }],
    port: [{ required: true, message: '请输入端口号', trigger: 'blur' }],
    username: [
      { required: true, message: '请输入邮箱用户名', trigger: 'blur' }
    ],
    password: [{ required: true, message: '请输入邮箱密码', trigger: 'blur' }]
  })

  // 验证表单并保存
  const handleSave = () => validateAndSave(emailAccountForm.value)
</script>
