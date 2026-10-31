declare module 'nprogress' {
  const NProgress: any
  export default NProgress
}

declare module 'vue-cropper'

declare module '*.pem?raw' {
  const content: string
  export default content
}

declare module 'virtual:route-component-names' {
  export const componentNames: Record<string, string>
}

declare module 'virtual:app-icons' {
  import type { IconifyIcon } from '@iconify/vue'
  export const commonIcons: Record<string, IconifyIcon>
  export const collections: Record<
    string,
    () => Promise<{
      names: string[]
      getIcon: (name: string) => Promise<IconifyIcon | undefined>
    }>
  >
}
