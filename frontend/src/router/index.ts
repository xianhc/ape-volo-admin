import {
  createRouter,
  createWebHashHistory,
  Router,
  RouteRecordRaw
} from 'vue-router'
import type { App } from 'vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/login/index.vue')
  },
  {
    path: '/403',
    name: 'Forbidden',
    meta: {
      closeTab: true
    },
    component: () => import('@/views/error/403.vue')
  },
  {
    path: '/:catchAll(.*)',
    meta: {
      closeTab: true
    },
    component: () => import('@/views/error/index.vue')
  }
]

const router: Router = createRouter({
  history: createWebHashHistory(),
  routes
})

export function setupRouter(app: App<Element>): void {
  app.use(router)
}

export default router
