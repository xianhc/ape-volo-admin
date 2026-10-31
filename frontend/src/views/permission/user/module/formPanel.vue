<template>
  <el-drawer
    :model-value="dialogVisible"
    :size="appStore.drawerSize"
    :before-close="closeDialog"
    :show-close="false"
    :close-on-press-escape="!loading"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <span class="text-lg">{{ getFormTitle() }}</span>
        <div>
          <el-button :disabled="loading" @click="closeDialog">取 消</el-button>
          <el-button type="primary" :loading="loading" @click="handleSave">
            确 定
          </el-button>
        </div>
      </div>
    </template>
    <el-form
      ref="userForm"
      :model="form"
      :rules="rules"
      :inline="true"
      label-width="120px"
      label-position="right"
      class="grid grid-cols-2"
    >
      <el-form-item label="用户名称" prop="userName">
        <el-input v-model="form.userName" />
      </el-form-item>
      <el-form-item label="用户昵称" prop="nickName">
        <el-input v-model="form.nickName" />
      </el-form-item>
      <el-form-item label="电话" prop="phone">
        <el-input v-model="form.phone" />
      </el-form-item>
      <el-form-item label="邮箱" prop="email">
        <el-input v-model="form.email" />
      </el-form-item>
      <el-form-item label="性别">
        <el-radio-group v-model="form.genderCode">
          <el-radio
            v-for="item in props.genderOption"
            :key="item.id"
            border
            :value="Number(item.value)"
          >
            {{ item.label }}
          </el-radio>
        </el-radio-group>
      </el-form-item>
      <el-form-item label="状态">
        <el-radio-group v-model="form.enabled" :disabled="false">
          <el-radio
            v-for="item in props.statusTypeOption"
            :key="item.id"
            border
            :value="strToBool(String(item.value))"
          >
            {{ item.label }}
          </el-radio>
        </el-radio-group>
      </el-form-item>
      <el-form-item prop="department" label="部门">
        <el-cascader
          v-model="form.dept.id"
          style="width: 100%"
          :options="departmentOption"
          :props="{
            checkStrictly: true,
            label: 'name',
            value: 'id',
            emitPath: false
          }"
          :show-all-levels="false"
          filterable
          clearable
        />
      </el-form-item>
      <el-form-item label="角色" prop="roles" class="col-span-2">
        <el-select
          v-model="form.roles"
          :disabled="!userStore.userInfo.roleCodes.includes('admin')"
          style="width: 100%"
          value-key="id"
          multiple
          clearable
          filterable
          collapse-tags
          collapse-tags-tooltip
          :max-collapse-tags="3"
          placeholder="请选择"
        >
          <el-option
            v-for="item in props.roleOption"
            :key="item.id"
            :label="item.name"
            :value="item"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="岗位" prop="positions" class="col-span-2">
        <el-select
          v-model="form.positions"
          style="width: 100%"
          value-key="id"
          clearable
          filterable
          multiple
          collapse-tags
          collapse-tags-tooltip
          :max-collapse-tags="3"
          placeholder="请选择"
        >
          <el-option
            v-for="item in props.positionOption"
            :key="item.id"
            :label="item.name"
            :value="item"
          />
        </el-select>
      </el-form-item>
    </el-form>
  </el-drawer>
</template>
<script setup lang="ts">
  import { useAppStore, useUserStore } from '@/pinia'
  import { ref, watch } from 'vue'
  import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
  import { add, edit } from '@/api/permission/user'
  import type { User, UserDraft } from '@/api/permission/types/user.types'
  import { createUserDraft } from '@/api/permission/userAdapter'
  import { useEntityEditor } from '@/composables/useEntityEditor'
  import { strToBool } from '@/utils/converter'
  import { isvalidPhone } from '@/utils/validate'
  import { type DictOption } from '@/composables/useDictionary'
  import { type Department } from '@/api/permission/department'
  import { type Role } from '@/api/permission/role'
  import { type Position } from '@/api/organization/positions'

  const appStore = useAppStore()
  const userStore = useUserStore()

  const emit = defineEmits<{ 'update:modelValue': [value: boolean]; saved: [] }>()
  const props = defineProps<{
    modelValue: boolean
    user?: User | null
    statusTypeOption: DictOption[]
    genderOption: DictOption[]
    departmentOption: Department[]
    positionOption: Position[]
    roleOption: Role[]
  }>()

  const validPhone = (_rule: unknown, value: string, callback: (error?: Error) => void) => {
    if (value && !isvalidPhone(value)) {
      callback(new Error('请输入正确的11位手机号码'))
    } else {
      callback()
    }
  }

  const rules: FormRules<UserDraft> = {
    userName: [
      { required: true, message: '请输入用户名', trigger: 'blur' },
      {
        min: 2,
        max: 20,
        message: '长度在 2 到 20 个字符',
        trigger: 'blur'
      }
    ],
    nickName: [
      { required: true, message: '请输入用户昵称', trigger: 'blur' },
      {
        min: 2,
        max: 20,
        message: '长度在 2 到 20 个字符',
        trigger: 'blur'
      }
    ],
    email: [
      { required: true, message: '请输入邮箱地址', trigger: 'blur' },
      { type: 'email', message: '请输入正确的邮箱地址', trigger: 'blur' }
    ],
    phone: [{ trigger: 'blur', validator: validPhone }],
    positions: [
      { required: true, message: '请至少选择一个岗位', trigger: 'change' }
    ],
    roles: [
      { required: true, message: '请至少选择一个角色', trigger: 'change' }
    ]
  }

  const editor = useEntityEditor<UserDraft>({
    defaults: () => createUserDraft(),
    create: add,
    update: edit,
    saved: async () => { ElMessage.success('保存成功'); emit('saved') }
  })
  const { form, visible: dialogVisible, saving: loading } = editor
  const closeDialog = editor.close
  const getFormTitle = () => editor.title.value
  const userForm = ref<FormInstance>()
  const handleSave = () => editor.validateAndSave(userForm.value)
  watch(() => props.modelValue, (open) => {
    if (open) {
      if (props.user) editor.openEdit(createUserDraft(props.user))
      else editor.openCreate()
    } else editor.close()
  })
  watch(dialogVisible, open => emit('update:modelValue', open))
</script>
