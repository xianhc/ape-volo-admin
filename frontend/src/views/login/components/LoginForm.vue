<script setup lang="ts">
  import { computed, onUnmounted, reactive, shallowRef } from 'vue'
  import { ElMessage, ElNotification, type FormInstance } from 'element-plus'
  import { getLoginCaptcha } from '@/api/verificationCode'
  import { signIn } from '@/services/session/applicationSession'
  import { httpFeedback } from '@/services/http/httpFeedback'
  import { useRouter, useRoute } from 'vue-router'
  import setting from '@/setting'

  const router = useRouter()
  const route = useRoute()
  /** 验证码策略加载成功前不允许提交登录请求。 */
  const captchaLoaded = shallowRef(false)
  /** 当前验证码策略是否加载失败，用于显示局部重试入口。 */
  const captchaError = shallowRef(false)
  /** 表单验证实例，不通过组件公开暴露认证数据。 */
  const loginForm = shallowRef<FormInstance | null>(null)
  /** 服务端返回的验证码图片，策略关闭验证码时为空。 */
  const picPath = shallowRef('')
  /** 表单验证或认证提交期间的防重复提交标志。 */
  const loading = shallowRef(false)
  /** 组件是否已卸载，阻止迟到的租户请求继续更新界面。 */
  let disposed = false
  /** 验证码请求序号，只有最新请求可以更新挑战数据。 */
  let captchaSequence = 0
  /** 当前页面拥有的登录输入，密码不写入浏览器偏好存储。 */
  const loginFormData = reactive({
    userName: 'apevolo',
    password: '',
    captcha: '',
    captchaId: '',
    showCaptcha: false
  })

  /**
   * 重新加载验证码策略及挑战，忽略卸载前或旧请求的迟到响应。
   * @returns 最新请求完成策略更新或失败提示后结束。
   */
  async function loginVerify(): Promise<void> {
    const sequence = ++captchaSequence
    captchaLoaded.value = false
    captchaError.value = false
    try {
      const response = await getLoginCaptcha()
      if (sequence !== captchaSequence) return
      picPath.value = response.data.img ?? ''
      loginFormData.captchaId = response.data.captchaId ?? ''
      loginFormData.showCaptcha = response.data.showCaptcha
      loginFormData.captcha = ''
      loginForm.value?.clearValidate('captcha')
      captchaLoaded.value = true
    } catch {
      if (sequence === captchaSequence) captchaError.value = true
    }
  }

  /**
   * 校验当前输入并通过统一会话入口提交认证；失败后刷新验证码挑战。
   * @returns 表单校验和本次提交完成后结束，不返回或记录认证凭据。
   */
  async function submitForm(): Promise<void> {
    if (loading.value || !captchaLoaded.value || !loginForm.value) return
    loading.value = true
    try {
      const isValid = await loginForm.value.validate().catch(() => false)
      if (!isValid) {
        ElMessage({ type: 'error', message: '请完善所有登录信息后再提交', showClose: true })
        await loginVerify()
        return
      }
      await signIn(
        loginFormData.userName,
        loginFormData.password,
        loginFormData.captcha,
        loginFormData.captchaId
      )
      const redirect = route.query.redirect
      if (typeof redirect === 'string' && redirect.startsWith('/') && !redirect.startsWith('//')) {
        await router.replace(redirect)
      } else if (router.hasRoute(setting.defaultRouter)) {
        await router.replace({ name: setting.defaultRouter })
      } else {
        await router.replace('/403')
      }
      ElNotification({ title: '登录成功', message: '欢迎回来:' + loginFormData.userName, type: 'success' })
    } catch (error) {
      httpFeedback.error(error)
      if (!disposed) await loginVerify()
    } finally {
      loading.value = false
    }
  }

  /** 与当前语言和验证码启用策略同步的必填校验规则。 */
  const rules = computed(() => ({
    userName: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
    password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
    captcha: [{ required: loginFormData.showCaptcha, message: '请输入验证码', trigger: 'blur' }]
  }))

  onUnmounted(() => {
    disposed = true
    captchaSequence++
  })
  void loginVerify()
</script>

<template>
  <div class="login-form-content">
    <div class="login-heading">
      <h1 id="login-heading">{{ '欢迎回来' }}</h1>
      <p>{{ '请输入您的帐户信息以开始管理您的项目' }}</p>
    </div>
    <div v-if="captchaError" class="captcha-error" role="alert">
      <el-alert :title="'验证码加载失败'" type="error" :closable="false" />
      <el-button @click="loginVerify">{{ '重试' }}</el-button>
    </div>
    <el-form ref="loginForm" class="login-form" :model="loginFormData" :rules="rules" :validate-on-rule-change="false" label-position="top" @submit.prevent="submitForm">
      <el-form-item :label="'用户名'" prop="userName">
        <el-input v-model="loginFormData.userName" name="username" size="large" autocomplete="username" :aria-label="'用户名'" :placeholder="'请输入用户名'" :disabled="loading" />
      </el-form-item>
      <el-form-item :label="'密码'" prop="password">
        <el-input v-model="loginFormData.password" name="password" show-password size="large" type="password" autocomplete="current-password" :aria-label="'密码'" :placeholder="'请输入密码'" :disabled="loading" />
      </el-form-item>
      <el-form-item v-if="loginFormData.showCaptcha" :label="'验证码'" prop="captcha">
        <div class="captcha-row">
          <el-input v-model="loginFormData.captcha" name="captcha" :aria-label="'验证码'" :placeholder="'请输入验证码'" size="large" autocomplete="off" :disabled="loading" />
          <button type="button" class="captcha-image" :aria-label="'重试'" :disabled="loading || !captchaLoaded" @click="loginVerify">
            <img v-if="picPath" :src="picPath" :alt="'请输入验证码'" />
          </button>
        </div>
      </el-form-item>
      <el-form-item class="login-submit">
        <el-button native-type="submit" type="primary" size="large" :loading="loading" :disabled="loading || !captchaLoaded">{{ '登录' }}</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<style scoped>
  .login-heading { margin-bottom: 28px; }
  .login-heading h1 { margin: 0 0 12px; font-size: 35px; font-weight: 650; line-height: 1.25; letter-spacing: -.7px; }
  .login-heading p { margin: 0; color: var(--login-muted); font-size: 12px; line-height: 1.8; }
  .login-form { width: 100%; }
  .login-form :deep(.el-form-item) { margin-bottom: 18px; }
  .login-form :deep(.el-form-item__label) { position: absolute; width: 1px; height: 1px; margin: 0; padding: 0; overflow: hidden; clip-path: inset(50%); white-space: nowrap; }
  .login-form :deep(.el-select) { width: 100%; }
  .login-form :deep(.el-input__wrapper), .login-form :deep(.el-select__wrapper) { min-height: 42px; border-radius: 6px; background: var(--login-field); box-shadow: 0 0 0 1px var(--login-border) inset; }
  .login-form :deep(.el-input__inner), .login-form :deep(.el-select__selected-item) { font-size: 12px; color: var(--login-ink); }
  .login-form :deep(.el-input__inner::placeholder), .login-form :deep(.el-select__placeholder.is-transparent) { color: var(--login-muted); }
  .login-form :deep(.el-input__wrapper.is-focus), .login-form :deep(.el-select__wrapper.is-focused) { box-shadow: 0 0 0 1px var(--el-color-primary) inset, 0 0 0 3px color-mix(in srgb, var(--el-color-primary) 8%, transparent); }
  .captcha-row { display: grid; grid-template-columns: minmax(0, 1fr) 114px; gap: 12px; width: 100%; }
  .captcha-image { display: grid; place-items: center; width: 114px; height: 42px; padding: 0; overflow: hidden; border: 1px solid var(--login-border); border-radius: 6px; background: #e1eaf8; cursor: pointer; }
  .captcha-image img { width: 100%; height: 100%; object-fit: contain; }
  .captcha-image:disabled { cursor: wait; opacity: .6; }
  .captcha-image:focus-visible { outline: 2px solid var(--el-color-primary); outline-offset: 3px; }
  .login-form :deep(.login-submit) { margin-top: 6px; margin-bottom: 0; }
  .login-submit :deep(.el-button) { width: 100%; height: 40px; border-radius: 6px; color: var(--app-primary-on); font-size: 13px; font-weight: 500; box-shadow: 0 2px 5px color-mix(in srgb, var(--el-color-primary) 18%, transparent); }
  .captcha-error { display: grid; gap: 12px; margin-bottom: 20px; }
  .captcha-error > .el-button { justify-self: start; }
  @media (max-height: 790px) and (min-width: 768px), (max-width: 767px) and (max-height: 740px) {
    .login-heading { margin-bottom: 20px; }
    .login-form :deep(.el-form-item) { margin-bottom: 16px; }
    .login-form :deep(.login-submit) { margin-bottom: 0; }
  }
</style>
