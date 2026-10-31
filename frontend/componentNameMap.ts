import fs from 'node:fs'
import path from 'node:path'
import type { Plugin } from 'vite'
import { parse } from '@vue/compiler-sfc'
import ts from 'typescript'

const publicId = 'virtual:route-component-names'
const resolvedId = '\0' + publicId

const filesIn = (directory: string): string[] => fs.readdirSync(directory, { withFileTypes: true }).flatMap(entry => {
  const file = path.join(directory, entry.name)
  return entry.isDirectory() ? filesIn(file) : file.endsWith('.vue') ? [file] : []
})

function componentName(file: string): string {
  const { descriptor } = parse(fs.readFileSync(file, 'utf8'))
  const script = [descriptor.script?.content, descriptor.scriptSetup?.content].filter(Boolean).join('\n')
  const source = ts.createSourceFile(file, script, ts.ScriptTarget.Latest, true)
  let name: string | undefined
  const visit = (node: ts.Node): void => {
    if (ts.isCallExpression(node) && node.expression.getText(source) === 'defineOptions') {
      const options = node.arguments[0]
      if (options && ts.isObjectLiteralExpression(options)) {
        const property = options.properties.find(item => ts.isPropertyAssignment(item) && item.name.getText(source).replace(/['"]/g, '') === 'name')
        if (property && ts.isPropertyAssignment(property) && ts.isStringLiteral(property.initializer)) name = property.initializer.text
      }
    }
    ts.forEachChild(node, visit)
  }
  visit(source)
  return name ?? path.basename(file, '.vue').replace(/(^\w|-\w)/g, word => word.replace('-', '').toUpperCase())
}

/** 通过虚拟模块提供缓存组件名，不向源码目录写入构建产物。 */
export default function vueFilePathPlugin(): Plugin {
  let root = process.cwd()
  return {
    name: 'vue-route-component-names',
    configResolved(config) { root = config.root },
    resolveId(id) { if (id === publicId) return resolvedId },
    load(id) {
      if (id !== resolvedId) return
      const names = Object.fromEntries(filesIn(path.join(root, 'src/views')).map(file => ['/' + path.relative(root, file).replace(/\\/g, '/'), componentName(file)]))
      return 'export const componentNames = ' + JSON.stringify(names)
    },
    configureServer(next) {
      const changed = (_event: string, file: string): void => {
        if (!file.replace(/\\/g, '/').includes('/src/views/') || !file.endsWith('.vue')) return
        const module = next.moduleGraph.getModuleById(resolvedId)
        if (module) next.moduleGraph.invalidateModule(module)
        next.ws.send({ type: 'full-reload' })
      }
      next.watcher.on('all', changed)
      next.httpServer?.once('close', () => next.watcher.off('all', changed))
    }
  }
}
