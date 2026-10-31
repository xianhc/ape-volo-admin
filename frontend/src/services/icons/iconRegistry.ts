import { loadIcon } from '@iconify/vue'
import type { IconifyIcon } from '@iconify/vue'
import { commonIcons, collections } from 'virtual:app-icons'

export const iconCollections = [
  { prefix: 'lucide', label: 'Lucide' },
  { prefix: 'mdi', label: 'Material Design Icons' },
  { prefix: 'ant-design', label: 'Ant Design' },
  { prefix: 'mdi-light', label: 'Material Design Light' },
  { prefix: 'ep', label: 'Element Plus' }
] as const

const localModules = import.meta.glob<string>('../../assets/icons/**/*.svg', { query: '?raw', import: 'default' })
const localFiles = new Map(Object.entries(localModules).map(([path, loader]) => ['svg:' + path.split('/assets/icons/')[1].replace(/\.svg$/, ''), loader]))
const pending = new Map<string, Promise<IconifyIcon>>()
const aliases: Record<string, string> = { job: 'svg:position', timing: 'svg:scheduled-job' }

function normalizeIconName(name: string): string {
  if (aliases[name]) return aliases[name]
  if (localFiles.has('svg:' + name)) return 'svg:' + name
  if (/^[A-Z][A-Za-z0-9]*$/.test(name)) return 'ep:' + name.replace(/[A-Z]/g, (char, index) => (index ? '-' : '') + char.toLowerCase())
  if (!name.includes(':')) return 'ep:' + name
  return name
}

export async function getIconNames(prefix: string): Promise<string[]> {
  if (prefix === 'svg') return [...localFiles.keys()].sort()
  const loader = collections[prefix]
  if (!loader) return []
  return (await loader()).names.map(name => prefix + ':' + name)
}

export function resolveIcon(name: string): Promise<IconifyIcon> {
  const key = normalizeIconName(name)
  if (!/^[a-z0-9]+(?:-[a-z0-9]+)*:[a-z0-9]+(?:[/-][a-z0-9]+)*$/.test(key)) return Promise.reject(new Error('Invalid icon name'))
  if (commonIcons[key]) return Promise.resolve(commonIcons[key])
  let result = pending.get(key)
  if (!result) {
    result = (async () => {
      const local = localFiles.get(key)
      if (local) {
        const [source, { toIconData }] = await Promise.all([local(), import('./localSvgIcons')])
        return toIconData(source)
      }
      const prefix = key.split(':')[0]
      if (prefix === 'svg') throw new Error('Local SVG not found')
      const loader = collections[prefix]
      if (loader) {
        const data = await (await loader()).getIcon(key.slice(prefix.length + 1))
        if (!data) throw new Error('Icon not found')
        return data
      }
      return loadIcon(key)
    })().catch(error => { pending.delete(key); throw error })
    pending.set(key, result)
  }
  return result
}
