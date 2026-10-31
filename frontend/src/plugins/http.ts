import { ElMessage, ElNotification } from 'element-plus'
import { configureHttpFeedback } from '@/services/http/httpFeedback'
import { resetApplicationSession } from '@/services/session/applicationSession'
import { apiErrorBody } from '@/utils/apiError'
import router from '@/router'

export function setupHttpFeedback(): void {
  configureHttpFeedback({
    error(error) {
      const show = (body: unknown): void => {
        const message = body && typeof body === 'object' && 'message' in body && typeof body.message === 'string' ? body.message : '请求失败，请稍后重试'
        ElMessage.error(message)
      }
      const body = apiErrorBody(error)
      if (body instanceof Blob && body.type.includes('json')) {
        const reader = new FileReader()
        reader.onload = () => {
          try { show(JSON.parse(String(reader.result))) } catch { show(undefined) }
        }
        reader.onerror = () => show(undefined)
        reader.readAsText(body, 'utf-8')
      } else show(body)
    },
    expired() {
      const redirect = router.currentRoute.value.fullPath
      resetApplicationSession()
      ElNotification({ title: '提示', message: '您的会话已过期，请重新登录', type: 'info' })
      if (router.currentRoute.value.name !== 'Login') void router.replace({ name: 'Login', query: { redirect } })
    },
    forbidden() {
      if (router.currentRoute.value.path !== '/403') void router.replace('/403')
    }
  })
}
