import type { App } from 'vue'
import { defineAsyncComponent } from 'vue'
import AppIcon from '@/components/AppIcon.vue'

export const setupIcons = (app: App): void => {
  app.component('AppIcon', AppIcon)
  app.component('IconPicker', defineAsyncComponent(() => import('@/components/IconPicker.vue')))
}
