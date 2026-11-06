import { getUserInfo, login, logout } from '@/api/auth'
import type { AuthContext } from '@/api/auth.types'
import { build } from '@/api/identity/menu'
import { useUserStore } from '@/pinia/modules/user'
import { useRouterStore } from '@/pinia/modules/router'
import { useDictionaryStore } from '@/pinia/modules/dictionary'
import { useScreenLockStore } from '@/pinia/modules/screenLock'
import router from '@/router'
import { clearToken, getApeToken, setApeToken } from '@/services/session/tokenStorage'
import { removeStoredValue } from '@/services/storage/appStorage'
import { createSessionInitializer } from './sessionInitializer'
import { createRouteInstaller } from '../navigation/menuRoutes'
import { prepareMenuRoutes } from '@/utils/dynamicRouter'

const routes = createRouteInstaller(router)
const session = createSessionInitializer<{
  context: AuthContext
  menu: ReturnType<typeof prepareMenuRoutes>
}, AuthContext>({
  identity: () => getApeToken().sessionId,
  load: async (seed) => {
    const [context, menus] = await Promise.all([seed ?? getUserInfo().then(result => result.data), build()])
    return { context, menu: prepareMenuRoutes(menus.data) }
  },
  apply: ({ context, menu }) => {
    routes.replace(menu.records)
    useRouterStore().install(menu)
    useUserStore().setSession(context)
  },
  reset: () => {
    routes.reset()
    useRouterStore().reset()
    useUserStore().reset()
    useDictionaryStore().reset()
    useScreenLockStore().reset()
  }
})

export const initializeSession = session.initialize

export function resetApplicationSession(): void {
  session.reset()
  for (const key of ['historys', 'activeValue', 'topActive', 'needCloseAll', 'role-page-context', 'screen-lock']) removeStoredValue(sessionStorage, key)
}

export async function signIn(username: string, password: string, captcha: string, captchaId: string): Promise<AuthContext> {
  const response = await login(username, password, captcha, captchaId)
  resetApplicationSession()
  setApeToken(response.data.token)
  await initializeSession(response.data)
  return response.data
}

let signingOut: Promise<void> | undefined
export function signOut(): Promise<void> {
  if (signingOut) return signingOut
  const identity = getApeToken().sessionId
  const pending = logout().then(() => {}).catch(() => {
    // 服务端不可用仍结束当前本地会话。
  }).finally(async () => {
    if (getApeToken().sessionId !== identity) return
    clearToken()
    resetApplicationSession()
    await router.replace({ name: 'Login' })
  }).finally(() => { signingOut = undefined })
  signingOut = pending
  return pending
}
