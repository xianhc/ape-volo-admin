import type { App } from 'vue'
import ElementPlus from 'element-plus'
import { setupDirective } from '@/directive'
import { setupRouter } from '@/router'
import { setupStore } from '@/pinia'
import { setupIcons } from './icons'
import { setupPermission } from './permission'
import { setupHttpFeedback } from './http'
import { setupDiagnostics } from './diagnostics'

export default {
  install(app: App): void {
    // Element Plus
    app.use(ElementPlus)
    // 仓储
    setupStore(app)
    setupDiagnostics(app)
    setupDirective(app)
    setupHttpFeedback()
    // Icon
    setupIcons(app)
    // 路由守卫
    setupPermission()
    setupRouter(app)
  }
}
