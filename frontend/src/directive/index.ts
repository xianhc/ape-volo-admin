import type { App } from 'vue'
import { hasPerm, hasRole } from './permission'

export function setupDirective(app: App): void {
  app.directive('hasRole', hasRole)
  app.directive('hasPerm', hasPerm)
}
