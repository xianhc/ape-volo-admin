import type { Component } from 'vue'
import IframeView from '@/components/IframeView.vue'
import { componentNames } from 'virtual:route-component-names'
import { compileMenuRoutes } from '@/services/navigation/menuRoutes'
import type { RouteMenuItem } from '@/services/navigation/menuRoutes'

const viewModules = import.meta.glob('../views/**/*.vue') as Record<string, () => Promise<Component>>
const modules = Object.fromEntries(Object.entries(viewModules).map(([path, loader]) => [path.replace('../', '/src/'), loader]))

export const prepareMenuRoutes = (menus: readonly RouteMenuItem[]) => compileMenuRoutes(menus, {
  modules, names: componentNames, iframe: IframeView
})
