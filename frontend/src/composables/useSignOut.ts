/**
 * 退出登录组合式函数。
 */
import { ref } from 'vue'
import { ElMessageBox } from 'element-plus'
import { signOut as endSession } from '@/services/session/applicationSession'

/**
 * 协调退出登录、凭据清理及退出期间的重复点击保护。
 * @returns 退出中的响应式状态和 signOut 操作。
 */
export function useSignOut() {
  const signingOut = ref(false)

  /**
   * 按当前会话协调规则执行退出登录。
   */
  async function signOut(): Promise<void> {
    if (signingOut.value) return
    signingOut.value = true
    try {
      await ElMessageBox.confirm('确定退出当前登录会话吗？', '退出登录', {
        confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning', modalClass: 'sign-out-confirmation'
      })
      await endSession()
    } catch (error) {
      if (error !== 'cancel' && error !== 'close') throw error
    } finally {
      signingOut.value = false
    }
  }

  return { signingOut, signOut }
}
