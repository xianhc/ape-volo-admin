import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { AuthContext, UserInfo } from '@/api/auth.types'
export type { UserInfo } from '@/api/auth.types'

const emptyUser = (): UserInfo => ({ id: '', userName: '', nickName: '', headerImg: '', genderCode: 0, email: '', phone: '', dept: {}, authority: {}, roleCodes: [], authCodes: [] })

/** Store 只保存当前身份，登录、路由和 UI 由应用会话入口协调。 */
export const useUserStore = defineStore('user', () => {
  const userInfo = ref<UserInfo>(emptyUser())
  const setSession = (context: AuthContext): void => { userInfo.value = context.user }
  const ResetUserInfo = (patch: Partial<UserInfo>): void => { userInfo.value = { ...userInfo.value, ...patch } }
  const reset = (): void => { userInfo.value = emptyUser() }
  return { userInfo, setSession, ResetUserInfo, reset }
})
