import fs from 'node:fs'
import path from 'node:path'
import { createRequire } from 'node:module'
import { addCollection, getIcon } from '@iconify/vue'
import type { IconifyIcon, IconifyJSON } from '@iconify/vue'
import type { Plugin } from 'vite'

const require = createRequire(import.meta.url)
const prefixes = ['lucide', 'mdi', 'ant-design', 'mdi-light', 'ep'] as const
const rootId = 'virtual:app-icons'
const shardSize = 128
const sourceFiles = (directory: string): string[] => fs.readdirSync(directory, { withFileTypes: true }).flatMap(entry => {
  const file = path.join(directory, entry.name)
  return entry.isDirectory() ? sourceFiles(file) : /\.(vue|ts)$/.test(file) ? [file] : []
})

/** 静态引用直接打包；动态菜单按小批量图标加载，选择器只先加载名称索引。 */
export default function iconAssetsPlugin(): Plugin {
  let root = process.cwd()
  const sets = new Map<string, { names: string[]; visibleNames: string[]; icons: Record<string, IconifyIcon> }>()
  const getSet = (prefix: string) => {
    let set = sets.get(prefix)
    if (!set) {
      const data = JSON.parse(fs.readFileSync(require.resolve(`@iconify-json/${prefix}/icons.json`), 'utf8')) as IconifyJSON
      addCollection(data)
      const names = [...Object.keys(data.icons), ...Object.keys(data.aliases ?? {})].sort()
      const icons: Record<string, IconifyIcon> = {}
      for (const name of names) {
        const icon = getIcon(`${prefix}:${name}`)
        if (icon) icons[name] = icon
      }
      set = {
        names: names.filter(name => icons[name]),
        visibleNames: names.filter(name => icons[name] && !(data.icons[name] ?? data.aliases?.[name])?.hidden),
        icons
      }
      sets.set(prefix, set)
    }
    return set
  }
  return {
    name: 'app-icon-assets',
    configResolved(config) { root = config.root },
    resolveId(id) {
      if (id === rootId || id.startsWith('virtual:app-icon-index/') || id.startsWith('virtual:app-icon-shard/')) return '\0' + id
    },
    load(id) {
      if (id === '\0' + rootId) {
        const common: Record<string, IconifyIcon> = {}
        for (const file of sourceFiles(path.join(root, 'src'))) {
          const source = fs.readFileSync(file, 'utf8')
          for (const match of source.matchAll(/['"]((lucide|ep|mdi|mdi-light|ant-design):([a-z0-9-]+))['"]/g)) {
            const icon = getSet(match[2]).icons[match[3]]
            if (!icon) throw new Error(`Unknown static icon ${match[1]} in ${path.relative(root, file)}`)
            common[match[1]] = icon
          }
        }
        return `export const commonIcons = ${JSON.stringify(common)};\nexport const collections = {${prefixes.map(prefix => `${JSON.stringify(prefix)}: () => import(${JSON.stringify('virtual:app-icon-index/' + prefix)})`).join(',')}};`
      }
      if (id.startsWith('\0virtual:app-icon-index/')) {
        const prefix = id.split('/').pop()!
        const { names, visibleNames } = getSet(prefix)
        const visible = new Set(visibleNames)
        const hiddenNames = names.filter(name => !visible.has(name))
        const loaders = Array.from({ length: Math.ceil(names.length / shardSize) }, (_, index) => `() => import(${JSON.stringify(`virtual:app-icon-shard/${prefix}/${index}`)})`)
        return `const allNames = ${JSON.stringify(names)};\nconst hidden = new Set(${JSON.stringify(hiddenNames)});\nexport const names = allNames.filter(name => !hidden.has(name));\nconst positions = new Map(allNames.map((name, index) => [name, index]));\nconst shards = [${loaders.join(',')}];\nexport async function getIcon(name) { const index = positions.get(name); if (index === undefined) return; const data = await shards[Math.floor(index / ${shardSize})](); return data.default[name]; }`
      }
      if (id.startsWith('\0virtual:app-icon-shard/')) {
        const [, prefix, index] = id.slice(1).split('/')
        const { names, icons } = getSet(prefix)
        const selected = names.slice(Number(index) * shardSize, (Number(index) + 1) * shardSize)
        return 'export default ' + JSON.stringify(Object.fromEntries(selected.map(name => [name, icons[name]])))
      }
    },
    handleHotUpdate({ file, server }) {
      if (!/\.(vue|ts)$/.test(file)) return
      const module = server.moduleGraph.getModuleById('\0' + rootId)
      if (module) server.moduleGraph.invalidateModule(module)
    }
  }
}
