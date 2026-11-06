<template>
  <el-dialog
    :model-value="props.showUpdateEmailPanel"
    title="修改邮箱"
    width="500px"
    @close="clearEmailAndHide"
  >
    <el-form
      ref="emailForm"
      :model="emailModify"
      :rules="rules"
      label-width="80px"
      class="py-4"
    >
      <el-form-item label="邮箱" prop="email">
        <div class="flex gap-4">
          <el-input v-model="emailModify.email" placeholder="请输入" />
          <el-button
            type="primary"
            :disabled="time > 0"
            class="w-32"
            @click="getEmailCode"
          >
            {{ time > 0 ? `${time}s` : '获取验证码' }}
          </el-button>
        </div>
      </el-form-item>
      <el-form-item label="验证码" prop="code">
        <el-input v-model="emailModify.code" placeholder="请输入"> </el-input>
      </el-form-item>
      <el-form-item label="当前密码" prop="pass">
        <el-input
          v-model="emailModify.pass"
          placeholder="请输入"
          show-password
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="handleCancel">取 消</el-button>
        <el-button type="primary" @click="doSubmit">确 定</el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
  import { onBeforeUnmount, reactive, ref } from 'vue'
  import { ElMessage } from 'element-plus'
  import type { FormInstance } from 'element-plus'
  import { editUserEmail } from '@/api/identity/user'
  import { getResetEmailCode } from '@/api/verificationCode'
  import { useUserStore } from '@/pinia/modules/user'
  import { validEmail } from '@/utils/validate'

  interface EmailModifyForm {
    email: string
    code: string
    pass: string
  }

  const userStore = useUserStore()

  const emailForm = ref<FormInstance | null>(null)
  const emailModify = ref<EmailModifyForm>({
    email: '',
    code: '',
    pass: ''
  })
  const time = ref(0)
  let countdownTimer: ReturnType<typeof setInterval> | null = null

  const props = defineProps({
    showUpdateEmailPanel: {
      type: Boolean,
      required: true
    }
  })

  const emit = defineEmits(['update:showUpdateEmailPanel'])

  const handleCancel = () => {
    emit('update:showUpdateEmailPanel', false)
    clearEmail()
  }

  const clearEmailAndHide = () => {
    emit('update:showUpdateEmailPanel', false)
    clearEmail()
  }

  const validMail = (_rule: any, value: any, callback: any) => {
    if (value === '' || value === null) {
      callback(new Error('新邮箱不能为空'))
    } else if (value === userStore.userInfo.email) {
      callback(new Error('新邮箱不能与旧邮箱相同'))
    } else if (validEmail(value)) {
      callback()
    } else {
      callback(new Error('邮箱格式错误'))
    }
  }
  const rules = reactive({
    pass: [{ required: true, message: '当前密码不能为空', trigger: 'blur' }],
    email: [{ required: true, validator: validMail, trigger: 'blur' }],
    code: [{ required: true, message: '验证码不能为空', trigger: 'blur' }]
  })

  const clearEmail = () => {
    if (countdownTimer) {
      clearInterval(countdownTimer)
      countdownTimer = null
    }
    time.value = 0
    emailModify.value.email = ''
    emailModify.value.code = ''
    emailModify.value.pass = ''
    emailForm.value?.clearValidate()
  }

  const getEmailCode = async () => {
    getResetEmailCode({
      email: emailModify.value.email
    }).then((res) => {
      ElMessage.success((res.data as any)?.message || '验证码已发送')
      time.value = 60
      if (countdownTimer) clearInterval(countdownTimer)
      countdownTimer = setInterval(() => {
        time.value--
        if (time.value <= 0 && countdownTimer) {
          clearInterval(countdownTimer)
          countdownTimer = null
        }
      }, 1000)
    })
  }
  const doSubmit = async () => {
    emailForm.value?.validate((valid: boolean) => {
      if (valid) {
        editUserEmail({
          email: emailModify.value.email,
          code: emailModify.value.code,
          pass: emailModify.value.pass
        }).then(() => {
          userStore.ResetUserInfo({
            email: emailModify.value.email
          })
          ElMessage.success('更新邮箱成功')
          emit('update:showUpdateEmailPanel', false)
        })
      }
    })
  }

  onBeforeUnmount(() => {
    if (countdownTimer) clearInterval(countdownTimer)
  })
</script>
