import { createPinia } from 'pinia'
import type { App } from 'vue'
import { useAppStore } from '@/pinia/modules/app'
import { useUserStore } from '@/pinia/modules/user'

const store = createPinia()

export function setupStore(app: App<Element>): void {
  app.use(store)
}

export { store, useAppStore, useUserStore }
